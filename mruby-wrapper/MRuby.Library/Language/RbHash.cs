using System.Collections;
using System.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace MRuby.Library.Language
{
    public partial class RbHash : IEnumerable<KeyValuePair<RbValue, RbValue>>
    {
        public readonly RbValue Value;
        public readonly RbState State;

        public Int64 Size => mrb_hash_size(this.State.NativeHandler, this.Value.NativeValue);

        private RbHash(RbValue value)
        {
            this.Value = value;
            this.State = value.RbState;
        }

        public static RbHash FromHashObject(RbValue value)
        {
            // todo: check if value is a hash of ruby
            return new RbHash(value);
        }

        public static RbHash New(RbState state)
        {
            var hashPtr = mrb_hash_new(state.NativeHandler);
            return FromHashObject(new RbValue(state, hashPtr));
        }

        public static RbHash NewWithCapacity(RbState state, int capacity)
        {
            var hashPtr = mrb_hash_new_capa(state.NativeHandler, capacity);
            return FromHashObject(new RbValue(state, hashPtr));
        }

        public static RbHash FromDictionary(RbState state, IDictionary<RbValue, RbValue> dict)
        {
            var hash = NewWithCapacity(state, dict.Count);
            foreach (var (key, value) in dict)
            {
                hash.Set(key, value);
            }
            return hash;
        }

        public void Set(RbValue key, RbValue value) => mrb_hash_set(this.State.NativeHandler, this.Value.NativeValue, key.NativeValue, value.NativeValue);

        public RbValue Get(RbValue key)
        {
            var valuePtr = mrb_hash_get(this.State.NativeHandler, this.Value.NativeValue, key.NativeValue);
            return new RbValue(this.State, valuePtr);
        }

        public RbValue Fetch(RbValue key, RbValue defaultValue)
        {
            var valuePtr = mrb_hash_fetch(this.State.NativeHandler, this.Value.NativeValue, key.NativeValue, defaultValue.NativeValue);
            return new RbValue(this.State, valuePtr);
        }

        public RbValue DeleteKey(RbValue key)
        {
            var valuePtr = mrb_hash_delete_key(this.State.NativeHandler, this.Value.NativeValue, key.NativeValue);
            return new RbValue(this.State, valuePtr);
        }

        public RbArray Keys
        {
            get
            {
                var keysPtr = mrb_hash_keys(this.State.NativeHandler, this.Value.NativeValue);
                return this.State.NewArrayFromArrayObject(new RbValue(this.State, keysPtr));
            }
        }

        public bool ContainsKey(RbValue key) => mrb_hash_key_p(this.State.NativeHandler, this.Value.NativeValue, key.NativeValue);

        public bool IsEmpty => mrb_hash_empty_p(this.State.NativeHandler, this.Value.NativeValue);

        public RbArray Values
        {
            get
            {
                var valuesPtr = mrb_hash_values(this.State.NativeHandler, this.Value.NativeValue);
                return this.State.NewArrayFromArrayObject(new RbValue(this.State, valuesPtr));
            }
        }

        public void Clear() => mrb_hash_clear(this.State.NativeHandler, this.Value.NativeValue);

        public RbHash Duplicate()
        {
            UInt64 hashPtr = mrb_hash_dup(this.State.NativeHandler, this.Value.NativeValue);
            return FromHashObject(new RbValue(this.State, hashPtr));
        }

        public void Merge(RbHash other) => mrb_hash_merge(this.State.NativeHandler, this.Value.NativeValue, other.Value.NativeValue);

        public RbValue this[RbValue key]
        {
            get => this.Get(key);
            set => this.Set(key, value);
        }
        
        [ExcludeFromCodeCoverage]
        public IEnumerator<KeyValuePair<RbValue, RbValue>> GetEnumerator() => new RbHashEnumerator(this);

        [ExcludeFromCodeCoverage]
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        [ExcludeFromCodeCoverage]
        private class RbHashEnumerator : IEnumerator<KeyValuePair<RbValue, RbValue>>
        {
            private readonly RbHash hash;
            private readonly RbValue[] keys;
            private int currentKeyIndex;

            public RbHashEnumerator(RbHash hash)
            {
                this.hash = hash;
                this.keys = this.hash.Keys.Select(v => v).ToArray();
                this.currentKeyIndex = -1;
            }

            public bool MoveNext()
            {
                if (this.currentKeyIndex >= this.keys.Length - 1)
                {
                    return false;
                }

                ++this.currentKeyIndex;
                return true;
            }

            public void Reset()
            {
                this.currentKeyIndex = -1;
            }

            public KeyValuePair<RbValue, RbValue> Current
            {
                get
                {
                    var key = this.keys[this.currentKeyIndex];
                    var value = this.hash[key];
                    return new KeyValuePair<RbValue, RbValue>(key, value);
                }
            }

            object IEnumerator.Current => Current;

            public void Dispose()
            {
            }
        }
    }

    public static class DictionaryExtension
    {
        public static RbHash ToRbHash(this IDictionary<RbValue, RbValue> self, RbState state)
        {
            return state.NewHashFromDictionary(self);
        }
    }
}