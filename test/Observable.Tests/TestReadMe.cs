using System.Collections.Generic;
using Xunit;

namespace Skclusive.Mobx.Observable.Tests
{
    public class TestReadMe
    {
        [Fact]
        public void TestCounter()
        {
            var counter = new Counter();

            var counts = new List<int>();

            Reactions.Autorun(() =>
            {
                counts.Add(counter.Count);
            });

            counter.Increment();
            counter.Increment();

            Assert.Equal(3, counts.Count);
            Assert.Equal(0, counts[0]);
            Assert.Equal(1, counts[1]);
            Assert.Equal(2, counts[2]);
        }

        public class Counter
        {
            private readonly IObservableValue<int> _count = ObservableValue<int>.From(0);

            public int Count
            {
                get => _count.Value;
                set => _count.Value = value;
            }

            public void Increment()
            {
                _count.Value++;
            }
        }

        [Fact]
        public void TestContact()
        {
            var contact = new Contact();

            var fullNames = new List<string>();

            contact.FirstName = "Skclusive";

            contact.LastName = "Sk";

            Reactions.Autorun(() =>
            {
                fullNames.Add(contact.FullName);
            });

            contact.FirstName = "Naguvan";

            contact.LastName = "Senthilnathan";

            Assert.Equal(3, fullNames.Count);
            Assert.Equal("Skclusive, Sk", fullNames[0]);
            Assert.Equal("Naguvan, Sk", fullNames[1]);
            Assert.Equal("Naguvan, Senthilnathan", fullNames[2]);
        }

        public class Contact
        {
            private readonly IObservableValue<string> _firstName;

            private readonly IObservableValue<string> _lastName;

            private readonly IComputedValue<string> _fullName;

            public Contact()
            {
                _firstName = ObservableValue<string>.From();

                _lastName = ObservableValue<string>.From();

                _fullName = ComputedValue<string>.From(() => $"{FirstName}, {LastName}");
            }

            public string FirstName
            {
                get => _firstName.Value;
                set => _firstName.Value = value;
            }

            public string LastName
            {
                get => _lastName.Value;
                set => _lastName.Value = value;
            }

            public string FullName => _fullName.Value;
        }

        [Fact]
        public void TestGreeting()
        {
            var greeting = ObservableValue<string>.From("Hello World");

            var greetings = new List<string>();

            var disposable = Reactions.Autorun((_) =>
            {
                // System.Console.WriteLine(greeting.Value);

                greetings.Add(greeting.Value);
            });

            greeting.Value = "Hello Mobx";

            // done with Autorun()
            disposable.Dispose();

            Assert.Equal(2, greetings.Count);
            Assert.Equal("Hello World", greetings[0]);
            Assert.Equal("Hello Mobx", greetings[1]);
        }

        [Fact]
        public void TestReaction()
        {
            var greeting = ObservableValue<string>.From("Hello World");

            var greetings = new List<string>();

            var disposable = Reactions.Reaction<string>((reaction) => greeting.Value, (value, reaction) =>
            {
                // System.Console.WriteLine(greeting.Value);

                greetings.Add(greeting.Value);
            });

            greeting.Value = "Hello Mobx";  // Cause a change

            // done with reaction()
            disposable.Dispose();

            Assert.Single(greetings);
            Assert.Equal("Hello Mobx", greetings[0]);
        }
    }
}
