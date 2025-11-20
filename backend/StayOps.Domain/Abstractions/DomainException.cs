namespace StayOps.Domain.Abstractions;

public class DomainException : Exception
{
    public DomainException(string message)
        : base(message)
    {
    }
}
