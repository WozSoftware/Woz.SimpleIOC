using System;

namespace Woz.SimpleIOC
{
    internal sealed class Identity
    {
        public Identity(Type toResolve, string name)
        {
            ToResolve = toResolve;
            Name = name;
        }

        public Type ToResolve { get; }
        public string Name { get; }

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
