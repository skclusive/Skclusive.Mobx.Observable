using System;
using System.Collections.Generic;
using Xunit;

namespace Skclusive.Mobx.Observable.Tests
{
    public class TestAction
    {
        [Fact]
        public void TestWrapInTransaction()
        {
            var values = new List<int>();

            var observable = ObservableValue<int>.From(0);

            Reactions.Autorun(r =>
            {
                values.Add(observable.Value);
            });

            var increment = Actions.CreateAction<int, int>("Increment", (amount) =>
            {
                observable.Value += amount * 2;

                observable.Value -= amount; // oops

                return observable.Value;
            });

            var value = increment(7);

            Assert.Equal(7, value);

            Assert.Equal(2, values.Count);
            Assert.Equal(0, values[0]);
            Assert.Equal(7, values[1]);
        }

        [Fact]
        public void TestActionModificationPickup1()
        {
            var a = ObservableValue<int>.From(1);
            var i = 3;
            var b = 0;

            Reactions.Autorun(r =>
            {
                b = a.Value * 2;
            });

            Assert.Equal(2, b);

            var action = Actions.CreateAction("action", () =>
            {
                a.Value = ++i;
            });

            action();

            Assert.Equal(8, b);

            action();

            Assert.Equal(10, b);
        }

        [Fact]
        public void TestActionModificationPickup2()
        {
            var a = ObservableValue<int>.From(1);
            var b = 0;

            Reactions.Autorun(r =>
            {
                b = a.Value * 2;
            });

            Assert.Equal(2, b);

            var action = Actions.CreateAction("action", () =>
            {
                a.Value = a.Value + 1; // ha, no loop!
            });

            action();

            Assert.Equal(4, b);

            action();

            Assert.Equal(6, b);
        }

        [Fact]
        public void TestActionModificationPickup3()
        {
            var a = ObservableValue<int>.From(1);
            var b = 0;

            var doubler = ComputedValue<int>.From(() => a.Value * 2);

            doubler.Observe(change =>
            {
                b = doubler.Value;

            }, true);

            Assert.Equal(2, b);

            var action = Actions.CreateAction("action", () =>
            {
                a.Value = a.Value + 1; // ha, no loop!
            });

            action();

            Assert.Equal(4, b);

            action();

            Assert.Equal(6, b);
        }

        [Fact]
        public void TestActionUnTracked()
        {
            var a = ObservableValue<int>.From(3);
            var b = ObservableValue<int>.From(4);

            var latest = 0;
            var runs = 0;

            var action = Actions.CreateAction<int>("action", (baseValue) =>
            {
                b.Value = baseValue * 2;
                latest = b.Value; // without action this would trigger loop
            });

            var d = Reactions.Autorun(r =>
            {
                runs++;
                var current = a.Value;
                action(current);
            });

            Assert.Equal(6, b.Value);
            Assert.Equal(6, latest);

            a.Value = 7;

            Assert.Equal(14, b.Value);
            Assert.Equal(14, latest);

            a.Value = 8;

            Assert.Equal(16, b.Value);
            Assert.Equal(16, latest);

            b.Value = 7; // should have no effect

            Assert.Equal(8, a.Value);
            Assert.Equal(7, b.Value);
            Assert.Equal(16, latest); // effect not triggered

            a.Value = 3;

            Assert.Equal(6, b.Value);
            Assert.Equal(6, latest);

            Assert.Equal(4, runs);

            d.Dispose();
        }

        [Fact]
        public void TestAutorunInAction()
        {
            var a = ObservableValue<int>.From(1);

            var values = new List<int>();

            var adder = Actions.CreateAction<int, IDisposable>("incr", (incr) =>
            {
                return Reactions.Autorun(() =>
                {
                    values.Add(a.Value + incr);
                });
            });

            var d1 = adder(2);

            a.Value = 3;

            var d2 = adder(17);

            a.Value = 24;

            d1.Dispose();

            a.Value = 11;

            d2.Dispose();

            a.Value = 100;

            // n.b. order could swap as autorun creation order doesn't guarantee stuff
            Assert.Equal(new int[] { 3, 5, 20, 26, 41, 28 }, values.ToArray());
        }

        [Fact]
        public void TestModificationInComputed()
        {
            var a = ObservableValue<int>.From(2);

            var action = Actions.CreateAction("action", () =>
            {
                a.Value = 3;
            });

            var c = ComputedValue<object>.From(() =>
            {
                action();
                return null;
            });

            var d = Reactions.Autorun(() =>
            {
                // expect not to throws
                var x = c.Value;
            });


            d.Dispose();
        }

        [Fact]
        public void TestAllowModificationInComputed()
        {
            var a = ObservableValue<int>.From(2);
            var d = Reactions.Autorun(() => { var x = a.Value; });

            IComputedValue<int> c2 = null;

            var action = Actions.CreateAction("action", () =>
            {
                Actions.AllowStateChangesInsideComputed(() =>
                {
                    a.Value = 3;

                    //// a second level computed should throw
                    Assert.Throws<CaughtException>(() =>
                    {
                        // /Computed values are not allowed to cause side effects by changing observables that are already being observed/

                        var x = c2.Value;
                    });
                });

                Assert.Equal(3, a.Value);

                Assert.Throws<Exception>(() =>
                {
                    // /Computed values are not allowed to cause side effects by changing observables that are already being observed/

                    a.Value = 4;
                });
            });

            var c = ComputedValue<int>.From(() =>
            {
                action();
                return a.Value;
            });


            c2 = ComputedValue<int>.From(() => {
                a.Value = 6;
                return a.Value;
            });


            var _ = c.Value;

            d.Dispose();
        }

        [Fact]
        public void TestModificationErrorInComputed()
        {
            var a = ObservableValue<int>.From(2);
            var d = Reactions.Autorun(() => { var x = a.Value; });

            var action = Actions.CreateAction("action", () =>
            {
                a.Value = 3;
            });

            var c = ComputedValue<int>.From(() =>
            {
                action();
                return a.Value;
            });

            Assert.Throws<Exception>(() =>
            {
                var x = c.Value;
            });

            d.Dispose();
        }

        [Fact]
        public void TestActionAutorunUnTracked()
        {
            var a = ObservableValue<int>.From(2);
            var b = ObservableValue<int>.From(3);

            var values = new List<int>();

            var multiplier = Actions.CreateAction<int, int>("multiplier", (val) => val * b.Value);

            var d = Reactions.Autorun(() =>
            {
                values.Add(multiplier(a.Value));
            });

            a.Value = 3;
            b.Value = 4;
            a.Value = 5;

            d.Dispose();

            a.Value = 6;

            Assert.Equal(new[] { 6, 9, 20 }, values.ToArray());
        }

        [Fact]
        public void TestRunInAction()
        {
            var observable = ObservableValue<int>.From(0);

            var values = new List<int>();

            var d = Reactions.Autorun(() => values.Add(observable.Value));

            var res = Actions.RunInAction<int>("increment", () =>
            {
                observable.Value = observable.Value + 6 * 2;
                observable.Value = observable.Value - 3; // oops
                return 2;
            });

            Assert.Equal(2, res);
            Assert.Equal(new[] { 0, 9 }, values.ToArray());

            res = Actions.RunInAction<int>("another", () =>
            {
                observable.Value = observable.Value + 5 * 2;
                observable.Value = observable.Value - 4; // oops
                return 3;
            });

            Assert.Equal(3, res);
            Assert.Equal(new[] { 0, 9, 15 }, values.ToArray());

            d.Dispose();
        }

        [Fact]
        public void TestAutorunInActionDoesNotKeepComputedAlive()
        {
            var calls = 0;
            var computed = ComputedValue<int>.From(() => calls++);

            Action callComputedTwice = () =>
            {
                var x = computed.Value;
                var y = computed.Value;
            };

            Action<Action> runWithMemoizing = (fun) =>
            {
                Reactions.Autorun(fun).Dispose();
            };

            callComputedTwice();
            Assert.Equal(2, calls);

            runWithMemoizing(callComputedTwice);
            Assert.Equal(3, calls);

            callComputedTwice();
            Assert.Equal(5, calls);

            runWithMemoizing(() =>
            {
                Actions.RunInAction<int>("x", () =>
                {
                    callComputedTwice();
                    return 0;
                });
            });
            Assert.Equal(6, calls);

            callComputedTwice();
            Assert.Equal(8, calls);
        }

        [Fact]
        public void TestComputedValuesAndAction()
        {
            var calls = 0;

            var number = ObservableValue<int>.From(1);
            var squared = ComputedValue<int>.From(() =>
            {
                calls++;
                return number.Value * number.Value;
            });

            var changeNumber10Times = Actions.CreateAction("changeNumber10Times", () =>
            {
                var x = squared.Value;
                var y = squared.Value;
                for(int i = 0; i < 10; i++)
                {
                    number.Value = number.Value + 1;
                }
            });

            changeNumber10Times();
            Assert.Equal(1, calls);

            Reactions.Autorun(r =>
            {
                changeNumber10Times();
                Assert.Equal(2, calls);
            });

            Assert.Equal(2, calls);

            changeNumber10Times();
            Assert.Equal(3, calls);
        }
    }
}
