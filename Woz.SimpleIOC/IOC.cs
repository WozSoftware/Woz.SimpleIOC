using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace Woz.SimpleIOC
{
    public class IOC
    {
        private static readonly object LockInstance = new object();
        private static IOC _container;

        private readonly IDictionary<Identity, Func<object>> _typeMap =
            new Dictionary<Identity, Func<object>>(); 

        public static IOC Container
        {
            get
            {
                lock (LockInstance)
                {
                    if (_container == null)
                    {
                        _container = new IOC();
                    }
                }
                return _container;
            }
        }

        public void Clear()
        {
            lock (LockInstance)
            {
                _container = null;
            }
        }

        public void Register<TInterface, TImplemtation>(Func<TImplemtation> builder)
            where TInterface : class
            where TImplemtation : class, TInterface
        {
            Register<TInterface, TImplemtation>(string.Empty, ObjectLifetime.Instance, builder);
        }

        public void Register<TInterface, TImplemtation>(
            string name, Func<TImplemtation> builder)
            where TInterface : class
            where TImplemtation : class, TInterface
        {
            Register<TInterface, TImplemtation>(name, ObjectLifetime.Instance, builder);
        }

        public void Register<TInterface, TImplemtation>(
            ObjectLifetime lifetime, Func<TImplemtation> builder)
            where TInterface : class
            where TImplemtation : class, TInterface
        {
            Register<TInterface, TImplemtation>(string.Empty, lifetime, builder);
        }

        public void Register<TInterface, TImplemtation>(
            string name, ObjectLifetime lifetime, Func<TImplemtation> builder)
            where TInterface : class
            where TImplemtation : class, TInterface
        {
            Debug.Assert(builder != null);

            var wrappedBuilder = lifetime == ObjectLifetime.Instance
                ? builder
                : Singleton(builder);

            _typeMap[new Identity(typeof (TInterface), name)] = wrappedBuilder;
        }

        private static Func<object> Singleton<T>(Func<T> builder)
            where T : class
        {
            var builderLock = new object();
            T instance = null;
            return
                () =>
                {
                    lock (builderLock)
                    {
                        if (instance == null)
                        {
                            instance = builder();
                        }
                    }

                    return instance;
                };
        }

        public static TInterface Resolve<TInterface>()
        {
            return (TInterface)_container.Resolve(typeof(TInterface), string.Empty);
        }

        public static TInterface Resolve<TInterface>(string name)
        {
            return (TInterface)_container.Resolve(typeof (TInterface), name);
        }

        [SuppressMessage("ReSharper", "PossibleNullReferenceException")]
        private object Resolve(Type type, string name)
        {
            Func<object> builder;

            lock (LockInstance)
            {
                if (!_typeMap.TryGetValue(
                    new Identity(type, name),
                    out builder))
                {
                    throw new ArgumentException(
                        $"Type {type.FullName} named '{name}' not registered");
                }
            }

            return builder();
        } 
    }
}