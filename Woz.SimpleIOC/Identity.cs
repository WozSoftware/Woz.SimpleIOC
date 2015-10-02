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

namespace Woz.SimpleIOC
{
    internal sealed class Identity
    {
        public Type ToResolve { get; }
        public string Name { get; }

        public static Identity For(Type toResolve, string name)
        {
            return new Identity(toResolve, name);
        }

        private Identity(Type toResolve, string name)
        {
            ToResolve = toResolve;
            Name = name;
        }

        public override bool Equals(object obj)
        {
            var other = obj as Identity;
            if (other == null)
            {
                return false;
            }

            return
                ToResolve == other.ToResolve &&
                Name == other.Name;
        }

        public override int GetHashCode()
        {
            return ToResolve.GetHashCode() | Name.GetHashCode();
        }
    }
}
