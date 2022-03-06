// Copyright (c) Arjen Post. See LICENSE in the project root for license information.

namespace Giddup.Domain;

public abstract class ValueObject
{
    public static bool operator ==(ValueObject obj1, ValueObject obj2) => obj1.Equals(obj2);

    public static bool operator !=(ValueObject obj1, ValueObject obj2) => !obj1.Equals(obj2);

    public override bool Equals(object? obj)
    {
        if (obj == null || obj.GetType() != GetType())
        {
            return false;
        }

        var other = (ValueObject)obj;

        return GetEqualityComponents().SequenceEqual(other.GetEqualityComponents());
    }

    public override int GetHashCode()
        => GetEqualityComponents()
            .Select(equalityComponents => equalityComponents.GetHashCode())
            .Aggregate((result, hashCode) => result ^ hashCode);

    protected abstract IEnumerable<object> GetEqualityComponents();
}
