#region License
// This file is part of Woz.SimpleIOC.
// [https://github.com/WozSoftware/Woz.SimpleIOC]
//
// This is free and unencumbered software released into the public domain.
// 
// Anyone is free to copy, modify, publish, use, compile, sell, or
// distribute this software, either in source code form or as a compiled
// binary, for any purpose, commercial or non-commercial, and by any
// means.
// 
// In jurisdictions that recognize copyright laws, the author or authors
// of this software dedicate any and all copyright interest in the
// software to the public domain. We make this dedication for the benefit
// of the public at large and to the detriment of our heirs and
// successors. We intend this dedication to be an overt act of
// relinquishment in perpetuity of all present and future rights to this
// software under copyright law.
// 
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
// IN NO EVENT SHALL THE AUTHORS BE LIABLE FOR ANY CLAIM, DAMAGES OR
// OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE,
// ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.
// 
// For more information, please refer to<http://unlicense.org>
#endregion
using System;
using System.Collections.Generic;
using static System.Threading.LazyInitializer;

namespace Woz.SimpleIOC
{
    /// <summary>
    /// Inversion of Control resolver cache. Used to register concrete 
    /// implementations for Interfaces and Abstract types. Think of as
    /// a generic abstract factory pattern.
    /// </summary>
    public class IOC
    {
        private bool _frozen;
        private readonly object _lockInstance;
        private readonly IDictionary<Identity, Func<IOC, object>> _typeMap;

        /// <summary>
        /// Create an instance of the resolver cache
        /// </summary>
        /// <returns>The resolver cache</returns>
        public static IOC Create() => new IOC();

        private IOC()
        {
            _lockInstance = new object();
            _typeMap = new Dictionary<Identity, Func<IOC, object>>();
        }

        /// <summary>
        /// Freezes the IOC. No new registrations are allowed. This removes
        /// locking contention on the IOC lookup
        /// </summary>
        public void FreezeRegistrations() => _frozen = true;

        /// <summary>
        /// Empties the IOC resolver cache of all registrations and removes
        /// the freeze lock.
        /// </summary>
        public void Clear()
        {
            lock (_lockInstance)
            {
                _typeMap.Clear();
                _frozen = false;
            }
        }

        private static Func<IOC, T> SingletonOf<T>(Func<IOC, T> builder)
            where T : class
        {
            var lockInstance = new object();
            bool initialised = false;
            T instance = null;

            return
                ioc =>
                {
                    EnsureInitialized(
                        ref instance, ref initialised, ref lockInstance, () => builder(ioc));
                    return instance;
                };
        }

        private static T DefaultBuilder<T>(IOC ioc)
            where T : new()
            => new T();

        /// <summary>
        /// Registers an un-named singleton type in the IOC resolver cache
        /// </summary>
        /// <typeparam name="T">The type to register</typeparam>
        /// <typeparam name="TConcrete">The implementation type</typeparam>
        public void Register<T, TConcrete>()
            where T : class
            where TConcrete : class, T, new()
            => Register<T>(DefaultBuilder<TConcrete>);

        /// <summary>
        /// Registers an un-named singleton type in the IOC resolver cache
        /// </summary>
        /// <typeparam name="T">The type to register</typeparam>
        /// <param name="builder">The concrete builder for the type</param>
        public void Register<T>(Func<IOC, T> builder)
            where T : class
            => RegisterFor(string.Empty, ObjectLifetime.Singleton, builder);

        /// <summary>
        /// Registers a named singleton type in the IOC resolver cache
        /// </summary>
        /// <typeparam name="T">The type to register</typeparam>
        /// <typeparam name="TConcrete">The implementation type</typeparam>
        /// <param name="name">The name to register the type under</param>
        public void Register<T, TConcrete>(object name)
            where T : class
            where TConcrete : class, T, new()
            => Register<T>(name, DefaultBuilder<TConcrete>);

        /// <summary>
        /// Registers a named singleton type in the IOC resolver cache
        /// </summary>
        /// <typeparam name="T">The type to register</typeparam>
        /// <param name="name">The name to register the type under</param>
        /// <param name="builder">The concrete builder for the type</param>
        public void Register<T>(object name, Func<IOC, T> builder)
            where T : class
            => RegisterFor(name, ObjectLifetime.Singleton, builder);

        /// <summary>
        /// Registers an un-named type with the specified lifetime in the IOC 
        /// resolver cache
        /// </summary>
        /// <typeparam name="T">The type to register</typeparam>
        /// <typeparam name="TConcrete">The implementation type</typeparam>
        /// <param name="lifetime">The lifetime of the type</param>
        public void Register<T, TConcrete>(ObjectLifetime lifetime)
            where T : class
            where TConcrete : class, T, new()
            => Register<T>(lifetime, DefaultBuilder<TConcrete>);

        /// <summary>
        /// Registers an un-named type with the specified lifetime in the IOC 
        /// resolver cache
        /// </summary>
        /// <typeparam name="T">The type to register</typeparam>
        /// <param name="lifetime">The lifetime of the type</param>
        /// <param name="builder">The concrete builder for the type</param>
        public void Register<T>(ObjectLifetime lifetime, Func<IOC, T> builder)
            where T : class
            => RegisterFor(string.Empty, lifetime, builder);

        /// <summary>
        /// Registers a named type with the specified lifetime in the IOC 
        /// resolver cache
        /// </summary>
        /// <typeparam name="T">The type to register</typeparam>
        /// <typeparam name="TConcrete">The implementation type</typeparam>
        /// <param name="name">The name to register the type under</param>
        /// <param name="lifetime">The lifetime of the type</param>
        public void Register<T, TConcrete>(object name, ObjectLifetime lifetime)
            where T : class
            where TConcrete : class, T, new()
            => Register<T>(name, lifetime, DefaultBuilder<TConcrete>);

        /// <summary>
        /// Registers a named type with the specified lifetime in the IOC 
        /// resolver cache
        /// </summary>
        /// <typeparam name="T">The type to register</typeparam>
        /// <param name="name">The name to register the type under</param>
        /// <param name="lifetime">The lifetime of the type</param>
        /// <param name="builder">The concrete builder for the type</param>
        public void Register<T>(object name, ObjectLifetime lifetime, Func<IOC, T> builder)
            where T : class
            => RegisterFor(name, lifetime, builder);

        private void RegisterFor<T>(
            object name, ObjectLifetime lifetime, Func<IOC, T> builder)
            where T : class
        {
            if (_frozen)
            {
                throw new InvalidOperationException(
                    "IOC is now locked for registrations");
            }

            var wrappedBuilder = lifetime == ObjectLifetime.Instance
                ? builder
                : SingletonOf(builder);

            lock (_lockInstance)
            {
                _typeMap[Identity.For(typeof(T), name)] = wrappedBuilder;
            }
        }

        /// <summary>
        /// Resolve an un-named type from the IOC resolver cache
        /// </summary>
        /// <typeparam name="T">The type to resolve</typeparam>
        /// <returns>The concrete instance of the type</returns>
        public T Resolve<T>()
            where T : class
            => ResolverFor<T>(string.Empty);

        /// <summary>
        /// Resolve a named type from the IOC resolver cache
        /// </summary>
        /// <typeparam name="T">The type to resolve</typeparam>
        /// <param name="name">The name of the type to resolve</param>
        /// <returns>The concrete instance of the type</returns>
        public T Resolve<T>(object name)
            where T : class
            => ResolverFor<T>(name);

        private T ResolverFor<T>(object name)
            where T : class
        {
            var type = typeof(T);

            Func<IOC, T> resolve =
                ioc => _typeMap.TryGetValue(
                    Identity.For(type, name), out Func<IOC, object> builder)
                        ? builder(ioc) as T
                        : null;

            T instance;
            if (_frozen)
            {
                instance = resolve(this);
            }
            else
            {
                lock (_lockInstance)
                {
                    instance = resolve(this);
                }
            }

            if (instance == null)
            {
                throw new InvalidOperationException(
                    $"Type {type.FullName} named '{name}' not registered or bad registration");
            }

            return instance;
        }
    }
}
