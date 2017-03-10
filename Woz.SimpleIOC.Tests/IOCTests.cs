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
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Woz.SimpleIOC.Tests
{
    [TestClass]
    public class IOCTests
    {
        private enum Name {ViaEnum}
        private const string Name1 = "1";
        private const string Name2 = "2";

        private interface IThing {}
        private class Thing1 : IThing {}
        private class Thing2 : IThing {}

        private interface IComplexThing
        {
            IThing Thing { get; }
        }

        private class ComplexThing : IComplexThing
        {
            public ComplexThing(IThing thing)
            {
                Thing = thing;
            }

            public IThing Thing { get; }
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void FreezeRegistrations()
        {
            var instance = IOC.Create();
            instance.FreezeRegistrations();
            instance.Register<IThing, Thing1>(ObjectLifetime.Instance);
        }


        [TestMethod]
        public void ResolveWhenFrozen()
        {
            var instance = IOC.Create();
            instance.Register<IThing>(ObjectLifetime.Instance, ioc => new Thing1());
            instance.FreezeRegistrations();

            Assert.IsNotNull(instance.Resolve<IThing>());
        }

        [TestMethod]
        public void InstanceRegistrationDefaultBuilder()
        {
            var instance = IOC.Create();
            instance.Register<IThing, Thing1>(ObjectLifetime.Instance);

            Assert.IsNotNull(instance.Resolve<IThing>());

            Assert.AreNotSame(
                instance.Resolve<IThing>(),
                instance.Resolve<IThing>());
        }

        [TestMethod]
        public void InstanceRegistration()
        {
            var instance = IOC.Create();
            instance.Register<IThing>(ObjectLifetime.Instance, ioc => new Thing1());

            Assert.IsNotNull(instance.Resolve<IThing>());

            Assert.AreNotSame(
                instance.Resolve<IThing>(),
                instance.Resolve<IThing>());
        }

        [TestMethod]
        public void SingletonRegistrationDefaultBuilder()
        {
            var instance = IOC.Create();
            instance.Register<IThing, Thing1>();

            Assert.IsNotNull(instance.Resolve<IThing>());

            Assert.AreSame(
                instance.Resolve<IThing>(),
                instance.Resolve<IThing>());
        }

        [TestMethod]
        public void SingletonRegistration()
        {
            var instance = IOC.Create();
            instance.Register<IThing>(ioc => new Thing1());

            Assert.IsNotNull(instance.Resolve<IThing>());

            Assert.AreSame(
                instance.Resolve<IThing>(),
                instance.Resolve<IThing>());
        }

        [TestMethod]
        public void NamedRegistrationDefaultBuilder()
        {
            var instance = IOC.Create();
            instance.Register<IThing, Thing1>(Name1, ObjectLifetime.Instance);
            instance.Register<IThing, Thing2>(Name2);

            Assert.AreEqual(
                instance.Resolve<IThing>(Name1).GetType().FullName,
                typeof(Thing1).FullName);

            Assert.AreEqual(
                instance.Resolve<IThing>(Name2).GetType().FullName,
                typeof(Thing2).FullName);
        }

        [TestMethod]
        public void NamedRegistration()
        {
            var instance = IOC.Create();
            instance.Register<IThing>(Name1, ObjectLifetime.Instance, ioc => new Thing1());
            instance.Register<IThing>(Name2, ioc => new Thing2());

            Assert.AreEqual(
                instance.Resolve<IThing>(Name1).GetType().FullName, 
                typeof(Thing1).FullName);

            Assert.AreEqual(
                instance.Resolve<IThing>(Name2).GetType().FullName,
                typeof(Thing2).FullName);
        }

        [TestMethod]
        public void NamedRegistrationViaEnum()
        {
            var instance = IOC.Create();
            instance.Register<IThing>(
                Name.ViaEnum, ObjectLifetime.Instance, ioc => new Thing1());

            Assert.IsNotNull(instance.Resolve<IThing>(Name.ViaEnum));
        }

        [TestMethod]
        public void GenericInterfaceResolution()
        {
            var instance = IOC.Create();
            instance.Register<IDictionary<string, IList<int>>>(
                ioc => new Dictionary<string, IList<int>>());

            Assert.IsNotNull(instance.Resolve<IDictionary<string, IList<int>>>());
        }

        [TestMethod]
        public void NestedRegistration()
        {
            var instance = IOC.Create();
            instance.Register<IThing>(ioc => new Thing1());
            instance.Register<IComplexThing>(ioc => new ComplexThing(ioc.Resolve<IThing>()));

            var resolved = instance.Resolve<IComplexThing>();
            Assert.IsNotNull(resolved);
            Assert.IsNotNull(resolved.Thing);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void UnknowRegistration()
        {
            IOC.Create().Resolve<IThing>();
        }
    }
}
