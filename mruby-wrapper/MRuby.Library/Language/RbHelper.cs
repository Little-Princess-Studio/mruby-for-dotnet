namespace MRuby.Library.Language
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using MRuby.Library;

    public struct RbDataClassType
    {
        [MarshalAs(UnmanagedType.LPStr)]
        public readonly string Name;

        [MarshalAs(UnmanagedType.FunctionPtr)]
        public readonly NativeDataObjectFreeFunc FreeFunc;

        public RbDataClassType(string name, NativeDataObjectFreeFunc freeFunc)
        {
            this.Name = name;
            this.FreeFunc = freeFunc;
        }
    }

    public static partial class RbHelper
    {
        [ExcludeFromCodeCoverage]
        public static UInt32 MRB_ARGS_REQ(uint n) => (n & 0x1fU) << 18;

        [ExcludeFromCodeCoverage]
        public static UInt32 MRB_ARGS_OPT(uint n) => (n & 0x1fU) << 13;

        [ExcludeFromCodeCoverage]
        public static UInt32 MRB_ARGS_ARG(uint n1, uint n2) => MRB_ARGS_REQ(n1) | MRB_ARGS_OPT(n2);

        [ExcludeFromCodeCoverage]
        public static UInt32 MRB_ARGS_REST() => 1U << 12;

        [ExcludeFromCodeCoverage]
        public static UInt32 MRB_ARGS_POST(uint n) => (n & 0x1fU) << 7;

        [ExcludeFromCodeCoverage]
        public static UInt32 MRB_ARGS_KEY(uint n1, uint n2) => ((n1 & 0x1fU) << 2) | (n2 != 0 ? 1U : 0);

        [ExcludeFromCodeCoverage]
        public static UInt32 MRB_ARGS_BLOCK() => 1U;

        [ExcludeFromCodeCoverage]
        public static UInt32 MRB_ARGS_ANY() => MRB_ARGS_REST();

        [ExcludeFromCodeCoverage]
        public static UInt32 MRB_ARGS_NONE() => 0U;

        public static IntPtr GetIntPtrOfCSharpObject(object? obj) => GCHandle.ToIntPtr(GCHandle.Alloc(obj));

        public static void FreeIntPtrOfCSharpObject(IntPtr ptr) => GCHandle.FromIntPtr(ptr).Free();

        public static object? GetObjectFromIntPtr(IntPtr ptr) => GCHandle.FromIntPtr(ptr).Target;

        public static IntPtr GetRbObjectPtrFromValue(RbValue value) => GetRbObjectPtrFromValue(value.NativeValue);

        public static IntPtr GetRbObjectPtrFromValue(UInt64 nativeHandler) => mrb_value_to_obj_ptr(nativeHandler);

        public static byte[] GetRawBytesFromRbStringObject(RbValue value)
        {
            unsafe
            {
                IntPtr bytes = IntPtr.Zero;
                ulong length = 0;
                mrb_get_raw_bytes_from_string(value.NativeValue, ref bytes, ref length);

                if (length <= 0)
                {
                    return Array.Empty<byte>();
                }

                var result = new byte[length];
                for (ulong i = 0; i < length; ++i)
                {
                    result[i] = ((byte*)bytes)[i];
                }
                return result;
            }
        }

        public static RbValue BuildRbStringObjectFromRawBytes(RbState state, byte[] bytes)
        {
            var v = mrb_str_new(state.NativeHandler, bytes, bytes.Length);
            return new RbValue(state, v);
        }

        private static Dictionary<string, (RbDataClassType, IntPtr)> RbDataClassMapping { get; } = new Dictionary<string, (RbDataClassType, IntPtr)>();

        // Guards the process-wide RbDataClassMapping. The check-then-add in
        // GetOrCreateNewRbDataStructPtr must be atomic: concurrent registration of the
        // same data-class name would otherwise double-call Dictionary.Add (throwing
        // ArgumentException) and leak the Marshal.AllocHGlobal allocation.
        private static readonly object RbDataClassMappingLock = new object();

        // Canonical per-state RbState cache. Instead of allocating a new RbState wrapper per
        // callback invocation, the trampoline looks up the single canonical instance created at
        // Ruby.Open() time. This eliminates 5+ allocations per callback invocation (RbState +
        // 4 RbValue sentinels) that drive CLR gen0 GC frequency.
        private static readonly Dictionary<long, RbState> CanonicalStateCache =
            new Dictionary<long, RbState>();

        internal static readonly object CanonicalStateCacheLock = new object();

        internal static void RegisterCanonicalState(RbState state)
        {
            lock (CanonicalStateCacheLock)
            {
                CanonicalStateCache[state.NativeHandler.ToInt64()] = state;
            }
        }

        internal static void UnregisterCanonicalState(RbState state)
        {
            lock (CanonicalStateCacheLock)
            {
                CanonicalStateCache.Remove(state.NativeHandler.ToInt64());
            }
        }

        private static RbState GetOrCreateTransientState(IntPtr nativeHandle)
        {
            lock (CanonicalStateCacheLock)
            {
                if (CanonicalStateCache.TryGetValue(nativeHandle.ToInt64(), out var cached))
                {
                    return cached;
                }
            }

            // Fallback for callbacks before Open() or after Close(): create a transient state.
            // This should not happen in normal usage.
            return new RbState
            {
                NativeHandler = nativeHandle
            };
        }

        // Internal accessor so the native callback dispatcher (RbCallbackDispatch) can reuse
        // the canonical per-state cache (avoids allocating a new RbState per callback).
        internal static RbState GetOrCreateTransientStatePublic(IntPtr nativeHandle)
            => GetOrCreateTransientState(nativeHandle);

        private static bool RbDataStructExist(string name) => RbDataClassMapping.ContainsKey(name);

        private static void RbDataStructAdd(string name, Action<RbState, object?>? releaseFn)
        {
            var typeStruct = Marshal.AllocHGlobal(Marshal.SizeOf<RbDataClassType>());
            RbDataClassType type;
            if (releaseFn != null)
            {
                type = new RbDataClassType(name, (mrb, data) =>
                {
                    if (data == IntPtr.Zero)
                    {
                        return;
                    }

                    var obj = GetObjectFromIntPtr(data);
                    RemoveFromDataRegistry(mrb, data);
                    FreeIntPtrOfCSharpObject(data);
                    releaseFn(new RbState
                    {
                        NativeHandler = mrb
                    }, obj);
                });
            }
            else
            {
                type = new RbDataClassType(name, NativeDataObjectFreeFunc);
            }
            Marshal.StructureToPtr(type, typeStruct, false);
            RbDataClassMapping.Add(name, (type, typeStruct));
        }

        internal static bool IsInteger(RbValue obj) => mrb_check_type_integer(obj.NativeValue);

        internal static bool IsSymbol(RbValue obj) => mrb_check_type_symbol(obj.NativeValue);

        internal static bool IsFloat(RbValue obj) => mrb_check_type_float(obj.NativeValue);

        internal static bool IsArray(RbValue obj) => mrb_check_type_array(obj.NativeValue);

        internal static bool IsString(RbValue obj) => mrb_check_type_string(obj.NativeValue);

        internal static bool IsHash(RbValue obj) => mrb_check_type_hash(obj.NativeValue);

        internal static bool IsException(RbValue obj) => mrb_check_type_exception(obj.NativeValue);

        internal static bool IsObject(RbValue obj) => mrb_check_type_object(obj.NativeValue);

        internal static bool IsClass(RbValue obj) => mrb_check_type_class(obj.NativeValue);

        internal static bool IsModule(RbValue obj) => mrb_check_type_moudle(obj.NativeValue);

        internal static bool IsSClass(RbValue obj) => mrb_check_type_sclass(obj.NativeValue);

        internal static bool IsProc(RbValue obj) => mrb_check_type_proc(obj.NativeValue);

        internal static bool IsRange(RbValue obj) => mrb_check_type_range(obj.NativeValue);

        internal static bool IsFiber(RbValue obj) => mrb_check_type_fiber(obj.NativeValue);

        internal static IntPtr GetOrCreateNewRbDataStructPtr(string name, Action<RbState, object?>? releseFn = null)
        {
            lock (RbDataClassMappingLock)
            {
                if (RbDataStructExist(name))
                {
                    return RbDataClassMapping[name].Item2;
                }

                RbDataStructAdd(name, releseFn);
                return RbDataClassMapping[name].Item2;
            }
        }

        [ExcludeFromCodeCoverage]
        internal static unsafe NativeMethodFunc BuildCSharpCallbackToNativeCallbackBridgeMethod(CSharpMethodFunc callback)
        {
            UInt64 Lambda(IntPtr state, ulong self)
            {
                var argc = mrb_get_argc(state);
                var argv = mrb_get_argv(state);
                var csharpState = GetOrCreateTransientState(state);

                RbValue[] args;
                if (argc == 0)
                {
                    args = Array.Empty<RbValue>();
                }
                else
                {
                    args = new RbValue[(int)argc];
                    for (int i = 0; i < argc; i++)
                    {
                        var arg = *(((UInt64*)argv) + i);
                        args[i] = new RbValue(csharpState, arg);
                    }
                }

                var csharpSelf = new RbValue(csharpState, self);
                try
                {
                    var csharpRes = callback(csharpState, csharpSelf, args);
                    return csharpRes.NativeValue;
                }
                catch (TargetInvocationException e)
                {
                    var totalMsg = $"Native Exception Message: {e.InnerException?.Message ?? e.Message} \n Stacktrace: {e.InnerException?.StackTrace ?? e.Message}";
                    var excCls = csharpState.GetClass("Exception");
                    var exc = csharpState.GenerateExceptionWithNewStr(excCls, totalMsg);
                    return RaiseNativeCallbackException(csharpState, exc);
                }
                catch (Exception e)
                {
                    var totalMsg = $"Native Exception Message: {e.Message} \n Stacktrace: {e.StackTrace}";
                    var excCls = csharpState.GetClass("Exception");
                    var exc = csharpState.GenerateExceptionWithNewStr(excCls, totalMsg);
                    return RaiseNativeCallbackException(csharpState, exc);
                }
            }
            return Lambda;
        }

        // Builds the native callback bridge AND roots it for the lifetime of the
        // owning RbState. mruby keeps only the raw function pointer of the delegate
        // (in the method table, a proc, etc.); the managed NativeMethodFunc itself has
        // no other GC root once the caller discards the `out` value (the idiomatic
        // `out _`). Without rooting, the next GC collects the delegate and mruby later
        // calls through a freed function pointer -> hard native crash. Rooting into the
        // per-state keeper ties the delegate's lifetime to Ruby.Close(state).
        internal static NativeMethodFunc BuildAndRootNativeCallback(RbState state, CSharpMethodFunc callback)
        {
            var nativeFunc = BuildCSharpCallbackToNativeCallbackBridgeMethod(callback);
            RootNativeCallback(state, nativeFunc);
            return nativeFunc;
        }

        // Trampoline path: register `callback` for `state`, returning its callbackId. The
        // native define-helpers store this id in the method/proc env; mruby only ever holds
        // the static native trampoline pointer (never a managed delegate), so a raise from
        // the callback longjmps below the managed frame (no macOS crash) and the delegate
        // can never be collected out from under mruby.
        internal static long RegisterCallback(RbState state, CSharpMethodFunc callback)
        {
            return RbCallbackDispatch.Register(state, callback);
        }

        // Roots an already-built native callback delegate to the RbState lifetime so
        // mruby's retained function pointer never dangles. Safe to call for transient
        // callbacks too (they are simply released at Ruby.Close).
        internal static void RootNativeCallback(RbState state, NativeMethodFunc nativeFunc)
        {
            var keeper = RbSetObjectKeeper<RbCallbackKeeper, NativeMethodFunc>.GetOrCreateKeeper(state);
            keeper.Keep(nativeFunc);
        }

        [ExcludeFromCodeCoverage]
        private static UInt64 RaiseNativeCallbackException(RbState state, RbValue exc)
        {
            state.Raise(exc);
            return state.RbNil.NativeValue;
        }

        private static void NativeDataObjectFreeFunc(IntPtr state, IntPtr data)
        {
            if (data == IntPtr.Zero)
            {
                return;
            }

            RemoveFromDataRegistry(state, data);
            FreeIntPtrOfCSharpObject(data);
        }

        private static void RemoveFromDataRegistry(IntPtr mrbHandle, IntPtr dataHandle)
        {
            // Look up the RbState by native handle to find its keeper.
            // StateMapper is keyed by RbState objects; we need to find the one matching mrbHandle.
            lock (RbNativeObjectLiveKeeper.StateMapperLock)
            {
                foreach (var kvp in RbNativeObjectLiveKeeper.StateMapper)
                {
                    if (kvp.Key.NativeHandler == mrbHandle)
                    {
                        if (kvp.Value.TryGetValue(typeof(RbDataObjectKeeper), out var keeper))
                        {
                            ((RbKeyedObjectKeeper<RbDataObjectKeeper, RbDataObjectRegistration>)keeper)
                                .Release(dataHandle);
                        }

                        return;
                    }
                }
            }
        }

        internal static UInt64 GetInternSymbol(RbState state, string str) => mrb_intern_cstr(state.NativeHandler, str);

        internal static RbValue CallMethod(RbState state, RbValue value, string name, params RbValue[] args)
        {
            int length = args.Length;

            UInt64 resVal;
            var sym = mrb_intern_cstr(state.NativeHandler, name);

            resVal = mrb_funcall_argv(
                state.NativeHandler,
                value.NativeValue,
                sym,
                length,
                length == 0 ? null! : args.Select(v => v.NativeValue).ToArray());

            return new RbValue(state, resVal);
        }

        internal static RbValue CallMethodWithBlock(RbState state, RbValue value, string name, RbValue block, params RbValue[] args)
        {
            int length = args.Length;

            UInt64 resVal;
            var sym = mrb_intern_cstr(state.NativeHandler, name);

            resVal = mrb_funcall_with_block(
                state.NativeHandler,
                value.NativeValue,
                sym,
                length,
                length == 0 ? null! : args.Select(v => v.NativeValue).ToArray(),
                block.NativeValue);

            return new RbValue(state, resVal);
        }

        internal static bool BlockGivenP(RbState state) => mrb_block_given_p(state.NativeHandler);

        internal static string? GetSymbolName(RbState state, UInt64 sym)
        {
            var ptr = mrb_sym_name(state.NativeHandler, sym);
            return Marshal.PtrToStringAnsi(ptr);
        }

        // internal static string? GetSymbolDump(State State, UInt64 sym)
        // {
        //     var ptr = mrb_sym_dump(State.NativeHandler, sym);
        //     return Marshal.PtrToStringAnsi(ptr);
        // }

        internal static RbValue GetSymbolStr(RbState state, UInt64 sym)
        {
            var result = mrb_sym_str(state.NativeHandler, sym);
            return new RbValue(state, result);
        }

        internal static RbValue NewRubyString(RbState state, string str)
        {
            var result = mrb_str_new_cstr(state.NativeHandler, str);
            return new RbValue(state, result);
        }

        internal static RbValue PtrToRbValue(RbState state, IntPtr p)
        {
            var result = mrb_ptr_to_mrb_value(p);
            return new RbValue(state, result);
        }

        internal static RbClass GetRbClassFromValue(RbState state, RbValue value)
        {
            var ptr = mrb_get_class_ptr(value.NativeValue);
            return new RbClass(ptr, state);
        }

        internal static RbValue GetConst(RbState state, RbValue scope, string name)
        {
            var sym = state.GetInternSymbol(name);
            var result = mrb_const_get(state.NativeHandler, scope.NativeValue, sym);
            return new RbValue(state, result);
        }

        internal static void SetConst(RbState state, RbValue scope, string name, RbValue val)
        {
            var sym = state.GetInternSymbol(name);
            mrb_const_set(state.NativeHandler, scope.NativeValue, sym, val.NativeValue);
        }

        internal static Int64 GetArgs(RbState stat, string format, ref RbValue[] args)
        {
            var argc = args.Length;
            IntPtr[] argsToParse = new IntPtr[args.Length];

            var argCnt = mrb_get_args_a(stat.NativeHandler, format, ref argsToParse);
            for (var i = 0; i < argc; ++i)
            {
                // args[i] = new RbValue(stat,  *(UInt64*)argsToParse[i]);
                args[i] = PtrToRbValue(stat, argsToParse[i]);
            }
            return argCnt;
        }
    }
}
