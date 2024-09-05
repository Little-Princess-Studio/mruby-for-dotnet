using MRuby.Library.Language;

namespace MRuby.UnitTest
{
    using Library;

    public class RbHashTest : IDisposable
    {
        private readonly RbState state;

        public RbHashTest()
        {
            this.state = Ruby.Open();
        }

        public void Dispose()
        {
            Ruby.Close(this.state);
        }

        [Fact]
        public void TestCreateEmptyHash()
        {
            var hash = this.state.NewHash();
            Assert.Equal(this.state, hash.State);
        }

        [Fact]
        public void TestCreateHashWithCapacity()
        {
            var hash = RbHash.NewWithCapacity(this.state, 10);
            Assert.Equal(this.state, hash.State);
        }

        [Fact]
        public void TestStoreAndRetrieveValue()
        {
            var hash = this.state.NewHash();
            var key = this.state.BoxString("key");
            var value = this.state.BoxString("value");
            ;
            hash[key] = value;

            var retrievedValue = hash.Get(key);
            Assert.Equal(1, hash.Size);
            Assert.Equal(value, retrievedValue);
        }

        [Fact]
        public void TestRetrieveValueOrDefault()
        {
            var hash = this.state.NewHash();
            var key = this.state.BoxInt(1);
            var defaultValue = this.state.BoxString("value");
            var fetchedValue = hash.Fetch(key, defaultValue);
            Assert.Equal(0, hash.Size);
            Assert.Equal(defaultValue, fetchedValue);

            var replaceValue = this.state.BoxString("value2");
            hash[key] = replaceValue;
            fetchedValue = hash.Fetch(key, defaultValue);
            Assert.Equal(1, hash.Size);
            Assert.Equal(replaceValue, fetchedValue);
        }

        [Fact]
        public void TestRemoveKeyFromHash()
        {
            var hash = this.state.NewHash();
            var key = this.state.BoxString("key");
            var value = this.state.BoxString("value");
            hash.Set(key, value);
            var deletedValue = hash.DeleteKey(key);

            Assert.Equal(0, hash.Size);
            Assert.Equal(value, deletedValue);
            Assert.False(hash.ContainsKey(key));
        }

        [Fact]
        public void TestHash()
        {
            var val1 = this.state.BoxString("1111");
            var val2 = this.state.BoxString("1111");
            
            Assert.Equal(val1.GetHashCode(), val2.GetHashCode());
        }
        
        [Fact]
        public void TestGetAllKeys()
        {
            var dict = new Dictionary<RbValue, RbValue>
            {
                [this.state.BoxString("key1")] = this.state.BoxString("value1"),
                [this.state.BoxString("key2")] = this.state.BoxString("value2"),
                [this.state.BoxString("key3")] = this.state.BoxString("value3"),
            };

            var hash = this.state.NewHashFromDictionary(dict);
            var keys = hash.Keys;

            Assert.Equal(3, hash.Size);
            Assert.Equal(3, keys.Size);

            for (int i = 0; i < keys.Size; ++i)
            {
                var key = keys[i];
                var val = hash[key];

                Assert.True(dict.ContainsKey(key));
                Assert.True(dict.ContainsValue(val));
                Assert.Equal(dict[key], val);
            }
        }

        [Fact]
        public void TestCheckIfKeyExists()
        {
            var dict = new Dictionary<RbValue, RbValue>
            {
                [this.state.BoxString("key1")] = this.state.BoxString("value1"),
                [this.state.BoxString("key2")] = this.state.BoxString("value2"),
                [this.state.BoxString("key3")] = this.state.BoxString("value3"),
            };

            var hash = this.state.NewHashFromDictionary(dict);
            
            Assert.True(hash.ContainsKey(this.state.BoxString("key1")));
            Assert.True(hash.ContainsKey(this.state.BoxString("key2")));
            Assert.True(hash.ContainsKey(this.state.BoxString("key3")));
            Assert.False(hash.ContainsKey(this.state.BoxString("key4")));
        }

        [Fact]
        public void TestCheckIfHashIsEmpty()
        {
            var hash = this.state.NewHash();
            Assert.True(hash.IsEmpty);

            hash.Set(this.state.BoxInt(1), this.state.BoxFloat(1.0));
            Assert.False(hash.IsEmpty);
        }

        [Fact]
        public void TestGetAllValues()
        {
            var dict = new Dictionary<RbValue, RbValue>
            {
                [this.state.BoxString("key1")] = this.state.BoxString("value1"),
                [this.state.BoxString("key2")] = this.state.BoxString("value2"),
                [this.state.BoxString("key3")] = this.state.BoxString("value3"),
            };

            var hash = this.state.NewHashFromDictionary(dict);
            var values = hash.Values;

            Assert.Equal(3, hash.Size);
            Assert.Equal(3, values.Size);

            for (int i = 0; i < values.Size; ++i)
            {
                var val = values[i];
                Assert.True(dict.ContainsValue(val));
            }
        }

        [Fact]
        public void TestRemoveAllEntries()
        {
            var dict = new Dictionary<RbValue, RbValue>
            {
                [this.state.BoxString("key1")] = this.state.BoxString("value1"),
                [this.state.BoxString("key2")] = this.state.BoxString("value2"),
                [this.state.BoxString("key3")] = this.state.BoxString("value3"),
            };

            var hash = this.state.NewHashFromDictionary(dict);
            Assert.Equal(3, hash.Size);
            
            hash.Clear();

            Assert.True(hash.IsEmpty);
        }

        [Fact]
        public void TestCreateCopyOfHash()
        {
            var dict = new Dictionary<RbValue, RbValue>
            {
                [this.state.BoxString("key1")] = this.state.BoxString("value1"),
                [this.state.BoxString("key2")] = this.state.BoxString("value2"),
                [this.state.BoxString("key3")] = this.state.BoxString("value3"),
            };
            var hash = this.state.NewHashFromDictionary(dict);
            
            var duplicate = hash.Duplicate();
            var keys = hash.Keys;

            Assert.Equal(hash.Size, duplicate.Size);
            for (int i = 0; i < keys.Size; ++i)
            {
                var key = keys[i];
                var val = hash[key];
                var valDup = duplicate[key];

                Assert.Equal(val, valDup);
            }
        }

        [Fact]
        public void TestCombineTwoHashes()
        {
            var dict1 = new Dictionary<RbValue, RbValue>
            {
                [this.state.BoxString("key1")] = this.state.BoxString("value1"),
                [this.state.BoxString("key2")] = this.state.BoxString("value2"),
                [this.state.BoxString("key3")] = this.state.BoxString("value3"),
            };
            var hash1 = this.state.NewHashFromDictionary(dict1);

            var dict2 = new Dictionary<RbValue, RbValue>
            {
                [this.state.BoxString("key3")] = this.state.BoxString("value3"),
                [this.state.BoxString("key4")] = this.state.BoxString("value4"),
                [this.state.BoxString("key5")] = this.state.BoxString("value5"),
            };
            var hash2 = this.state.NewHashFromDictionary(dict2);
            
            hash1.Merge(hash2);
            
            Assert.Equal(5, hash1.Size);

            foreach (var (key, value) in dict1)
            {
                Assert.True(hash1.ContainsKey(key));
                Assert.Equal(value, hash1[key]);
            }            
            
            foreach (var (key, value) in dict2)
            {
                Assert.True(hash1.ContainsKey(key));
                Assert.Equal(value, hash1[key]);
            }
        }
    }
}