# Woz.SimpleIOC

Now released under Unlicense (http://unlicense.org)

A compact lightweight thread safe IOC library for .NET compiled against .NETStandard 1.6

It provides:
- Multiple named registrations per interface or class.
- Object lifetime to create instance or singleton objects.

## Background

While this might appear to be a new project with few commits it is a recent rewrite of my personal long standing IOC library that has existed for a few years. It has been used in small and large scale projects, the largest being a set of web services that runs a company backend with 500K+ lines of code.

## Create container:

<code>var ioc = IOC.Create();</code>

## Sample registrations:

Register a singleton for an interface.

<code>ioc.Register&lt;IThing&gt;(ioc => new Thing());</code>

Register a named singleton for an interface.

<code>ioc.Register&lt;IThing&gt;("Name", ioc => new Thing());</code>

Register a named via enum singleton for an interface.

<code>ioc.Register&lt;IThing&gt;(EnumType.Value, ioc => new Thing());</code>

Register an instance for an interface.

<code>ioc.Register&lt;IThing&gt;(ObjectLifetime.Instance, ioc => new Thing());</code>

Register a named Instance for an interface.

<code>ioc.Register&lt;IThing&gt;("Name", ObjectLifetime.Instance, ioc => new Thing());</code>

Register with nested resolution.

<code>ioc.Register&lt;IThing&gt;(ioc => new Thing(ioc.Resolve&lt;IList&lt;int&gt;&gt;()));</code>

Registration with default builder, all registration methods support this style.

<code>ioc.Register&lt;IThing, Thing&gt;();</code>

## Sample resolutions

Resolve an instance.

<code>var instance = ioc.Resolve&lt;IThing&gt;();</code>

Resolve a named instance.

<code>var instance = ioc.Resolve&lt;IThing&gt;("Name");</code>

## Other operations 

Flush the registration list and unfreeze.

<code>ioc.Clear();</code>

Freeze the IOC container to remove most thread locking contentions

<code>ioc.FreezeRegistrations();</code>
