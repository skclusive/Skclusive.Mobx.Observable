using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Skclusive.Mobx.Observable.Tests
{
    public class TestObservableList
    {
        [Fact]
        public void TestSetup()
        {
            var list = ObservableList<int>.From();

            Assert.Empty(list);

            list.Add(1);

            Assert.Single(list);
            Assert.Equal(1, list[0]);

            list[1] = 2;

            Assert.Equal(2, list.Length);
            Assert.Equal(1, list[0]);
            Assert.Equal(2, list[1]);

            var compute = ComputedValue<int>.From(() =>
            {
                return -1 + list.Aggregate(1, (acc, curr) => acc + curr);
            });

            Assert.Equal(3, compute.Value);

            list[1] = 3;

            Assert.Equal(2, list.Length);
            Assert.Equal(1, list[0]);
            Assert.Equal(3, list[1]);
            Assert.Equal(4, compute.Value);

            list.Splice(1, 1, 4, 5);

            Assert.Equal(3, list.Length);
            Assert.Equal(1, list[0]);
            Assert.Equal(4, list[1]);
            Assert.Equal(5, list[2]);
            Assert.Equal(10, compute.Value);

            list.Replace(new[] { 2, 4 });

            Assert.Equal(2, list.Length);
            Assert.Equal(2, list[0]);
            Assert.Equal(4, list[1]);
            Assert.Equal(6, compute.Value);

            list.Splice(1, 1);

            Assert.Equal(1, list.Length);
            Assert.Equal(2, list[0]);
            Assert.Equal(2, compute.Value);

            list.Splice(0, 0, new[] { 4, 3 });

            Assert.Equal(3, list.Length);
            Assert.Equal(4, list[0]);
            Assert.Equal(3, list[1]);
            Assert.Equal(2, list[2]);
            Assert.Equal(9, compute.Value);

            list.Clear();

            Assert.Empty(list);
            Assert.Equal(0, compute.Value);

            list.Length = 4;

            Assert.Equal(4, list.Length);
            Assert.Equal(0, compute.Value);

            list.Replace(new[] { 1, 2, 2, 4 });

            Assert.Equal(4, list.Length);
            Assert.Equal(9, compute.Value);

            list.Length = 4;

            Assert.Equal(4, list.Length);
            Assert.Equal(9, compute.Value);

            list.Length = 2;

            Assert.Equal(2, list.Length);
            Assert.Equal(3, compute.Value);
            Assert.Equal(1, list[0]);
            Assert.Equal(2, list[1]);

            list.Unshift(3);

            Assert.Equal(3, list.Length);
            Assert.Equal(6, compute.Value);
            Assert.Equal(3, list[0]);
            Assert.Equal(1, list[1]);
            Assert.Equal(2, list[2]);

            list[2] = 4;

            Assert.Equal(3, list.Length);
            Assert.Equal(8, compute.Value);
            Assert.Equal(3, list[0]);
            Assert.Equal(1, list[1]);
            Assert.Equal(4, list[2]);
        }


        [Fact]
        public void TestObserver()
        {
            var list = ObservableList<int>.From(new[] { 1, 4 });

            var changes = new List<IListDidChange<int>>();

            var disposable = list.Observe(change => changes.Add(change), true);

            list[1] = 3; // 1, 3
            list[2] = 0; // 1, 3, 0

            Assert.Equal(3, list.Length);

            list.Shift(); // 3, 0
            list.Push(1, 2); // 3, 0, 1, 2
            list.Splice(1, 2, 3, 4); // 3, 3, 4, 2

            Assert.Equal(4, list.Length);
            Assert.Equal(3, list[0]);
            Assert.Equal(3, list[1]);
            Assert.Equal(4, list[2]);
            Assert.Equal(2, list[3]);

            list.Splice(6);
            list.Splice(6, 2);
            list.Replace(new[] { 6 });
            list.Pop();

            Assert.Throws<InvalidOperationException>(() => list.Pop());

            Assert.Equal(8, changes.Count);

            Assert.Equal(list, changes[0].Object);
            Assert.Equal(ChangeType.SPLICE, changes[0].Type);
            Assert.Equal(0, changes[0].Index);
            Assert.Equal(2, changes[0].AddedCount);
            Assert.Equal(0, changes[0].RemovedCount);
            Assert.Empty(changes[0].Removed);
            Assert.Equal(2, changes[0].Added.Length);
            Assert.Equal(1, changes[0].Added[0]);
            Assert.Equal(4, changes[0].Added[1]);


            Assert.Equal(list, changes[1].Object);
            Assert.Equal(ChangeType.UPDATE, changes[1].Type);
            Assert.Equal(1, changes[1].Index);
            Assert.Equal(4, changes[1].OldValue);
            Assert.Equal(3, changes[1].NewValue);

            Assert.Equal(list, changes[2].Object);
            Assert.Equal(ChangeType.SPLICE, changes[2].Type);
            Assert.Equal(2, changes[2].Index);
            Assert.Equal(1, changes[2].AddedCount);
            Assert.Equal(0, changes[2].RemovedCount);
            Assert.Empty(changes[2].Removed);
            Assert.Single(changes[2].Added);
            Assert.Equal(0, changes[2].Added[0]);

            Assert.Equal(list, changes[3].Object);
            Assert.Equal(ChangeType.SPLICE, changes[3].Type);
            Assert.Equal(0, changes[3].Index);
            Assert.Equal(0, changes[3].AddedCount);
            Assert.Equal(1, changes[3].RemovedCount);
            Assert.Single(changes[3].Removed);
            Assert.Empty(changes[3].Added);
            Assert.Equal(1, changes[3].Removed[0]);

            Assert.Equal(list, changes[4].Object);
            Assert.Equal(ChangeType.SPLICE, changes[4].Type);
            Assert.Equal(2, changes[4].Index);
            Assert.Equal(2, changes[4].AddedCount);
            Assert.Equal(0, changes[4].RemovedCount);
            Assert.Empty(changes[4].Removed);
            Assert.Equal(2, changes[4].Added.Length);
            Assert.Equal(1, changes[4].Added[0]);
            Assert.Equal(2, changes[4].Added[1]);

            Assert.Equal(list, changes[5].Object);
            Assert.Equal(ChangeType.SPLICE, changes[5].Type);
            Assert.Equal(1, changes[5].Index);
            Assert.Equal(2, changes[5].AddedCount);
            Assert.Equal(2, changes[5].RemovedCount);
            Assert.Equal(2, changes[5].Removed.Length);
            Assert.Equal(2, changes[5].Added.Length);
            Assert.Equal(0, changes[5].Removed[0]);
            Assert.Equal(1, changes[5].Removed[1]);
            Assert.Equal(3, changes[5].Added[0]);
            Assert.Equal(4, changes[5].Added[1]);

            Assert.Equal(list, changes[6].Object);
            Assert.Equal(ChangeType.SPLICE, changes[6].Type);
            Assert.Equal(0, changes[6].Index);
            Assert.Equal(1, changes[6].AddedCount);
            Assert.Equal(4, changes[6].RemovedCount);
            Assert.Equal(4, changes[6].Removed.Length);
            Assert.Single(changes[6].Added);
            Assert.Equal(3, changes[6].Removed[0]);
            Assert.Equal(3, changes[6].Removed[1]);
            Assert.Equal(4, changes[6].Removed[2]);
            Assert.Equal(2, changes[6].Removed[3]);
            Assert.Equal(6, changes[6].Added[0]);

            Assert.Equal(list, changes[7].Object);
            Assert.Equal(ChangeType.SPLICE, changes[7].Type);
            Assert.Equal(0, changes[7].Index);
            Assert.Equal(0, changes[7].AddedCount);
            Assert.Equal(1, changes[7].RemovedCount);
            Assert.Single(changes[7].Removed);
            Assert.Empty(changes[7].Added);
            Assert.Equal(6, changes[7].Removed[0]);
        }

        [Fact]
        public void TestAutorun1()
        {
            var list = ObservableList<int>.From();
            var count = 0;

            Reactions.Autorun((r) =>
            {
                var x = list.ToString();
                count++;
            });

            list.Push(1);

            Assert.Equal(2, count);
        }

        [Fact]
        public void TestAutorun2()
        {
            var list = ObservableList<int>.From(new[] { 4, 2, 3 });
            List<int> sorted = null;

            var sortedX = ComputedValue<List<int>>.From(() =>
            {
                 var splice = list.ToList();
                 splice.Sort();
                 return splice;
            });

            Reactions.Autorun((r) =>
            {
                sorted = sortedX.Value;
            });

            Assert.Equal(4, list[0]);
            Assert.Equal(2, list[1]);
            Assert.Equal(3, list[2]);

            Assert.Equal(2, sorted[0]);
            Assert.Equal(3, sorted[1]);
            Assert.Equal(4, sorted[2]);

            list.Add(1);

            Assert.Equal(4, list[0]);
            Assert.Equal(2, list[1]);
            Assert.Equal(3, list[2]);
            Assert.Equal(1, list[3]);

            Assert.Equal(1, sorted[0]);
            Assert.Equal(2, sorted[1]);
            Assert.Equal(3, sorted[2]);
            Assert.Equal(4, sorted[3]);

            list.Shift();

            Assert.Equal(2, list[0]);
            Assert.Equal(3, list[1]);
            Assert.Equal(1, list[2]);

            Assert.Equal(1, sorted[0]);
            Assert.Equal(2, sorted[1]);
            Assert.Equal(3, sorted[2]);
        }
    }
}
