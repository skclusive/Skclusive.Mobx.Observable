Skclusive.Mobx.Observable
=============================

<!--- using the https://github.com/mobxjs/mobx.dart/blob/master/README.md --->

Port of [MobX](https://github.com/mobxjs/mobx) for the C# language.

> Supercharge the state-management in your Blazor apps with Transparent Functional Reactive Programming (TFRP)

## Introduction

MobX is a state-management library that makes it simple to connect the
reactive data of your application with the UI. This wiring is completely automatic
and feels very natural. As the application-developer, you focus purely on what reactive-data
needs to be consumed in the UI (and elsewhere) without worrying about keeping the two
in sync.

It's not really magic but it does have some smarts around what is being consumed (**observables**)
and where (**reactions**), and automatically tracks it for you. When the _observables_
change, all _reactions_ are re-run. What's interesting is that these reactions can be anything from a simple
console log, a network call to re-rendering the UI.

> MobX has been a very effective library for the JavaScript
> apps and this port to the C# language aims to bring the same levels of productivity.


## Core Concepts

At the heart of MobX are three important concepts: **Observables**, **Actions** and **Reactions**.

### Observables

Observables represent the reactive-state of your application. They can be simple scalars to complex object trees. By
defining the state of the application as a tree of observables, you can expose a _reactive-state-tree_ that the UI
(or other observers in the app) consume.

A simple reactive-counter is represented by the following observable:

```c#
using Skclusive.Mobx.Observable;

var counter = ObservableValue<int>.From(0);
```

More complex observables, such as classes, can be created as well.

```C#
using Skclusive.Mobx.Observable;

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
```

### Computed Observables

> What can be derived, should be derived. Automatically.

The state of your application consists of _**core-state**_ and _**derived-state**_. The _core-state_ is state inherent to the domain you are dealing with. For example, if you have a `Contact` entity, the `FirstName` and `LastName` form the _core-state_ of `Contact`. However, `FullName` is _derived-state_, obtained by combining `FirstName` and `LastName`.

Such _derived state_, that depends on _core-state_ or _other derived-state_ is called a **Computed Observable**. It is automatically kept in sync when its underlying observables change.

> State in MobX = Core-State + Derived-State

```C#
using Skclusive.Mobx.Observable;

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
```

In the example above **`FullName`** is automatically kept in sync if either `FirstName` and/or `LastName` changes.

### Actions

Actions are how you mutate the observables. Rather than mutating them directly, actions
add a semantic meaning to the mutations. For example, instead of just doing `Value++`,
firing an `Increment()` action carries more meaning. Besides, actions also batch up
all the notifications and ensure the changes are notified only after they complete.
Thus the observers are notified only upon the atomic completion of the action.

Note that actions can also be nested, in which case the notifications go out
when the top-most action has completed.

```C#
var counter = ObservableValue<int>.From(0);

var increment = Actions.CreateAction<int, int>("Increment", (amount) =>
{
    counter.Value += amount * 2;

    counter.Value -= amount; // oops

    return counter.Value;
});

increment(2);
```
### Reactions

Reactions complete the _MobX triad_ of **observables**, **actions** and **reactions**. They are
the observers of the reactive-system and get notified whenever an observable they
track is changed. Reactions come in few flavors as listed below. All of them
return a `IReactionDisposable`, a disposable that can be called to dispose the reaction.

One _striking feature_ of reactions is that they _automatically track_ all the observables without any explicit wiring. The act of _reading an observable_ within a reaction is enough to track it!

> The code you write with MobX appears to be literally ceremony-free!

**`IReactionDisposable Reactions.Autorun(Action<IReactionPublic> action)`**

Runs the reaction immediately and also on any change in the observables used inside
`action`.

```C#
using Skclusive.Mobx.Observable;

var greeting = ObservableValue<string>.From("Hello World");

var disposable = Reactions.Autorun((_) =>
{
    System.Console.WriteLine(greeting.Value);
});

greeting.Value = "Hello Mobx";

// done with Autorun
disposable.Dispose();

// Prints:
// Hello World
// Hello MobX
```

**`IReactionDisposable Reactions.Reaction<T>(Func<IReactionPublic, T> expression, Action<T, IReactionPublic> effect)`**

Monitors the observables used inside the `predicate()` function and runs the `effect()` when
the predicate returns a different value. Only the observables inside `predicate()` are tracked.

```C#
using Skclusive.Mobx.Observable;

var greeting = ObservableValue<string>.From("Hello World");

var disposable = Reactions.Reaction<string>((reaction) => greeting.Value, (value, reaction) =>
{
    System.Console.WriteLine(greeting.Value);
});

greeting.Value = "Hello Mobx";  // Cause a change

// done with reaction()
disposable.Dispose();

// Prints:
// Hello MobX
```

### Installation

Add a reference to the library from [![NuGet](https://img.shields.io/nuget/v/Skclusive.Mobx.Observable.svg)](https://www.nuget.org/packages/Skclusive.Mobx.Observable/)

## Credits

This is an attempt to port [mobx](https://github.com/mobxjs/mobx) to dotnet-standard C# libaray.

## License

Skclusive.Mobx.Observable is licensed under [MIT license](http://www.opensource.org/licenses/mit-license.php)
