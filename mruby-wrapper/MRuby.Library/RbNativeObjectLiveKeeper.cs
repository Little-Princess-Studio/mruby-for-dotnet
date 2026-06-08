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

    [ExcludeFromCodeCoverage]
    public abstract class RbNativeObjectLiveKeeper
    {
        protected static readonly Dictionary<RbState, Dictionary<Type, RbNativeObjectLiveKeeper>> StateMapper =
            new Dictionary<RbState, Dictionary<Type, RbNativeObjectLiveKeeper>>();

        // Guards every access to the process-wide StateMapper (and its nested per-state
        // dictionaries). Independent RbState instances can be opened/closed concurrently
        // from different managed threads (e.g. xUnit runs test classes in parallel), so
        // the check-then-add in GetOrCreateKeeper and the remove in ReleaseKeeper must be
        // atomic. An unsynchronized Dictionary corrupts under concurrent mutation, which
        // surfaces as a native test-host crash because this keeper roots delegates handed
        // to mruby.
        protected static readonly object StateMapperLock = new object();

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
    }

    [ExcludeFromCodeCoverage]
    public class RbNativeObjectLiveKeeper<TCategory, TObjectType> : RbNativeObjectLiveKeeper
    {
        private readonly Dictionary<IComparable, TObjectType> KeyedStorage = new Dictionary<IComparable, TObjectType>();
        private readonly HashSet<TObjectType> Storage = new HashSet<TObjectType>();

        public static RbNativeObjectLiveKeeper<TCategory, TObjectType> GetOrCreateKeeper(RbState state)
        {
            lock (StateMapperLock)
            {
                if (!StateMapper.TryGetValue(state, out var keepers))
                {
                    keepers = new Dictionary<Type, RbNativeObjectLiveKeeper>();
                    StateMapper.Add(state, keepers);
                }

                if (!keepers.TryGetValue(typeof(TCategory), out var obj))
                {
                    var keeper = new RbNativeObjectLiveKeeper<TCategory, TObjectType>();
                    keepers.Add(typeof(TCategory), keeper);
                    return keeper;
                }

                return (RbNativeObjectLiveKeeper<TCategory, TObjectType>)obj;
            }
        }

        public void Keep(IComparable key, TObjectType obj) => this.KeyedStorage[key] = obj;

        public void Keep(TObjectType obj) => this.Storage.Add(obj);
        
        public bool Contains(IComparable key) => this.KeyedStorage.ContainsKey(key);

        public bool Contains(TObjectType obj) => this.Storage.Contains(obj);
        
        public TObjectType Get(IComparable key) => this.KeyedStorage[key];

        public void Release(IComparable key) => this.KeyedStorage.Remove(key);

        public void Release(TObjectType obj) => this.Storage.Remove(obj);

        public override void Clear()
        {
            this.Storage.Clear();
            this.KeyedStorage.Clear();
        }
    }
}