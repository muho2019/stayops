using System.Text.RegularExpressions;
using StayOps.Domain.Abstractions;

namespace StayOps.Domain.Identity;

public sealed class Email : IEquatable<Email>
{
    private static readonly Regex EmailRegex = new(
        @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
        RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);

    private Email(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static Email Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new DomainException("Email is required.");
        }

        string normalized = value.Trim().ToLowerInvariant();
        if (!EmailRegex.IsMatch(normalized))
        {
            throw new DomainException("Email format is invalid.");
        }

        return new Email(normalized);
    }

    public bool Equals(Email? other)
    {
        if (ReferenceEquals(null, other))
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        return string.Equals(Value, other.Value, StringComparison.Ordinal);
    }

    public override bool Equals(object? obj)
    {
        return ReferenceEquals(this, obj) || obj is Email other && Equals(other);
    }

    public override int GetHashCode()
    {
        return StringComparer.Ordinal.GetHashCode(Value);
    }

    public override string ToString()
    {
        return Value;
    }
}
