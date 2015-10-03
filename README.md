# Woz.SimpleIOC

A compact lightweight thread safe IOC library for .NET compiled to a pertable class library with support for:
- .NET Framework 4.5
- .NET Framework 4.5.1
- .NET Framework 4.6
- ASP.NET Core 5
- Windows Universal 10
- Windows 8
- Windows Phone 8.1
- Windows Phone Silverlight 8

It provides:
- Multiple named registrations per interface or class.
- Object lifetime to create instance or singleton objects.

## Background

While this might appear to be a new project with few commits it is a recent rewrite of my personal long standing IOC library that has existed for a few years. It has been used in small and large scale projects, the largest being a set of web services that runs a company backend with 500K+ lines of code.

## Sample registrations:

Register a singleton for an interface.

<code>IOC.Register&lt;IThing&gt;(() => new Thing());</code>

Register a named singleton for an interface.

<code>IOC.Register&lt;IThing&gt;("Name", () => new Thing());</code>

Register a named via enum singleton for an interface.

<code>IOC.Register&lt;IThing&gt;(EnumType.Value, () => new Thing());</code>

Register an instance for an interface.

<code>IOC.Register&lt;IThing&gt;(ObjectLifetime.Instance, () => new Thing());</code>

Register a named Instance for an interface.

<code>IOC.Register&lt;IThing&gt;("Name", ObjectLifetime.Instance, () => new Thing());</code>

Register with nested resolution.

<code>IOC.Register&lt;IThing&gt;(() => new thing(IOC.Resolve&lt;IList&lt;int&gt;&gt;()));</code>

## Sample resolutions

Resolve an instance.

<code>var instance = IOC.Resolve&lt;IThing&gt;();</code>

Resolve a named instance.

<code>var instance = IOC.Resolve&lt;IThing&gt;("Name");</code>

## Other operations 

Flush the registration list.

<code>IOC.Clear();</code>

