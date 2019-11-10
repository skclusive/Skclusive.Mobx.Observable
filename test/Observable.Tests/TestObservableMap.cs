using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Skclusive.Mobx.Observable.Tests
{
    public class TestObservableMap
    {
        [Fact]
        public void TestMapCrud()
        {
            var map = ObservableMap<object, object>.From(new Map<object, object> { { "1", "a" } });

            var changes = new List<IMapDidChange<object, object>>();
            map.Observe(change => changes.Add(change));

            Assert.True(map.Has("1"));
            Assert.False(map.Has(1));
            Assert.Equal("a", map["1"]);
            Assert.Null(map["b"]);
            Assert.Single(map);

            map["1"] = "aa";
            map[1] = "b";

            Assert.True(map.Has("1"));
            Assert.True(map.Has(1));
            Assert.Equal("aa", map["1"]);
            Assert.Equal("b", map[1]);
            Assert.Equal(2, map.Count);

            var k = new[] { "arr" };
            map[k] = "arrVal";

            Assert.True(map.Has(k));
            Assert.Equal("arrVal", map[k]);

            var keys = map.Keys.ToList();
            Assert.Equal(3, keys.Count);
            Assert.Equal("1", keys[0]);
            Assert.Equal(1, keys[1]);
            Assert.Equal(k, keys[2]);

            var values = map.Values.ToList();
            Assert.Equal(3, values.Count);
            Assert.Equal("aa", values[0]);
            Assert.Equal("b", values[1]);
            Assert.Equal("arrVal", values[2]);

            Assert.Equal(3, map.Count);

            map.Clear();
            Assert.Empty(map.Keys);
            Assert.Empty(map.Values);
            Assert.Empty(map);

            // Assert.Equal("ObservableMap@1[{}]", map.ToString());

            Assert.False(map.Has("a"));
            Assert.False(map.Has("b"));
            Assert.Null(map["a"]);
            Assert.Null(map["b"]);

            Assert.Equal(6, changes.Count);

            Assert.Equal(map, changes[0].Object);
            Assert.Equal("1", changes[0].Name);
            Assert.Equal("aa", changes[0].NewValue);
            Assert.Equal("a", changes[0].OldValue);
            Assert.Equal(ChangeType.UPDATE, changes[0].Type);

            Assert.Equal(map, changes[1].Object);
            Assert.Equal(1, changes[1].Name);
            Assert.Equal("b", changes[1].NewValue);
            Assert.Null(changes[1].OldValue);
            Assert.Equal(ChangeType.ADD, changes[1].Type);

            Assert.Equal(map, changes[2].Object);
            Assert.Equal(k, changes[2].Name);
            Assert.Equal("arrVal", changes[2].NewValue);
            Assert.Null(changes[2].OldValue);
            Assert.Equal(ChangeType.ADD, changes[2].Type);

            Assert.Equal(map, changes[3].Object);
            Assert.Equal("1", changes[3].Name);
            Assert.Equal("aa", changes[3].OldValue);
            Assert.Equal(ChangeType.REMOVE, changes[3].Type);

            Assert.Equal(map, changes[4].Object);
            Assert.Equal(1, changes[4].Name);
            Assert.Equal("b", changes[4].OldValue);
            Assert.Equal(ChangeType.REMOVE, changes[4].Type);

            Assert.Equal(map, changes[5].Object);
            Assert.Equal(k, changes[5].Name);
            Assert.Equal("arrVal", changes[5].OldValue);
            Assert.Equal(ChangeType.REMOVE, changes[5].Type);
        }

        [Fact]
        public void TestObserveValue()
        {
            var map = ObservableMap<object, object>.From();

            var hasX = false;
            object valueX = null;
            object valueY = null;

            Globals.Autorun((r) =>
            {
                hasX = map.Has("x");
            });

            Globals.Autorun((r) =>
            {
                valueX = map["x"];
            });

            Globals.Autorun((r) =>
            {
                valueY = map["y"];
            });

            Assert.False(hasX);
            Assert.Null(valueX);

            map["x"] = 3;

            Assert.True(hasX);
            Assert.Equal(3, valueX);

            map["x"] = 4;

            Assert.True(hasX);
            Assert.Equal(4, valueX);

            map.Remove("x");
            Assert.False(hasX);
            Assert.Null(valueX);

            map["x"] = 5;

            Assert.True(hasX);
            Assert.Equal(5, valueX);

            Assert.Null(valueY);

            map.Merge(new Map<object, object> { { "y", "hi" } });

            Assert.Equal("hi", valueY);

            map.Merge(new Map<object, object> { { "y", "hello" } });

            Assert.Equal("hello", valueY);

            map.Replace(new Map<object, object> { { "y", "stuff" }, { "z", "zoef" } });

            Assert.Equal("stuff", valueY);

            var keys = map.Keys.ToList();
            Assert.Equal(2, keys.Count);
            Assert.Equal("y", keys[0]);
            Assert.Equal("z", keys[1]);
        }

        [Fact]
        public void TestObserver()
        {
            var map = ObservableMap<object, object>.From();
            IList<object> keys = null;
            IList<object> values = null;
            IList<KeyValuePair<object, object>> entries = null;

            Globals.Autorun((r) =>
            {
                keys = map.Keys.ToList();
            });

            Globals.Autorun((r) =>
            {
                values = map.Values.ToList();
            });

            Globals.Autorun((r) =>
            {
                entries = map.ToList();
            });

            map["a"] = 1;

            Assert.Single(keys);
            Assert.Equal("a", keys[0]);

            Assert.Single(values);
            Assert.Equal(1, values[0]);

            Assert.Single(entries);
            Assert.Equal("a", entries[0].Key);
            Assert.Equal(1, entries[0].Value);

            // should not retrigger:
            keys = null;
            values = null;
            entries = null;

            map["a"] = 1;

            Assert.Null(keys);
            Assert.Null(values);
            Assert.Null(entries);

            map["a"] = 2;

            Assert.Single(values);
            Assert.Equal(2, values[0]);

            Assert.Single(entries);
            Assert.Equal("a", entries[0].Key);
            Assert.Equal(2, entries[0].Value);


            map["b"] = 3;

            Assert.Equal(2, keys.Count);

            Assert.Equal("a", keys[0]);
            Assert.Equal("b", keys[1]);

            Assert.Equal(2, values.Count);

            Assert.Equal(2, values[0]);
            Assert.Equal(3, values[1]);

            Assert.Equal(2, entries.Count);

            Assert.Equal("a", entries[0].Key);
            Assert.Equal(2, entries[0].Value);

            Assert.Equal("b", entries[1].Key);
            Assert.Equal(3, entries[1].Value);

            map.Has("c");

            Assert.Equal(2, keys.Count);

            Assert.Equal("a", keys[0]);
            Assert.Equal("b", keys[1]);

            Assert.Equal(2, values.Count);

            Assert.Equal(2, values[0]);
            Assert.Equal(3, values[1]);

            Assert.Equal(2, entries.Count);

            Assert.Equal("a", entries[0].Key);
            Assert.Equal(2, entries[0].Value);

            Assert.Equal("b", entries[1].Key);
            Assert.Equal(3, entries[1].Value);

            map.Remove("a");

            Assert.Single(keys);
            Assert.Equal("b", keys[0]);

            Assert.Single(values);
            Assert.Equal(3, values[0]);

            Assert.Single(entries);
            Assert.Equal("b", entries[0].Key);
            Assert.Equal(3, entries[0].Value);
        }
    }
}
