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
using System.Diagnostics;

namespace Woz.SimpleIOC
{
    public class IOC
    {
        private readonly IDictionary<Identity, Func<object>> _typeMap =
            new Dictionary<Identity, Func<object>>();

        private static readonly object LockInstance = new object();
        private static Func<IOC> _getContainer = GetContainerFactory();

        private static Func<IOC> GetContainerFactory()
        {
            return SingletonOf(() => new IOC());
        }

        public static void Clear()
        {
            lock (LockInstance)
            {
                _getContainer = GetContainerFactory();
            }
        }

        private static Func<T> SingletonOf<T>(Func<T> builder)
            where T : class
        {
            T instance = null;
            return
                () =>
                {
                    lock (LockInstance)
                    {
                        return instance ?? (instance = builder());
                    }
                };
        }

        public static void Register<T>(Func<T> builder)
            where T : class
        {
            _getContainer()
                .RegisterFor(string.Empty, ObjectLifetime.Singleton, builder);
        }

        public static void Register<T>(string name, Func<T> builder)
            where T : class
        {
            _getContainer()
                .RegisterFor(name, ObjectLifetime.Singleton, builder);
        }

        public static void Register<T>(
            ObjectLifetime lifetime, Func<T> builder)
            where T : class
        {
            _getContainer()
                .RegisterFor(string.Empty, lifetime, builder);
        }

        public static void Register<T>(
            string name, ObjectLifetime lifetime, Func<T> builder)
            where T : class
        {
            _getContainer()
                .RegisterFor(name, lifetime, builder);
        }

        private void RegisterFor<T>(
            string name, ObjectLifetime lifetime, Func<T> builder)
            where T : class
        {
            Debug.Assert(builder != null);

            var wrappedBuilder = lifetime == ObjectLifetime.Instance
                ? builder
                : SingletonOf(builder);

            lock (LockInstance)
            {
                _typeMap[Identity.For(typeof(T), name)] = wrappedBuilder;
            }
        }

        public static T Resolve<T>()
            where T : class
        {
            return _getContainer().ResolverFor<T>(string.Empty);
        }

        public static T Resolve<T>(string name)
            where T : class
        {
            return _getContainer().ResolverFor<T>(name);
        }

        private T ResolverFor<T>(string name)
            where T : class
        {
            var type = typeof (T);

            T instance = null;
            lock (LockInstance)
            {
                Func<object> builder;
                if (_typeMap.TryGetValue(Identity.For(type, name), out builder))
                {
                    instance = builder() as T;
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
