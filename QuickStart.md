# NAdapter

## Quick Start Guide

### Building a Specification

To build an adapter, you first need to build a specification. Suppose you are adapting a simple class that is declared like this:

```csharp
public class TestModel
{
  public string String { get; set; }

  public int Multiply(int x, int y) =>  x * y;
}```

The easiest way to do that is like this.

```csharp
using NAdapter;```

then...

```csharp
Specification<TestModel> spec = Specification.New<TestModel>();```

Now you have your `spec`. Working with that `spec` is the subject of the rest of this guide. You can create an adapter like so:

```csharp
TestModel model = new TestModel();
IAdapter<TestModel> adapter = spec.Finish().Create(model);```

The object passed back as `adapter` is in a class that you may think of as being declared like so:

```csharp
namespace NAdapter.Dynamic
{
  public class TestModelAdapter: IAdapter<TestModel>
  {
    // IAdapter member implementation
    public TestModel Source { get; set; }

    public string String
    {
      get { return Source.String; }
      set { Source.String = value; }
    }

    public int Multiply(int x, int y)
    {
      return Source.Multiply(x, y);
    }
  }
}```

Some notes before we move on.

- Once `Finish()` is called on a `Specification<T>`, that specification is, well, finished. It can no longer be amended. If any modifications are made to the `Specification<T>` after `Finish()` is called, they will be parsed but have no effect. If `Finish()` itself is called a second time, an exception will be thrown.
- The object returned by `AdapterFactory<T>.Create(model)` is an `IAdapter<T>` and has all the appropriate members. However, since it is created at runtime, there are only two ways to access those members: 1) Assign `adapter` to a variable of type `dynamic` and call them without compile-time type safety; or, 2) Access them via reflection. As such, the adapter is not intended to be a type that you work with statically.
- The object returned by `Specification<T>.Finish()` is an `AdapterFactory<T>`. The work performed by `Finish()` creates the `System.Type` and registers it in an assembly in the current `AppDomain`. All objects returned by a given `AdapterFactory<T>` will be of the same type, having been built by the same `Specification<T>`. Building a specification is, performance-wise, a fairly expensive operation, but creating new instances of a given type, and the operations of that type once it is finished, are fast (in some cases, faster than compiled c# code). As such, you will want to `Finish()` a single specification and cache the `AdapterFactory<T>` for ease of use in the rest of the application lifetime. Furthermore, the adapter type always has a default constructor and always implements `IAdapter<T>`, so you can write your own generic method taking `where T : IAdapter<TComponent>, new()` if you want to create your own factory.

### Properties

The most obvious thing to do to a property is to change its name.

```csharp
spec.SpecifyProperty(m => m.String, Access.Public, Behavior.AddOrGet)
  .Decoration()
  .PublicName = "FauxString";```

Let's explain this one piece at a time.

`SpecifyProperty` indicates that you are about to specify characteristics of one of the adapter's properties.

`m => m.String` is an expression which accesses the property of the adapter as it exists in the specification right now. Another overload takes the name and type of the property.

`Access.Public` is the default and could have been omitted. It merely indicates the access modifier for the property.

`Behavior.AddOrGet` is the default and could have been omitted. With this behavior, we are telling the specification that the property we wish to modify may already be present in the specification, but should be added if it is not yet present. Other available alternatives include `Add` which adds the property if it is not yet in the specification and returns null otherwise, `AddOrThrow` which adds the property if it is not yet in the specification and throws an exception otherwise, `Get`, and `GetOrThrow`, which are analogous.

`Decoration()` indicates that we are going to work with the property's decorators, as opposed to its code-behind behavior.

And, of course, changing the `PublicName` on the `PropertyDecoration` will change the name of the property. If we now finish the specification:

```csharp
TestModel model = new TestModel();
IAdapter<TestModel> adapter = spec.Finish().Create(model);```

then the code of the adapter's type looks more like this:

```csharp
// All adapters are contained in the NAdapter.Dynamic namespace, but I am omitting that for the sake of brevity.
public class TestModelAdapter: IAdapter<TestModel>
{
// IAdapter member implementation
	public TestModel Source { get; set; }

	public string FauxString
	{
		get { return Source.String; }
		set { Source.String = value; }
	}

	public int Multiply(int x, int y)
	{
		return Source.Multiply(x, y);
	}
}```

There are other members of the `PropertyDecoration` that we can work with.

+ Set `Decoration.IsHidden = true` to prevent the property from being created.
+ These will throw an exception if the current property behavior does not admit a getter.
  + Call `Decoration.SpecifyReadOnly()` to prevent the property from having a setter.
  + Call `Decoration.SpecifyPublicGetter()` to give the property a public getter.
  + Call `Decoration.SpecifyPrivateGetter()` to give the property a private getter.
+ These will throw an exception if the current property behavior does not admit a setter.
  + Call `Decoration.SpecifyWriteOnly()` to prevent the property from having a getter.
  + Call `Decoration.SpecifyPublicSetter()` to give the property a public setter.
  + Call `Decoration.SpecifyPrivateSetter()` to give the proprety a private setter.
+ Calling `SpecifyReadWrite()` gives the property both a getter and a setter and will throw an exception if the current property behavior does not admit both a getter and a setter.
+ Set `Decoration.PropertyIsVirtual = true` to make the property virtual.

Suppose we want to change the property named `String` to take an `int` instead.

```csharp
spec.SpecifyProperty(nameof(TestModel.String)).SpecifyAutoImplemented<int>();```

Now (if we `Finish()` the specification) the class will behave as though it was written like this:

```csharp
public class TestModelAdapter: IAdapter<TestModel>
{
  // IAdapter member implementation
  public TestModel Source { get; set; }

  public int String { get; set; }

  public int Multiply(int x, int y)
  {
    return Source.Multiply(x, y);
  }
}```

Note that, because we changed the behavior of the property by changing its type, we no longer have a property that exposes `Source.String`. We can fix that, however:

```csharp
spec.SpecifyProperty("SourceString").SpecifyBackingComponentProperty(t => t.String);```

And now it will look like this:

```csharp
public class TestModelAdapter: IAdapter<TestModel>
{
  // IAdapter member implementation
  public TestModel Source { get; set; }

  public int String { get; set; }
  public string SourceString
  {
    get { return Source.String; }
    set { Source.String = value; }
  }

  public int Multiply(int x, int y)
  {
    return Source.Multiply(x, y);
  }
}```

There is one more way we can change a property's behavior: by setting delegates to be called for the getter and setter. Let's start by getting a new specification.

```csharp
spec = Specification.New<TestModel>();```

Now we can reference even a local variable if we wish.

```csharp
string backingField = null;
spec.SpecifyProperty("NewProperty")
  .SpecifyDelegates(
  component => $"{component.String}.{backingField}",
  (component, value) => backingField = value);```

The delegates passed in must take the component type (in this case, `TestModel`) as their first parameter. The getter should be of type `Func<TComponent, TValue>` and the setter should be of type `Action<TComponent, TValue>` where `TValue` is the property's type. Setting these delegates will change the property's type according to the signature of the delegates passed in. The class will now look like this:

```csharp
public class TestModelAdapter: IAdapter<TestModel>
{
  // IAdapter member implementation
  public TestModel Source { get; set; }

  public string String { get; set; }

  [GeneratedCode("NAdapter", "0.1.0")]
  private class <>c__DisplayClass1
  {
    public static Func<TestModel, string> <NewProperty>k__BackingGetter;
    public static Action<TestModel, string> <NewProperty>k__BackingSetter;
  }

  public string NewProperty
  {
    get { return <>c__DisplayClass1.<NewProperty>k__BackingGetter(Source); }
    set { <>c__DisplayClass1.<NewProperty>k__BackingSetter(Source, value); }
  }

  public int Multiply(int x, int y)
  {
    return Source.Multiply(x, y);
  }
}```

where the private class `<>c__DisplayClass1`'s fields `<NewProperty>k__BackingGetter` and `<NewProperty>k__BackingSetter` contain references to the delegates that were passed in to `SpecifyDelegates` above.

If a `null` getter or setter are passed in to `SpecifyDelegates`, then the corresponding property will be read- or write-only, respectively.

### Methods

To make a method that functions like this:

```csharp
public int Sum(int _0, int _1)
{
  return _0 + _1;
}```

You can do this...				

```csharp
var sum = spec.SpecifyMethod(Access.Public, Behavior.AddOrGet)
  .WithFunctionSignature<int, int, int>("Sum")
  .SpecifyDelegate(
    Param.Arg<int>(1),
    Param.Arg<int>(2),
    (x, y) => x + y);```

Again, note that `Access.Public` and `Behavior.AddOrGet` are included for example purposes and could have been omitted. Their meanings are the same as for properties.

However, unlike for properties, note that the name of the method is passed in to `WithFunctionSignature`. This is because the specification is trying to check for collisions between different members. With properties, it is enough to check that no two properties have the same name. With methods, however, different overloads may have the same name, so both the name and the signature must be checked.

```csharp
WithFunctionSignature<TP1, TP2, TReturn>(string name, ...);```

This function tells the spec to give the method the name given in the parameter and to have a return type of `typeof(TReturn)` and to take two parameters: the first of type `typeof(TP1)` and the second of type `typeof(TP2)`. The arguments to this function after the `name` are optional and are used for specifying details of the parameters, like their names, attributes, and default values.

The `Param` class has several static members for specifying how that parameter of the method is to be fulfilled. In this case, the `SpecifyDelegate` function looks like this:

```csharp
SpecifyDelegate<TP1, TP2, TResult>(IParam<TP1> param1, IParam<TP2> param2, Func<TP1, TP2, TResult> func)```

Let's look at some other examples.

To make a method that functions like this:

```csharp
public void PrintInteger(int value)
{
  Console.WriteLine("Value: {0:x,20}", this.Source.IntegerProperty + value);
}```

Do this...

```csharp
var printInt = spec.SpecifyMethod()
  .WithActionSignature<int>("PrintInteger", ParamSettings.WithName("value"))
  .SpecifyDelegate(
    Param.Source(spec),
    Param.Arg<int>("value"),
    (c, i) => Console.WriteLine("Value: {0:x,20}", c.IntegerProperty + i));```

Given the above functions already declared, to make a method that looks like this:

```csharp
public void SetInteger(object o, int x, string y = "Hello!")
{
  return this.Source.SetInteger(this.Sum(x, int.Parse(y)));
}```

You could do this...

```csharp
    var setInt = Spec.SpecifyMethod()
    	.WithActionSignature<object, int, string>(
    		"SetInteger",
    		ParamSettings.WithName("o"),
    		ParamSettings.WithName("x"),
    		ParamSettings.WithName("y").WithDefault("Hello!"))
    	.SpecifyDelegate(
    		Param.Source(spec),
    		Param.Arg<int>("x"),
    		Param.Arg<string>("y"),
    		Param.Method(sum),
    		(component, x, y, adder) =>
    		{
    			component.SetInteger(adder(x, int.Parse(y)));
    		});```

But in cases where you are just calling a method of the backing component, Linq expressions are supported. The same thing could have been accomplished with this...

```csharp
    var setInt = Spec.SpecifyMethod()
    	.WithActionSignature<object, int, string>("SetInteger", param3: ParamSettings.WithDefault("Hello!"))
    	.SpecifyLinq(
			c => c.SetInteger(Spec.Linq.Method(sum, Spec.Linq.Arg<int>(2), int.Parse(Spec.Linq.Arg<int>(3))))));```

Another example. To make a method that looks like this...

```csharp
    public void SetMembers()
    {
    	Source.P1 = "a";
    	Source.P2 = "b";
    }```

...cannot be done with expression syntax since it does not call a method of the component type. But the delegate syntax is still available:

```csharp
    var setMembers = spec.SpecifyMethod()
    	.WithActionSignature("SetMembers")
    	.SpecifyDelegate(Param.Source(spec), c =>
    	{
    		c.P1 = "a";
    		c.P2 = "b";
    	});```

One could manually construct the linq expression to do it in lieu of the expression syntax, but the block syntax (exposed by `System.Linq.Expressions.LambdaExpression`) is not the most user-friendly.

```csharp
  ParameterExpression component = Expression.Parameter(typeof(TestModel), "component");
  var setMembers = spec.SpecifyMethod()
    .WithActionSignature("SetMembers")
    .SpecifyLinq(Expression.Lambda(component,
      Expression.Block(
        Expression.Call(typeof(TestModel).GetProperty(nameof(TestModel.P1)).GetSetMethod(), component, Expression.Constant("a")),
        Expression.Call(typeof(TestModel).GetProperty(nameof(TestModel.P2)).GetSetMethod(), component, Expression.Constant("b"))
      )));```

Using local variables is possible, though it is discouraged, as it makes it difficult for the class to stand on its own, encapsulation-wise.



```csharp
    Param.Arg<int>(1)
    Param.Arg<int>("x")
    Param.Property<int>(intMember).OrDefault(5)
    Param.Source<TComponent>(spec)
    Param.Declare<string>("cheeseburger")
    Param.Engine<TComponent>(spec)
    	a => { return a.GetProperty(otherProperty) + a.CallMethod(otherMethod) + a.GetSourceProperty(sourceProp); }```

For further examples, refer to QuickStart.cs in the unit test project.
