using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using MRuby.Library.Language;

namespace MRuby.Library
{
    // Keeper category marker for native callback delegates (NativeMethodFunc) handed to
    // mruby by DefineMethod/DefineClassMethod/NewProc/etc. mruby retains only the raw
    // function pointer, so the managed delegate must be rooted to the RbState lifetime.
    [ExcludeFromCodeCoverage]
    public sealed class RbCallbackKeeper
    {
    }

    // Keeper category marker for per-object data GCHandle registrations.
    // Stores (GCHandle IntPtr, mrb_value UInt64, optional release callback) entries, keyed by the GCHandle IntPtr.
    // Used to pre-free and disarm data objects before mrb_close, preventing the
    // close-time dfree reverse-P/Invoke callback that can crash on macOS.
    [ExcludeFromCodeCoverage]
    public sealed class RbDataObjectKeeper
    {
    }

    [ExcludeFromCodeCoverage]
    internal readonly struct RbDataObjectRegistration
    {
        public readonly IntPtr Handle;
        public readonly UInt64 MrbValue;
        public readonly Action<RbState, object?>? ReleaseFn;

        public RbDataObjectRegistration(IntPtr handle, UInt64 mrbValue, Action<RbState, object?>? releaseFn)
        {
            this.Handle = handle;
            this.MrbValue = mrbValue;
            this.ReleaseFn = releaseFn;
        }
    }

    // Base class for per-state object keepers. Holds ONLY the shared process-wide
    // StateMapper plumbing (the per-state bucket of keepers, keyed by category type)
    // and the lock that guards it. It deliberately stores NO objects itself - the two
    // concrete subclasses each own exactly one storage shape:
    //
    //   * RbKeyedObjectKeeper<TCategory, TObjectType> - a keyed Dictionary, for objects
    //     that must be looked up / removed / drained individually (data-object GCHandles
    //     keyed by IntPtr, auto-registered method delegates keyed by name).
    //   * RbSetObjectKeeper<TCategory, TObjectType> - an unkeyed HashSet, for objects that
    //     only need to be kept alive in bulk and cleared at close (callback delegates).
    //
    // Splitting the two shapes keeps each keeper instance carrying a single collection
    // instead of one used + one perpetually-empty collection.
    [ExcludeFromCodeCoverage]
    public abstract class RbNativeObjectLiveKeeper
    {
        internal static readonly Dictionary<RbState, Dictionary<Type, RbNativeObjectLiveKeeper>> StateMapper =
            new Dictionary<RbState, Dictionary<Type, RbNativeObjectLiveKeeper>>();

        // Guards every access to the process-wide StateMapper (and its nested per-state
        // dictionaries). Independent RbState instances can be opened/closed concurrently
        // from different managed threads (e.g. xUnit runs test classes in parallel), so
        // the check-then-add in GetOrCreateKeeper and the remove in ReleaseKeeper must be
        // atomic. An unsynchronized Dictionary corrupts under concurrent mutation, which
        // surfaces as a native test-host crash because this keeper roots delegates handed
        // to mruby.
        internal static readonly object StateMapperLock = new object();

        public static void ReleaseKeeper(RbState state)
        {
            lock (StateMapperLock)
            {
                if (!StateMapper.TryGetValue(state, out var keepers))
                {
                    return;
                }

                foreach (var (_, keeper) in keepers)
                {
                    keeper.Clear();
                }

                StateMapper.Remove(state);
            }
        }

        public abstract void Clear();

        // Shared check-then-add of the per-state keeper bucket. The subclasses pass their
        // own category type and a factory so a single locked code path serves both shapes.
        protected static TKeeper GetOrCreateKeeper<TKeeper>(RbState state, Type category, Func<TKeeper> factory)
            where TKeeper : RbNativeObjectLiveKeeper
        {
            lock (StateMapperLock)
            {
                if (!StateMapper.TryGetValue(state, out var keepers))
                {
                    keepers = new Dictionary<Type, RbNativeObjectLiveKeeper>();
                    StateMapper.Add(state, keepers);
                }

                if (!keepers.TryGetValue(category, out var existing))
                {
                    var keeper = factory();
                    keepers.Add(category, keeper);
                    return keeper;
                }

                return (TKeeper)existing;
            }
        }
    }

    // Keyed keeper: stores objects in a Dictionary so individual entries can be looked up,
    // removed, and drained. Used by RbDataObjectKeeper (key = GCHandle IntPtr) and
    // RbAutoRegisterKeeper (key = "Type#method" name).
    [ExcludeFromCodeCoverage]
    public class RbKeyedObjectKeeper<TCategory, TObjectType> : RbNativeObjectLiveKeeper
    {
        private readonly Dictionary<IComparable, TObjectType> KeyedStorage = new Dictionary<IComparable, TObjectType>();

        public static RbKeyedObjectKeeper<TCategory, TObjectType> GetOrCreateKeeper(RbState state)
            => GetOrCreateKeeper(state, typeof(TCategory), () => new RbKeyedObjectKeeper<TCategory, TObjectType>());

        public void Keep(IComparable key, TObjectType obj) => this.KeyedStorage[key] = obj;

        public void Keep(IntPtr key, TObjectType obj) => this.Keep(key.ToInt64(), obj);

        public IReadOnlyDictionary<IComparable, TObjectType> Drain()
        {
            var snapshot = new Dictionary<IComparable, TObjectType>(this.KeyedStorage);
            this.KeyedStorage.Clear();
            return snapshot;
        }

        public void Release(IComparable key) => this.KeyedStorage.Remove(key);

        public void Release(IntPtr key) => this.Release(key.ToInt64());

        public override void Clear() => this.KeyedStorage.Clear();
    }

    // Set keeper: stores objects in a HashSet purely to root them for the RbState lifetime.
    // Used by RbCallbackKeeper, whose delegates only need to outlive the state, never to be
    // looked up or removed individually.
    [ExcludeFromCodeCoverage]
    public class RbSetObjectKeeper<TCategory, TObjectType> : RbNativeObjectLiveKeeper
    {
        private readonly HashSet<TObjectType> Storage = new HashSet<TObjectType>();

        public static RbSetObjectKeeper<TCategory, TObjectType> GetOrCreateKeeper(RbState state)
            => GetOrCreateKeeper(state, typeof(TCategory), () => new RbSetObjectKeeper<TCategory, TObjectType>());

        public void Keep(TObjectType obj) => this.Storage.Add(obj);

        public void Release(TObjectType obj) => this.Storage.Remove(obj);

        public override void Clear() => this.Storage.Clear();
    }
}
