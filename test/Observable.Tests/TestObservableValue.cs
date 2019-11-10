using System.Collections.Generic;
using Xunit;

namespace Skclusive.Mobx.Observable.Tests
{
    public class TestObservableValue
    {
        [Fact]
        public void TestNullValue()
        {
            var box = ObservableValue<object>.From(null);

            var reader = box as IValueReader;

            Assert.Null(reader.Value);
        }

        [Fact]
        public void TestIntValue()
        {
            var box = ObservableValue<int>.From(5);

            var reader = box as IValueReader;

            Assert.Equal(5, reader.Value);
        }

        [Fact]
        public void TestBasic()
        {
            var box = ObservableValue<int>.From(3);

            var reader = box as IValueReader<int>;
            var writer = box as IValueWriter<int>;

            var values = new List<int>();
            box.Observe(change => values.Add(change.NewValue));

            Assert.Equal(3, reader.Value);

            writer.Value = 5;

            Assert.Equal(5, reader.Value);

            Assert.Single(values);
            Assert.Equal(5, values[0]);
        }

        [Fact]
        public void TestObserveDisposal()
        {
            var box = ObservableValue<int>.From(3);

            var reader = box as IValueReader<int>;
            var writer = box as IValueWriter<int>;

            var values = new List<int>();

            var diposable = box.Observe(change => values.Add(change.NewValue));

            writer.Value = 5;
            writer.Value = 6;

            diposable.Dispose();

            writer.Value = 7;

            Assert.Equal(2, values.Count);
            Assert.Equal(5, values[0]);
            Assert.Equal(6, values[1]);
            Assert.Equal(7, reader.Value);
        }
    }
}
