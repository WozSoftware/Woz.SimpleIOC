#region License
// Copyright (C) Woz.Software 2015
// [https://github.com/WozSoftware/Woz.SimpleIOC]
//
// This file is part of Woz.SimpleIOC.
//
// Woz.SimpleIOC is free software: you can redistribute it 
// and/or modify it under the terms of the GNU General Public 
// License as published by the Free Software Foundation, either 
// version 3 of the License, or (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.
#endregion
using System;
using System.Collections.Generic;

namespace Woz.SimpleIOC
{
    /// <summary>
    /// Inversion of Control resolver cache. Used to register concrete 
    /// implementations for Interfaces and Abstract types. Think of as
    /// a generic abstract factory pattern.
    /// </summary>
    public class IOC
    {
        private static readonly object LockInstance = new object();
        private static bool _frozen;
        private static readonly IDictionary<Identity, Func<object>> TypeMap =
            new Dictionary<Identity, Func<object>>();

        /// <summary>
        /// Freezes the IOC. No new registrations are allowed. This removes
        /// locking contention on the IOC lookup
        /// </summary>
        public static void FreezeRegistrations()
        {
            lock (LockInstance)
            {
                _frozen = true;
            }
        }

        /// <summary>
        /// Empties the IOC resolver cache of all registrations.
        /// </summary>
        public static void Clear()
        {
            lock (LockInstance)
            {
                TypeMap.Clear();
                _frozen = false;
            }
        }

        private static Func<T> SingletonOf<T>(Func<T> builder)
            where T : class
        {
            var singletonLockInstance = new object();
            T instance = null;

            return
                () =>
                {
                    // Nested check means we only lock when required and also handle
                    // the issue where two threads get past the first if.
                    if (instance == null)
                    {
                        lock (singletonLockInstance)
                        {
                            if (instance == null)
                            {
                                instance = builder();
                            }
                        }
                    }

                    return instance;
                };
        }

        public static T DefaultBuilder<T>()
            where T : new()
            => new T();

        /// <summary>
        /// Registers an un-named singleton type in the IOC resolver cache
        /// </summary>
        /// <typeparam name="T">The type to register</typeparam>
        /// <typeparam name="TConcrete">The implementation type</typeparam>
        public static void Register<T, TConcrete>()
            where T : class
            where TConcrete : class, T, new()
            => Register<T>(DefaultBuilder<TConcrete>);

        /// <summary>
        /// Registers an un-named singleton type in the IOC resolver cache
        /// </summary>
        /// <typeparam name="T">The type to register</typeparam>
        /// <param name="builder">The concrete builder for the type</param>
        public static void Register<T>(Func<T> builder)
            where T : class
            => RegisterFor(string.Empty, ObjectLifetime.Singleton, builder);

        /// <summary>
        /// Registers a named singleton type in the IOC resolver cache
        /// </summary>
        /// <typeparam name="T">The type to register</typeparam>
        /// <typeparam name="TConcrete">The implementation type</typeparam>
        /// <param name="name">The name to register the type under</param>
        public static void Register<T, TConcrete>(object name)
            where T : class
            where TConcrete : class, T, new()
            => Register<T>(name, DefaultBuilder<TConcrete>);

        /// <summary>
        /// Registers a named singleton type in the IOC resolver cache
        /// </summary>
        /// <typeparam name="T">The type to register</typeparam>
        /// <param name="name">The name to register the type under</param>
        /// <param name="builder">The concrete builder for the type</param>
        public static void Register<T>(object name, Func<T> builder)
            where T : class
            => RegisterFor(name, ObjectLifetime.Singleton, builder);

        /// <summary>
        /// Registers an un-named type with the specified lifetime in the IOC 
        /// resolver cache
        /// </summary>
        /// <typeparam name="T">The type to register</typeparam>
        /// <typeparam name="TConcrete">The implementation type</typeparam>
        /// <param name="lifetime">The lifetime of the type</param>
        public static void Register<T, TConcrete>(ObjectLifetime lifetime)
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
        public static void Register<T>(
            ObjectLifetime lifetime, Func<T> builder)
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
        public static void Register<T, TConcrete>(
            object name, ObjectLifetime lifetime)
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
        public static void Register<T>(
            object name, ObjectLifetime lifetime, Func<T> builder)
            where T : class
            => RegisterFor(name, lifetime, builder);

        private static void RegisterFor<T>(
            object name, ObjectLifetime lifetime, Func<T> builder)
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

            lock (LockInstance)
            {
                TypeMap[Identity.For(typeof(T), name)] = wrappedBuilder;
            }
        }

        /// <summary>
        /// Resolve an un-named type from the IOC resolver cache
        /// </summary>
        /// <typeparam name="T">The type to resolve</typeparam>
        /// <returns>The concrete instance of the type</returns>
        public static T Resolve<T>()
            where T : class
            => ResolverFor<T>(string.Empty);

        /// <summary>
        /// Resolve a named type from the IOC resolver cache
        /// </summary>
        /// <typeparam name="T">The type to resolve</typeparam>
        /// <param name="name">The name of the type to resolve</param>
        /// <returns>The concrete instance of the type</returns>
        public static T Resolve<T>(object name)
            where T : class
            => ResolverFor<T>(name);

        private static T ResolverFor<T>(object name)
            where T : class
        {
            var type = typeof (T);

            Func<T> resolve =
                () =>
                {
                    Func<object> builder;
                    if (TypeMap.TryGetValue(Identity.For(type, name), out builder))
                    {
                        return builder() as T;
                    }
                    return null;
                };

            T instance;
            if (_frozen)
            {
                instance = resolve();
            }
            else
            {
                lock (LockInstance)
                {
                    instance = resolve();
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
