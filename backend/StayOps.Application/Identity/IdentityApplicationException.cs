namespace StayOps.Application.Identity;

public class IdentityApplicationException : Exception
{
    public IdentityApplicationException(string message)
        : base(message)
    {
    }
}
