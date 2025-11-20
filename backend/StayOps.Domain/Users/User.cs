using StayOps.Domain.Abstractions;

namespace StayOps.Domain.Users;

public sealed class User : Entity<UserId>
{
    private User()
        : base(default)
    {
        Email = null!;
        PasswordHash = null!;
        Name = null!;
    }

    private User(
        UserId id,
        Email email,
        string passwordHash,
        string name,
        RoleId roleId,
        UserStatus status,
        DateTimeOffset createdAt,
        UserId createdBy)
        : base(id)
    {
        Email = email ?? throw new ArgumentNullException(nameof(email));
        PasswordHash = !string.IsNullOrWhiteSpace(passwordHash)
            ? passwordHash
            : throw new DomainException("Password hash is required.");
        Name = !string.IsNullOrWhiteSpace(name)
            ? name.Trim()
            : throw new DomainException("Name is required.");
        RoleId = roleId;
        Status = status;
        CreatedAt = createdAt;
        CreatedBy = createdBy;
        UpdatedAt = createdAt;
        UpdatedBy = createdBy;
    }

    public Email Email { get; private set; }

    public string PasswordHash { get; private set; }

    public string Name { get; private set; }

    public UserStatus Status { get; private set; }

    public RoleId RoleId { get; private set; }

    public DateTimeOffset CreatedAt { get; }

    public UserId CreatedBy { get; }

    public DateTimeOffset UpdatedAt { get; private set; }

    public UserId UpdatedBy { get; private set; }

    public static User Create(
        UserId id,
        Email email,
        string passwordHash,
        string name,
        RoleId roleId,
        DateTimeOffset createdAt,
        UserId createdBy,
        UserStatus status = UserStatus.Active)
    {
        return new User(id, email, passwordHash, name, roleId, status, createdAt, createdBy);
    }

    public void Activate(UserId actor, DateTimeOffset occurredAt)
    {
        if (Status == UserStatus.Active)
        {
            return;
        }

        Status = UserStatus.Active;
        Touch(actor, occurredAt);
    }

    public void Deactivate(UserId actor, DateTimeOffset occurredAt)
    {
        if (Status == UserStatus.Inactive)
        {
            return;
        }

        Status = UserStatus.Inactive;
        Touch(actor, occurredAt);
    }

    public void ChangePasswordHash(string passwordHash, UserId actor, DateTimeOffset occurredAt)
    {
        if (string.IsNullOrWhiteSpace(passwordHash))
        {
            throw new DomainException("Password hash is required.");
        }

        PasswordHash = passwordHash;
        Touch(actor, occurredAt);
    }

    public void ChangeRole(RoleId roleId, UserId actor, DateTimeOffset occurredAt)
    {
        RoleId = roleId;
        Touch(actor, occurredAt);
    }

    public void Rename(string name, UserId actor, DateTimeOffset occurredAt)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainException("Name is required.");
        }

        Name = name.Trim();
        Touch(actor, occurredAt);
    }

    public void ChangeEmail(Email email, UserId actor, DateTimeOffset occurredAt)
    {
        Email = email ?? throw new ArgumentNullException(nameof(email));
        Touch(actor, occurredAt);
    }

    private void Touch(UserId actor, DateTimeOffset occurredAt)
    {
        UpdatedAt = occurredAt;
        UpdatedBy = actor;
    }
}
