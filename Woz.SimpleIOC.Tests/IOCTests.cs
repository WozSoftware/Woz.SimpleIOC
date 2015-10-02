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
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Woz.SimpleIOC.Tests
{
    [TestClass]
    public class IOCTests
    {
        public enum Name {ViaEnum}
        private const string Name1 = "1";
        private const string Name2 = "2";

        private interface IThing {}
        private class Thing1 : IThing {}
        private class Thing2 : IThing { }

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

        [TestCleanup]
        public void Cleanup()
        {
            IOC.Clear();
        }

        [TestMethod]
        public void InstanceRegistration()
        {
            IOC.Register<IThing>(
                ObjectLifetime.Instance, () => new Thing1());

            Assert.IsNotNull(IOC.Resolve<IThing>());

            Assert.AreNotSame(
                IOC.Resolve<IThing>(), 
                IOC.Resolve<IThing>());
        }

        [TestMethod]
        public void SingletonRegistration()
        {
            IOC.Register<IThing>(() => new Thing1());

            Assert.IsNotNull(IOC.Resolve<IThing>());

            Assert.AreSame(
                IOC.Resolve<IThing>(),
                IOC.Resolve<IThing>());
        }

        [TestMethod]
        public void NamedRegistration()
        {
            IOC.Register<IThing>(
                Name1, ObjectLifetime.Instance, () => new Thing1());
            IOC.Register<IThing>(
                Name2, ObjectLifetime.Singleton, () => new Thing2());

            Assert.AreEqual(
                IOC.Resolve<IThing>(Name1).GetType().FullName, 
                typeof(Thing1).FullName);

            Assert.AreEqual(
                IOC.Resolve<IThing>(Name2).GetType().FullName,
                typeof(Thing2).FullName);
        }

        [TestMethod]
        public void NamedRegistrationViaEnum()
        {
            IOC.Register<IThing>(
                Name.ViaEnum, ObjectLifetime.Instance, () => new Thing1());

            Assert.IsNotNull(IOC.Resolve<IThing>(Name.ViaEnum));
        }

        [TestMethod]
        public void GenericInterfaceResolution()
        {
            IOC.Register<IDictionary<string, IList<int>>>(
                () => new Dictionary<string, IList<int>>());

            Assert.IsNotNull(IOC.Resolve<IDictionary<string, IList<int>>>());
        }

        [TestMethod]
        public void NestedRegistration()
        {
            IOC.Register<IThing>(() => new Thing1());
            IOC.Register<IComplexThing>(() => new ComplexThing(IOC.Resolve<IThing>()));

            var instance = IOC.Resolve<IComplexThing>();
            Assert.IsNotNull(instance);
            Assert.IsNotNull(instance.Thing);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void UnknowRegistration()
        {
            IOC.Resolve<IThing>();
        }

    }
}
