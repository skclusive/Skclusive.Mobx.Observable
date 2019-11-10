using System.Collections.Generic;
using Xunit;

namespace Skclusive.Mobx.Observable.Tests
{
    public class TestComputedValue
    {
        [Fact]
        public void TestObservableAutoRun()
        {
            var box = ObservableValue<int>.From(1);

            var reader = box as IValueReader<int>;
            var writer = box as IValueWriter<int>;

            var values = new List<int>();
            Globals.Autorun((reaction) =>
            {
                Assert.NotNull(reaction);
                if (reader.Value == -1)
                {
                    reaction.Dispose();
                }
                values.Add(reader.Value);
            });

            writer.Value = 2;
            writer.Value = 2;
            writer.Value = -1;
            writer.Value = 3;
            writer.Value = 4;

            Assert.Equal(3, values.Count);
            Assert.Equal(1, values[0]);
            Assert.Equal(2, values[1]);
            Assert.Equal(-1, values[2]);
        }


        [Fact]
        public void TestComputed()
        {
            var box = ObservableValue<int>.From(3);

            var reader = box as IValueReader<int>;
            var writer = box as IValueWriter<int>;

            var x = ComputedValue<int>.From(() => reader.Value * 2);

            var y = ComputedValue<int>.From(() => reader.Value * 3);

            Assert.Equal(6, x.Value);
            Assert.Equal(9, y.Value);

            writer.Value = 5;

            Assert.Equal(10, x.Value);
            Assert.Equal(15, y.Value);
        }


        [Fact]
        public void TestComputedAutorun()
        {
            var box = ObservableValue<int>.From(3);

            var reader = box as IValueReader<int>;
            var writer = box as IValueWriter<int>;

            var x = ComputedValue<int>.From(() => reader.Value * 2);

            var values = new List<int>();
            Globals.Autorun((reaction) =>
            {
                values.Add(x.Value);
            });

            writer.Value = 5;
            writer.Value = 10;

            Assert.Equal(3, values.Count);
            Assert.Equal(6, values[0]);
            Assert.Equal(10, values[1]);
            Assert.Equal(20, values[2]);
        }

        [Fact]
        public void TestTransaction()
        {
            var x1 = ObservableValue<int>.From(3);
            var x2 = ObservableValue<int>.From(5);

            var x1reader = x1 as IValueReader<int>;
            var x1writer = x1 as IValueWriter<int>;

            var x2reader = x2 as IValueReader<int>;
            var x2writer = x2 as IValueWriter<int>;

            var y = ComputedValue<int>.From(() => x1reader.Value + x2reader.Value);

            var values = new List<int>();

            var diposable = y.Observe(change =>
            {
                values.Add(change.NewValue);
            }, true);

            Assert.Equal(8, y.Value);

            x1writer.Value = 4;

            Assert.Equal(9, y.Value);

            Globals.Transaction(() =>
            {
                x1writer.Value = 5;
                x2writer.Value = 6;
            });

            Assert.Equal(11, y.Value);
            Assert.Equal(3, values.Count);

            Assert.Equal(8, values[0]);
            Assert.Equal(9, values[1]);
            Assert.Equal(11, values[2]);
        }


        [Fact]
        public void TestDynamic()
        {
            var x = ObservableValue<int>.From(3);

            var xreader = x as IValueReader<int>;
            var xwriter = x as IValueWriter<int>;

            var y = ComputedValue<int>.From(() => xreader.Value);

            var values = new List<int>();

            var diposable = y.Observe(change =>
            {
                values.Add(change.NewValue);
            }, true);

            Assert.Equal(3, y.Value);

            xwriter.Value = 5;

            Assert.Equal(5, y.Value);

            Assert.Equal(2, values.Count);

            Assert.Equal(3, values[0]);
            Assert.Equal(5, values[1]);
        }


        [Fact]
        public void TestDynamic2()
        {
            var x = ObservableValue<int>.From(3);

            var xreader = x as IValueReader<int>;
            var xwriter = x as IValueWriter<int>;

            var y = ComputedValue<int>.From(() => xreader.Value * xreader.Value);

            Assert.Equal(9, y.Value);

            var values = new List<int>();

            var diposable = y.Observe(change =>
            {
                values.Add(change.NewValue);
            });

            xwriter.Value = 5;

            Assert.Equal(25, y.Value);

            Assert.Single(values);
            Assert.Equal(25, values[0]);
        }

        [Fact]
        public void TestReadme1()
        {
            var order = new Order();

            var values = new List<double>();

            var diposable = order.PriceWithVat.Observe(change =>
            {
                values.Add(change.NewValue);
            });

            (order.Price as IValueWriter<double>).Value = 20;

            Assert.Single(values);
            Assert.Equal(24, values[0]);

            (order.Price as IValueWriter<double>).Value = 10;

            Assert.Equal(2, values.Count);
            Assert.Equal(24, values[0]);

            Assert.Equal(12, values[1]);
        }

        internal class Order
        {
            public Order()
            {
                Vat = ObservableValue<double>.From(0.2);

                Price = ObservableValue<double>.From(10);

                PriceWithVat = ComputedValue<double>.From(() => (Price as IValueReader<double>).Value * (1 + (Vat as IValueReader<double>).Value));
            }

            private IObservableValue<double> Vat { set; get; }

            internal IObservableValue<double> Price { private set; get; }

            internal IComputedValue<double> PriceWithVat { private set; get; }
        }


        [Fact]
        public void TestBatch()
        {
            var a = ObservableValue<int>.From(2);
            var b = ObservableValue<int>.From(3);

            var areader = a as IValueReader<int>;
            var awriter = a as IValueWriter<int>;

            var breader = b as IValueReader<int>;
            var bwriter = b as IValueWriter<int>;

            var c = ComputedValue<int>.From(() => areader.Value * breader.Value);
            var d = ComputedValue<int>.From(() => c.Value * breader.Value);

            var values = new List<int>();
            var diposable = d.Observe(change =>
            {
                values.Add(change.NewValue);
            });

            awriter.Value = 4;
            bwriter.Value = 5;

            // Note, 60 should not happen! (that is d beign computed before c after update of b)
            Assert.Equal(2, values.Count);
            Assert.Equal(36, values[0]);
            Assert.Equal(100, values[1]);

            var x = Globals.Transaction<int>(() =>
            {
                awriter.Value = 2;
                bwriter.Value = 3;
                awriter.Value = 6;

                Assert.Equal(54, d.Value);

                return 2;
            });

            Assert.Equal(2, x);
            Assert.Equal(3, values.Count);
            Assert.Equal(36, values[0]);
            Assert.Equal(100, values[1]);
            Assert.Equal(54, values[2]);
        }

        [Fact]
        public void TestTransactionInspection()
        {
            var a = ObservableValue<int>.From(2);

            var areader = a as IValueReader<int>;
            var awriter = a as IValueWriter<int>;

            var calcs = 0;
            var b = ComputedValue<int>.From(() =>
            {
                calcs++;
                return areader.Value * 2;
            });

            // if not inspected during transaction, postpone value to end
            Globals.Transaction(() =>
            {
                awriter.Value = 3;
                Assert.Equal(6, b.Value);
                Assert.Equal(1, calcs);
            });

            Assert.Equal(6, b.Value);
            Assert.Equal(2, calcs);

            // if inspected, evaluate eagerly
            Globals.Transaction(() =>
            {
                awriter.Value = 4;
                Assert.Equal(8, b.Value);
                Assert.Equal(3, calcs);
            });

            Assert.Equal(8, b.Value);
            Assert.Equal(4, calcs);
        }
    }
}
