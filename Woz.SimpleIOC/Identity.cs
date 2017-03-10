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

namespace Woz.SimpleIOC
{
    internal sealed class Identity : IEquatable<Identity>
    {
        public Type ToResolve { get; }
        public string Name { get; }

        public static Identity For(Type toResolve, object name)
            => new Identity(toResolve, name.ToString());

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

            return Equals(other);
        }

        public bool Equals(Identity other)
            => ToResolve == other.ToResolve && Name == other.Name;

        public override int GetHashCode()
        {
            unchecked
            {
                return (ToResolve.GetHashCode() * 23) + Name.GetHashCode();
            }
        }
    }
}
