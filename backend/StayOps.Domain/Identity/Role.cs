using StayOps.Domain.Abstractions;

namespace StayOps.Domain.Identity;

public sealed class Role : Entity<RoleId>
{
    private readonly HashSet<Permission> _permissions = new();

    private Role()
        : base(default)
    {
        Name = string.Empty;
    }

    private Role(RoleId id, string name, IEnumerable<Permission> permissions)
        : base(id)
    {
        Name = !string.IsNullOrWhiteSpace(name)
            ? name.Trim()
            : throw new DomainException("Role name is required.");

        foreach (Permission permission in permissions)
        {
            _permissions.Add(permission);
        }
    }

    public string Name { get; }

    public IReadOnlyCollection<Permission> Permissions => _permissions;

    internal Permission PermissionsValue
    {
        get
        {
            Permission value = Permission.None;
            foreach (Permission permission in _permissions)
            {
                value |= permission;
            }

            return value;
        }
        private set
        {
            _permissions.Clear();
            foreach (Permission permission in Enum.GetValues<Permission>())
            {
                if (permission == Permission.None)
                {
                    continue;
                }

                if (value.HasFlag(permission))
                {
                    _permissions.Add(permission);
                }
            }
        }
    }

    public static Role Create(RoleId id, string name, IEnumerable<Permission> permissions)
    {
        return new Role(id, name, permissions);
    }

    public void SetPermissions(IEnumerable<Permission> newPermissions)
    {
        _permissions.Clear();
        foreach (Permission permission in newPermissions)
        {
            _permissions.Add(permission);
        }
    }
}
