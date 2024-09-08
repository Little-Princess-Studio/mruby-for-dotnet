using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using MRuby.Library.Language;

namespace MRuby.Library
{
    [ExcludeFromCodeCoverage]
    public abstract class RbNativeObjectLiveKeeper
    {
        protected static readonly Dictionary<RbState, Dictionary<Type, RbNativeObjectLiveKeeper>> StateMapper =
            new Dictionary<RbState, Dictionary<Type, RbNativeObjectLiveKeeper>>();

        public static void ReleaseKeeper(RbState state)
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

        public abstract void Clear();
    }

    [ExcludeFromCodeCoverage]
    public class RbNativeObjectLiveKeeper<TCategory, TObjectType> : RbNativeObjectLiveKeeper
    {
        private readonly Dictionary<IComparable, TObjectType> KeyedStorage = new Dictionary<IComparable, TObjectType>();
        private readonly HashSet<TObjectType> Storage = new HashSet<TObjectType>();

        public static RbNativeObjectLiveKeeper<TCategory, TObjectType> GetOrCreateKeeper(RbState state)
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