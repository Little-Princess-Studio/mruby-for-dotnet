namespace MRuby.Library.Language
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Threading;
    using MRuby.Library;

    // Native callback dispatcher (macOS longjmp-over-managed-frame crash fix, managed half).
    //
    // Previously each C# callback was wrapped in its own NativeMethodFunc delegate whose
    // function pointer was handed to mruby; on a C# exception the wrapper called mrb_raise
    // FROM INSIDE the managed frame -> longjmp across the managed frame -> dangling
    // Thread::m_pFrame -> macOS crash under concurrent GC (dotnet/runtime#1445 class).
    //
    // Now mruby only ever holds the address of a single STATIC native trampoline
    // (mrbdotnet_method_trampoline) carrying an integer callbackId in its proc env. When
    // mruby invokes it, native calls back into THIS single managed dispatcher with the
    // callbackId + already-unmarshaled (argc, argv). The dispatcher runs the user callback
    // and RETURNS NORMALLY: on success it returns the result; on a C# exception it writes a
    // message into the native buffer and sets *shouldRaise=1, returning nil. The NATIVE
    // trampoline then performs mrb_exc_raise AFTER this managed frame has popped, so the
    // longjmp originates below the managed frame and never crosses it.
    //
    // Bonus: because mruby now holds a static native function pointer (never a managed
    // delegate pointer), the class-A delegate-collection crash is structurally impossible,
    // so the per-callback RbCallbackKeeper rooting is no longer needed for these paths.
    [ExcludeFromCodeCoverage]
    internal static class RbCallbackDispatch
    {
        // Matches native `typedef uint64_t (*mrbdotnet_dispatch_fn)(mrb_state*, uint64_t self,
        // int64_t callback_id, int64_t argc, const uint64_t* argv, mrb_bool* should_raise,
        // char* msg_buf, int32_t msg_buf_len);`. Pointer params are passed as IntPtr and
        // written through with unsafe casts inside Dispatch (delegate signatures cannot carry
        // raw pointer types).
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate UInt64 ManagedDispatchFunc(
            IntPtr mrb,
            UInt64 self,
            Int64 callbackId,
            Int64 argc,
            IntPtr argv,
            IntPtr shouldRaise,
            IntPtr msgBuf,
            Int32 msgBufLen);

        [DllImport(Ruby.MrubyLib, CharSet = CharSet.Ansi)]
        private static extern void mrbdotnet_set_dispatcher(IntPtr fn);

        // The single process-static dispatcher delegate. Rooted forever (static field +
        // GCHandle) so its function pointer handed to native never dangles.
        private static readonly ManagedDispatchFunc s_dispatcher = Dispatch;
        private static GCHandle s_dispatcherHandle;
        private static int s_registered;

        // Per-state registry: callbackId -> user CSharpMethodFunc. Keyed by the native
        // mrb_state handle so it can be found from the dispatcher (which only has IntPtr mrb)
        // and drained at Ruby.Close. A per-state monotonically increasing id is allocated by
        // Register. Guarded by its own lock (mutated on define-paths from the owning thread,
        // read by the dispatcher on the same thread, but distinct states may be set up
        // concurrently - same contract as the other process-global maps).
        private static readonly Dictionary<long, StateCallbacks> StateCallbacksMap =
            new Dictionary<long, StateCallbacks>();

        private static readonly object MapLock = new object();

        private sealed class StateCallbacks
        {
            public long NextId;
            public readonly Dictionary<long, CSharpMethodFunc> ById = new Dictionary<long, CSharpMethodFunc>();
        }

        // Ensure the native side knows our dispatcher (idempotent, once per process).
        internal static void EnsureDispatcherRegistered()
        {
            if (Interlocked.Exchange(ref s_registered, 1) == 0)
            {
                s_dispatcherHandle = GCHandle.Alloc(s_dispatcher);
                var fp = Marshal.GetFunctionPointerForDelegate(s_dispatcher);
                mrbdotnet_set_dispatcher(fp);
            }
        }

        // Allocate a callbackId for `callback` on `state` and return it. The native
        // define-helpers store this id in the method/proc env; the dispatcher resolves it
        // back to `callback` at invocation time.
        internal static long Register(RbState state, CSharpMethodFunc callback)
        {
            EnsureDispatcherRegistered();
            var key = state.NativeHandler.ToInt64();
            lock (MapLock)
            {
                if (!StateCallbacksMap.TryGetValue(key, out var sc))
                {
                    sc = new StateCallbacks();
                    StateCallbacksMap.Add(key, sc);
                }

                var id = sc.NextId++;
                sc.ById[id] = callback;
                return id;
            }
        }

        // Drop all callbacks for a state (called from Ruby.Close teardown).
        internal static void ReleaseState(RbState state)
        {
            ReleaseState(state.NativeHandler);
        }

        // Overload keyed by the raw native handle, for use during Ruby.Close AFTER the
        // RbState.NativeHandler has been zeroed (the handle is captured before mrb_close).
        internal static void ReleaseState(IntPtr nativeHandle)
        {
            var key = nativeHandle.ToInt64();
            lock (MapLock)
            {
                StateCallbacksMap.Remove(key);
            }
        }

        private static CSharpMethodFunc? Lookup(IntPtr mrb, long callbackId)
        {
            var key = mrb.ToInt64();
            lock (MapLock)
            {
                if (StateCallbacksMap.TryGetValue(key, out var sc) &&
                    sc.ById.TryGetValue(callbackId, out var cb))
                {
                    return cb;
                }
            }

            return null;
        }

        // The single managed dispatcher invoked (indirectly) by the native trampoline.
        // MUST return normally - never throw across the native boundary and never call
        // mrb_raise here; signal errors via *shouldRaise + msgBuf and let native raise.
        private static unsafe UInt64 Dispatch(
            IntPtr mrb,
            UInt64 self,
            Int64 callbackId,
            Int64 argc,
            IntPtr argv,
            IntPtr shouldRaise,
            IntPtr msgBuf,
            Int32 msgBufLen)
        {
            var state = RbHelper.GetOrCreateTransientStatePublic(mrb);
            var raiseFlag = (byte*)shouldRaise;
            try
            {
                var callback = Lookup(mrb, callbackId);
                if (callback == null)
                {
                    WriteMessage(msgBuf, msgBufLen, $"mruby-for-dotnet: no callback registered for id {callbackId}");
                    *raiseFlag = 1;
                    return state.RbNil.NativeValue;
                }

                RbValue[] args;
                if (argc <= 0 || argv == IntPtr.Zero)
                {
                    args = Array.Empty<RbValue>();
                }
                else
                {
                    args = new RbValue[(int)argc];
                    var p = (UInt64*)argv;
                    for (int i = 0; i < argc; i++)
                    {
                        args[i] = new RbValue(state, *(p + i));
                    }
                }

                var csharpSelf = new RbValue(state, self);
                var result = callback(state, csharpSelf, args);
                *raiseFlag = 0;
                return result.NativeValue;
            }
            catch (TargetInvocationException e)
            {
                WriteMessage(msgBuf, msgBufLen,
                    $"Native Exception Message: {e.InnerException?.Message ?? e.Message} \n Stacktrace: {e.InnerException?.StackTrace ?? e.Message}");
                *raiseFlag = 1;
                return state.RbNil.NativeValue;
            }
            catch (Exception e)
            {
                WriteMessage(msgBuf, msgBufLen,
                    $"Native Exception Message: {e.Message} \n Stacktrace: {e.StackTrace}");
                *raiseFlag = 1;
                return state.RbNil.NativeValue;
            }
        }

        // Write a UTF-8, NUL-terminated message into the native fixed buffer (truncating).
        private static unsafe void WriteMessage(IntPtr msgBuf, int msgBufLen, string message)
        {
            if (msgBuf == IntPtr.Zero || msgBufLen <= 1)
            {
                return;
            }

            var bytes = Encoding.UTF8.GetBytes(message);
            var n = Math.Min(bytes.Length, msgBufLen - 1);
            var dst = (byte*)msgBuf;
            for (int i = 0; i < n; i++)
            {
                dst[i] = bytes[i];
            }

            dst[n] = 0;
        }
    }
}
