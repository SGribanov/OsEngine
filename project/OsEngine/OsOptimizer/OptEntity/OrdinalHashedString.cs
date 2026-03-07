#nullable enable

/*
 * Your rights to use code governed by this license https://github.com/AlexWan/OsEngine/blob/master/LICENSE
 * Ваши права на использование кода регулируются данной лицензией http://o-s-a.net/doc/license_simple_engine.pdf
 */

using System;

namespace OsEngine.OsOptimizer.OptEntity
{
    public readonly struct OrdinalHashedString : IEquatable<OrdinalHashedString>
    {
        public static readonly OrdinalHashedString Empty = new OrdinalHashedString(string.Empty);

        public OrdinalHashedString(string value)
        {
            Value = value ?? string.Empty;
            HashCode = StringComparer.Ordinal.GetHashCode(Value);
        }

        public string Value { get; }

        public int HashCode { get; }

        public bool Equals(OrdinalHashedString other)
        {
            return HashCode == other.HashCode
                && StringComparer.Ordinal.Equals(Value, other.Value);
        }

        public override bool Equals(object? obj)
        {
            return obj is OrdinalHashedString other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode;
        }

        public static bool operator ==(OrdinalHashedString left, OrdinalHashedString right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(OrdinalHashedString left, OrdinalHashedString right)
        {
            return !(left == right);
        }
    }
}
