using Skclusive.Mobx.Observable;
using System;
using System.Collections.Generic;
using Xunit;

namespace Skclusive.Mobx.Observable.Tests
{
    public class TestObservableObject
    {
        [Fact]
        public void TestOrder()
        {
            Func<Order, double> priceWithVatDelegate = ExpressionUtils.GetPropertyDelegate<Order, double>(o => o.PriceWithVat);

            var order = new Order() { };
            order.Price = 20;
            order.Vat = 2;

            Assert.Equal(60, priceWithVatDelegate(order));

            Assert.Equal(10, priceWithVatDelegate(new Order() { Price = 10 }));
        }

        //[Fact]
        //public void TestIOrder()
        //{

        //    Func<IOrder, double> priceWithVat = ExpressionUtils.GetPropertyDelegate<IOrder, double>(o => o.PriceWithVat);

        //    var order = ProxyFactory<Order, IOrder>.Proxy(new Order());

        //    order.Price = 20;
        //    order.Vat = 2;

        //    Assert.Equal(60, priceWithVat(order));

        //    Assert.Equal(40, priceWithVat(new TOrder { Price = 20, Vat = 2 }));

        //    Assert.Equal(10, priceWithVat(new Order() { Price = 10 }));
        //}

        internal class TOrder : IOrder
        {
            public double Vat { set; get; }

            public double Price { set; get; }

            public double PriceWithVat { get => Price * Vat; }

            public double Increment(double price)
            {
                Price += price;

                return Price;
            }
        }

        internal class OrderProxy : ObservableProxy<IOrder>, IOrder
        {
            public OrderProxy(IObservableObject<IOrder> target) : base(target)
            {
            }

            public double Vat
            {
                get => Read<double>(nameof(Vat));
                set => Write(nameof(Vat), value);
            }

            public double Price
            {
                get => Read<double>(nameof(Price));
                set => Write(nameof(Price), value);
            }

            public double PriceWithVat => Read<double>(nameof(PriceWithVat));

            public override IOrder Proxy => this;

            public double Increment(double price)
            {
                return (Target as dynamic).Increment(price);
            }
        }

        //[Fact]
        //public void TestProxy()
        //{
        //    var order = ProxyFactory<Order, IOrder>.Proxy(new Order());

        //    Assert.NotNull(order);

        //    order.Price = 10;

        //    Assert.Equal(10, order.Price);
        //    Assert.Equal(10, order.PriceWithVat);

        //    order.Price = 100;

        //    Assert.Equal(100, order.Price);
        //    Assert.Equal(100, order.PriceWithVat);
        //}

        [Fact]
        public void TestDelegate()
        {
            var x = new Order();

            var priceWithVatProperty = typeof(Order).GetProperty("PriceWithVat");

            var order = new Order();

            var priceWithVatDelegate = (Func<Order, double>)Delegate.CreateDelegate(typeof(Func<Order, double>), priceWithVatProperty.GetGetMethod());

            Assert.Equal(0, priceWithVatDelegate(new Order()));

            Assert.Equal(10, priceWithVatDelegate(new Order() { Price = 10 }));
        }

        [Fact]
        public void TestTypeDefProxy()
        {
            var typeDef = new ObservableTypeDefBuilder<IOrder>()
                .Observable(o => o.Price)
                .Observable(o => o.Vat)
                .Computed(o => o.PriceWithVat, (o) => o.Price * (1 + o.Vat))
                .Action<double, double>(o => o.Increment(0), (o, amount) => o.Price += amount)
                .Build();

            var order = ObservableObject<IOrder>.FromAs(typeDef, (x) => new OrderProxy(x), "order");

            Assert.NotNull(order);

            var prices = new List<double>();
            Globals.Autorun((r) =>
            {
                prices.Add(order.Price);
            });

            var pricesWithVat = new List<double>();
            Globals.Autorun((r) =>
            {
                pricesWithVat.Add(order.PriceWithVat);
            });

            order.Price = 10;

            Assert.Equal(10, order.Price);
            Assert.Equal(10, order.PriceWithVat);

            order.Price = 100;
            order.Vat = 2;

            Assert.Equal(100, order.Price);
            Assert.Equal(2, order.Vat);

            Assert.Equal(300, order.PriceWithVat);

            var incremented = order.Increment(20);

            Assert.Equal(120, order.Price);
            Assert.Equal(360, order.PriceWithVat);

            Assert.Equal(4, prices.Count);
            Assert.Equal(0, prices[0]);
            Assert.Equal(10, prices[1]);
            Assert.Equal(100, prices[2]);
            Assert.Equal(120, prices[3]);

            Assert.Equal(5, pricesWithVat.Count);
            Assert.Equal(0, pricesWithVat[0]);
            Assert.Equal(10, pricesWithVat[1]);
            Assert.Equal(100, pricesWithVat[2]);
            Assert.Equal(300, pricesWithVat[3]);
            Assert.Equal(360, pricesWithVat[4]);
        }

        [Fact]
        public void TestTypeDefActAs()
        {
            var typeDef = new ObservableTypeDefBuilder<IOrder>()
                .Observable(o => o.Price)
                .Observable(o => o.Vat)
                .Computed(o => o.PriceWithVat, (o) => o.Price * (1 + o.Vat))
                .Action<double, double>(o => o.Increment(0), (o, amount) => o.Price += amount)
                .Build();

            var order = ObservableObject<IOrder>.FromAs(typeDef, (x) => x.ActAs<IOrder>(typeof(IObservableObject<IOrder>)), "order");

            Assert.NotNull(order);

            var prices = new List<double>();
            Globals.Autorun((r) =>
            {
                prices.Add(order.Price);
            });

            var pricesWithVat = new List<double>();
            Globals.Autorun((r) =>
            {
                pricesWithVat.Add(order.PriceWithVat);
            });

            order.Price = 10;

            Assert.Equal(10, order.Price);
            Assert.Equal(10, order.PriceWithVat);

            order.Price = 100;
            order.Vat = 2;

            Assert.Equal(100, order.Price);
            Assert.Equal(2, order.Vat);

            Assert.Equal(300, order.PriceWithVat);

            var incremented = order.Increment(20);

            Assert.Equal(120, order.Price);
            Assert.Equal(360, order.PriceWithVat);

            Assert.Equal(4, prices.Count);
            Assert.Equal(0, prices[0]);
            Assert.Equal(10, prices[1]);
            Assert.Equal(100, prices[2]);
            Assert.Equal(120, prices[3]);

            Assert.Equal(5, pricesWithVat.Count);
            Assert.Equal(0, pricesWithVat[0]);
            Assert.Equal(10, pricesWithVat[1]);
            Assert.Equal(100, pricesWithVat[2]);
            Assert.Equal(300, pricesWithVat[3]);
            Assert.Equal(360, pricesWithVat[4]);
        }

        //[Fact]
        //public void TestObject()
        //{
        //    var order = ObservableObject<IOrder>.FromAs(new Order(), null, "order");

        //    Assert.NotNull(order);

        //    var prices = new List<double>();
        //    Globals.Autorun((r) =>
        //    {
        //        prices.Add(order.Price);
        //    });

        //    var pricesWithVat = new List<double>();
        //    Globals.Autorun((r) =>
        //    {
        //        pricesWithVat.Add(order.PriceWithVat);
        //    });

        //    order.Price = 10;

        //    Assert.Equal(10, order.Price);
        //    Assert.Equal(10, order.PriceWithVat);

        //    order.Price = 100;
        //    order.Vat = 2;

        //    Assert.Equal(100, order.Price);
        //    Assert.Equal(2, order.Vat);

        //    Assert.Equal(300, order.PriceWithVat);

        //    Assert.Equal(3, prices.Count);
        //    Assert.Equal(0, prices[0]);
        //    Assert.Equal(10, prices[1]);
        //    Assert.Equal(100, prices[2]);

        //    Assert.Equal(4, pricesWithVat.Count);
        //    Assert.Equal(0, pricesWithVat[0]);
        //    Assert.Equal(10, pricesWithVat[1]);
        //    Assert.Equal(100, pricesWithVat[2]);
        //    Assert.Equal(300, pricesWithVat[3]);
        //}

        public interface IOrder
        {
            [Observable]
            double Vat { get; set; }

            [Observable]
            double Price { get; set; }

            [Computed]
            double PriceWithVat { get; }

            double Increment(double price);
        }

        public class Order : IOrder
        {
            [Observable]
            public double Vat { set; get; }

            [Observable]
            public double Price { set; get; }

            [Computed]
            public double PriceWithVat { get => Price * (1 + Vat); }

            public double Increment(double amount)
            {
                Price += amount;

                return Price;
            }
        }
    }
}
