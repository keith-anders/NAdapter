# NAdapter

`NAdapter` is a dynamic adapter library for the .NET Framework. It is intended for use cases where you have a core model that should not be changed, but you also want to expose that model to another library that examines its clients via reflection and you need it to look a little different from the actual core model. For example, it could be useful to satisfy a serializer that offers little customizability, or to expose the model to a scripting engine in a language whose conventions are different from C#'s.

## Getting Started

`NAdapter` will soon be found on NuGet and is available on .NET 4.7.1. Lower versions of .NET will be supported in the future.

```
> Install-Package NAdapter
```

For examples of how to use the library and how to build your own adapters, see the `QuickStart.cs` file in the test project.

## Running the Tests

The `NAdapter` test project uses [xUnit.net](http://github.com/xunit/xunit). You should use the xUnit test adapter to run the tests.

## Contributing

At present, there is no fixed process for contributing to `NAdapter`. If you wish to contribute or to suggest improvement items, feel free to contact the author on GitHub.

## Versioning

Use [Semantic Versioning](http://semver.org/) for versioning. For the versions available, see the [tags on this repository](http://github.com/nadapter/project/tags).

## Authors

* **Keith Anders** - *Principal development* - [Kanders](http://github.com/keith-anders)

## Acknowledgments

* Inspiration for public-facing API taken from both [Castle Proxy](http://github.com/castleproject/Core) and [Moq](https://github.com/moq/moq).

## Motivation

I wanted to serialize a public-API-facing object with a new serializer. The problem was that the serializer I wanted to use (for performance reasons) doesn't support all the customization of the serializer I've been using so far. In order to get the object to serialize the way I need it to in the new serializer, I would need to create an [adapter](http://www.tutorialspoint.com/design_pattern/adapter_pattern.htm) class that is almost identical to my public object but changes some property names, removes some properties entirely, and adds some methods with certain behaviors so the serializer can understand what I want to do. Given the hundreds of properties in the public object, I did not look forward to maintaining the link between the public class and its adapter. There had to be a better way.

I later found myself exposing a public object to a scripting engine. But I wanted to log the getters, setters, and method calls made by the script and change the method and property names from [PascalCase](https://en.wikipedia.org/wiki/PascalCase) to [camelCase](https://en.wikipedia.org/wiki/Camel_case). But I didn't want to create an adapter class that did all that, because I'd have to manually keep the adapter's properties in sync whenever I add a property to the public object. And not all the properties and methods I want to log are virtual, so [Castle Dynamic Proxy](http://github.com/castleproject/Core) wouldn't work for me either. There had to be a better way.

So I invented one. `NAdapter` is an open-source library for the .NET Framework which allows you to create adapter classes at runtime, thus replacing the manual development time of keeping adapter classes in sync with simple logic.

## FAQ

**Q:** What is the plan for this library?

**A:** See the [Roadmap](#rm) section of this document.

**Q:** Don't other libraries already do all this?

**A:** Maybe, but not that I've been able to find. All the proxy libraries I found, including [Castle Dynamic Proxy](http://github.com/castleproject/Core) work by having the proxy implement an interface or extend a base class. As such, they are only able to intercept or reimplement the virtual members. I wanted a full adapter pattern, where I have control over every member, even non-virtual ones, to modify it in arbitrary ways, including changing the name, before the type is created.

**Q:** Can the adapter implement an interface or extend a base class?

**A:** At some point that functionality may be added. But that is the edge case, not the use case, for the foreseeable future. If you want an adapter around a `T` that actually **is** a `T`, you would probably be better served by Castle Proxies than `NAdapter`. This inheritance-less strategy also serves the idea of favoring composition over inheritance.

**Q:** So what **is** the use for `NAdapter`, a proxy that does not implement an interface or extend a base class? How will you ever be able to access the members?

**A:** It is intended for consumption by code that examines its clients via reflection (such as many serializers) or by assigning to the `dynamic` keyword (such as various scripting engines). Those are the only ways to access the members. It may also become a code-generation tool at some point. (see the [Distant Future](#distant) section of the roadmap)

**Q:** Why wouldn't you use an `ExpandoObject` in that case?

**A:** If you're going to be serializing a thousand instances of a class and you need to make the exact same change to the structure of each of them, it will be much cheaper to instantiate an adapter that was specifically designed to work with that class than to create a thousand ExpandoObjects and manually populate them all correctly. Since, as far as I know, there is no built-in support for creating an `ExpandoObject` based on a template or existing class (and even if there was, an `ExpandoObject` would not have the same performance as a class specifically built to work with your model), a dynamically created adapter class remains the best choice.

## Limitations and Quirks

+ Methods can only have up to six parameters. In order to support more, I would have to add generic overloads of a number of classes and functions. This isn't too bad in itself, but--as the API is still evolving--adding twice as many overloads would mean twice as much code to refactor every time the API changes significantly. At this stage, keeping the API small and nimble is the higher priority.
+ The contents of a property's Decoration are reset (including attributes and readonly/writeonly-ness) when you change the property's behavior. As such, make sure that the `Decoration()` changes are the last changes you make to a property.
+ Make sure to use fluent syntax for working with properties, or you may not have a handle to the thing you want to have a handle to.
+ If you `SpecifyProperty` by a linq expression and the name matches an existing property but the type doesn't, you will get null back instead of the actual property, even if you set the behavior to AddOrGet. This should only happen if you've already accessed that same property and changed its type. It is because the object you get back is strongly typed on the property type, and the property that's there cannot be casted to that type since it returns a different property type. There is a `SpecifyPropertyWeak` method that allows you to retrieve the property from an expression without returning the strongly typed object.

<a name="rm"></a>
### Roadmap ###

#### Version 0.1.1 : Simple Features ####

1. Add type-converting filters on properties.
2. Add option to create a new TComponent by default.
3. Add support for other frameworks: net45, net461, netcore
4. Add attributes to more types
	1. Property getters/setters
	2. Assemblies
	3. Return types
	4. Add an attribute to all properties
5. Index properties (and attributes)
6. Add a configurable option: when adapter method is called and component is null, `enum NullBehavior { ThrowNullReferenceException, ReturnDefault }`?
7. Reuse same parameter in linq methods if applicable.

#### Version 0.1.2 : Profiling and Code Analysis ####

Benchmarking and code analysis tools. See, e.g.,  https://visualstudiomagazine.com/articles/2017/10/01/code-analysis.aspx

#### Version 0.1.3 : Complex Features ####

1. Event handlers
	1. OnGetProperty
	2. OnSetProperty
	3. OnCallMethod
	4. OnAboutToGetProperty
	5. OnAboutToSetProperty
	6. OnAboutToCallMethod
2. Events
3. Static methods
4. More complex linq expressions with a syntax that isn't awful.
5. Add more Func and Action overloads.
6. Add placeholder tests for 0.2.0 features.
7. Generic methods (and attributes)
8. ref and out parameters

#### Version 0.2.0 : Strategies ####

1. NAdapter.Strategies
2. Custom constructors
3. Attributes on constructors (and their parameters)

#### Distant Future ####
<a name="distant"></a>

1. Saving the types as an assembly which can be statically referenced at compile-time later. `NAdapter` could then be useful as a code-generation tool.
