using StayOps.Application.Abstractions;
using StayOps.Application.Access;
using StayOps.Application.Users.Abstractions;
using StayOps.Application.Users.Commands;
using StayOps.Application.Users.Contracts;
using StayOps.Domain.Users;

namespace StayOps.Application.Users.Services;

public sealed class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IRoleRepository _roleRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IDateTimeProvider _clock;

    public UserService(
        IUserRepository userRepository,
        IRoleRepository roleRepository,
        IPasswordHasher passwordHasher,
        IDateTimeProvider clock)
    {
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _roleRepository = roleRepository ?? throw new ArgumentNullException(nameof(roleRepository));
        _passwordHasher = passwordHasher ?? throw new ArgumentNullException(nameof(passwordHasher));
        _clock = clock ?? throw new ArgumentNullException(nameof(clock));
    }

    public async Task<Guid> CreateUserAsync(CreateUserCommand command, CancellationToken cancellationToken)
    {
        Email email = Email.Create(command.Email);

        bool emailExists = await _userRepository.EmailExistsAsync(email, cancellationToken);
        if (emailExists)
        {
            throw new AccessManagementException("Email already exists.");
        }

        string roleName = string.IsNullOrWhiteSpace(command.RoleName)
            ? UserRoleDefaults.StaffRoleName
            : command.RoleName!.Trim();

        Role? role = await _roleRepository.GetByNameAsync(roleName, cancellationToken);
        if (role is null)
        {
            throw new AccessManagementException($"Role '{roleName}' does not exist.");
        }

        string passwordHash = _passwordHasher.Hash(command.Password);
        User user = User.Create(
            new UserId(Guid.NewGuid()),
            email,
            passwordHash,
            command.Name,
            role.Id,
            _clock.UtcNow,
            new UserId(command.ActorUserId));

        await _userRepository.AddAsync(user, cancellationToken);
        return user.Id.Value;
    }

    public async Task ChangeUserStatusAsync(ChangeUserStatusCommand command, CancellationToken cancellationToken)
    {
        User user = await GetUser(command.UserId, cancellationToken);

        if (command.IsActive)
        {
            user.Activate(new UserId(command.ActorUserId), _clock.UtcNow);
            await _userRepository.UpdateAsync(user, cancellationToken);
            return;
        }

        Role currentRole = await _roleRepository.GetByIdAsync(user.RoleId, cancellationToken)
            ?? throw new AccessManagementException("User role does not exist.");

        if (IsAdminRole(currentRole) && !await _userRepository.HasOtherActiveAdminsAsync(user.Id, cancellationToken))
        {
            throw new AccessManagementException("Cannot deactivate the last active admin.");
        }

        user.Deactivate(new UserId(command.ActorUserId), _clock.UtcNow);
        await _userRepository.UpdateAsync(user, cancellationToken);
    }

    public async Task ChangeUserRoleAsync(ChangeUserRoleCommand command, CancellationToken cancellationToken)
    {
        User user = await GetUser(command.UserId, cancellationToken);
        Role currentRole = await _roleRepository.GetByIdAsync(user.RoleId, cancellationToken)
            ?? throw new AccessManagementException("User role does not exist.");

        string roleName = string.IsNullOrWhiteSpace(command.RoleName)
            ? throw new AccessManagementException("Role name is required.")
            : command.RoleName.Trim();

        Role? newRole = await _roleRepository.GetByNameAsync(roleName, cancellationToken);
        if (newRole is null)
        {
            throw new AccessManagementException($"Role '{roleName}' does not exist.");
        }

        bool removingAdmin = IsAdminRole(currentRole) && !IsAdminRole(newRole);
        if (removingAdmin && !await _userRepository.HasOtherActiveAdminsAsync(user.Id, cancellationToken))
        {
            throw new AccessManagementException("Cannot remove the last active admin.");
        }

        user.ChangeRole(newRole.Id, new UserId(command.ActorUserId), _clock.UtcNow);
        await _userRepository.UpdateAsync(user, cancellationToken);
    }

    public async Task ChangePasswordAsync(UpdatePasswordCommand command, CancellationToken cancellationToken)
    {
        User user = await GetUser(command.UserId, cancellationToken);
        if (!_passwordHasher.Verify(user.PasswordHash, command.CurrentPassword))
        {
            throw new AccessManagementException("Current password is invalid.");
        }

        string passwordHash = _passwordHasher.Hash(command.NewPassword);
        user.ChangePasswordHash(passwordHash, user.Id, _clock.UtcNow);
        await _userRepository.UpdateAsync(user, cancellationToken);
    }

    public async Task<UserResult> GetUserAsync(Guid userId, CancellationToken cancellationToken)
    {
        User user = await GetUser(userId, cancellationToken);
        Role role = await _roleRepository.GetByIdAsync(user.RoleId, cancellationToken)
            ?? throw new AccessManagementException("User role does not exist.");

        return CreateUserResult(user, role);
    }

    public async Task<IReadOnlyCollection<UserResult>> GetUsersAsync(CancellationToken cancellationToken)
    {
        IReadOnlyCollection<User> users = await _userRepository.ListAsync(cancellationToken);
        var results = new List<UserResult>(users.Count);

        foreach (User user in users)
        {
            Role role = await _roleRepository.GetByIdAsync(user.RoleId, cancellationToken)
                ?? throw new AccessManagementException("User role does not exist.");

            results.Add(CreateUserResult(user, role));
        }

        return results;
    }

    private static bool IsAdminRole(Role role)
    {
        return string.Equals(role.Name, UserRoleDefaults.AdminRoleName, StringComparison.OrdinalIgnoreCase);
    }

    private async Task<User> GetUser(Guid userId, CancellationToken cancellationToken)
    {
        User? user = await _userRepository.GetByIdAsync(new UserId(userId), cancellationToken);
        if (user is null)
        {
            throw new AccessManagementException("User not found.");
        }

        return user;
    }

    private static UserResult CreateUserResult(User user, Role role)
    {
        return new UserResult(
            user.Id.Value,
            user.Email.Value,
            user.Name,
            role.Name,
            user.Status,
            user.CreatedAt,
            user.UpdatedAt,
            role.Permissions.ToArray());
    }
}
