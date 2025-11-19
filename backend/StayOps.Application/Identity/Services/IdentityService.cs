using StayOps.Application.Abstractions;
using StayOps.Application.Identity.Abstractions;
using StayOps.Application.Identity.Commands;
using StayOps.Application.Identity.Contracts;
using StayOps.Application.Identity;
using StayOps.Domain.Identity;

namespace StayOps.Application.Identity.Services;

public sealed class IdentityService : IIdentityService
{
    private readonly IUserRepository _userRepository;
    private readonly IRoleRepository _roleRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITokenService _tokenService;
    private readonly IDateTimeProvider _clock;

    public IdentityService(
        IUserRepository userRepository,
        IRoleRepository roleRepository,
        IPasswordHasher passwordHasher,
        ITokenService tokenService,
        IDateTimeProvider clock)
    {
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _roleRepository = roleRepository ?? throw new ArgumentNullException(nameof(roleRepository));
        _passwordHasher = passwordHasher ?? throw new ArgumentNullException(nameof(passwordHasher));
        _tokenService = tokenService ?? throw new ArgumentNullException(nameof(tokenService));
        _clock = clock ?? throw new ArgumentNullException(nameof(clock));
    }

    public async Task<Guid> CreateUserAsync(CreateUserCommand command, CancellationToken cancellationToken)
    {
        Email email = Email.Create(command.Email);

        bool emailExists = await _userRepository.EmailExistsAsync(email, cancellationToken);
        if (emailExists)
        {
            throw new IdentityApplicationException("Email already exists.");
        }

        RoleId roleId = new RoleId(command.RoleId);
        Role? role = await _roleRepository.GetByIdAsync(roleId, cancellationToken);
        if (role is null)
        {
            throw new IdentityApplicationException("Role does not exist.");
        }

        string passwordHash = _passwordHasher.Hash(command.Password);
        User user = User.Create(
            new UserId(Guid.NewGuid()),
            email,
            passwordHash,
            command.Name,
            roleId,
            _clock.UtcNow,
            new UserId(command.ActorUserId));

        await _userRepository.AddAsync(user, cancellationToken);
        return user.Id.Value;
    }

    public async Task ActivateUserAsync(ActivateUserCommand command, CancellationToken cancellationToken)
    {
        User user = await GetUser(command.UserId, cancellationToken);
        user.Activate(new UserId(command.ActorUserId), _clock.UtcNow);
        await _userRepository.UpdateAsync(user, cancellationToken);
    }

    public async Task DeactivateUserAsync(DeactivateUserCommand command, CancellationToken cancellationToken)
    {
        User user = await GetUser(command.UserId, cancellationToken);
        Role? currentRole = await _roleRepository.GetByIdAsync(user.RoleId, cancellationToken)
            ?? throw new IdentityApplicationException("User role does not exist.");

        if (IsAdminRole(currentRole) && !await _userRepository.HasOtherActiveAdminsAsync(user.Id, cancellationToken))
        {
            throw new IdentityApplicationException("Cannot deactivate the last active admin.");
        }

        user.Deactivate(new UserId(command.ActorUserId), _clock.UtcNow);
        await _userRepository.UpdateAsync(user, cancellationToken);
    }

    public async Task ChangeUserRoleAsync(ChangeUserRoleCommand command, CancellationToken cancellationToken)
    {
        User user = await GetUser(command.UserId, cancellationToken);
        Role? currentRole = await _roleRepository.GetByIdAsync(user.RoleId, cancellationToken)
            ?? throw new IdentityApplicationException("User role does not exist.");

        RoleId newRoleId = new RoleId(command.RoleId);
        Role? newRole = await _roleRepository.GetByIdAsync(newRoleId, cancellationToken)
            ?? throw new IdentityApplicationException("Role does not exist.");

        bool removingAdmin = IsAdminRole(currentRole) && !IsAdminRole(newRole);
        if (removingAdmin && !await _userRepository.HasOtherActiveAdminsAsync(user.Id, cancellationToken))
        {
            throw new IdentityApplicationException("Cannot remove the last active admin.");
        }

        user.ChangeRole(newRoleId, new UserId(command.ActorUserId), _clock.UtcNow);
        await _userRepository.UpdateAsync(user, cancellationToken);
    }

    public async Task ChangePasswordAsync(ChangePasswordCommand command, CancellationToken cancellationToken)
    {
        User user = await GetUser(command.UserId, cancellationToken);
        string passwordHash = _passwordHasher.Hash(command.NewPassword);
        user.ChangePasswordHash(passwordHash, new UserId(command.ActorUserId), _clock.UtcNow);
        await _userRepository.UpdateAsync(user, cancellationToken);
    }

    public async Task<LoginResult> LoginAsync(LoginCommand command, CancellationToken cancellationToken)
    {
        Email email = Email.Create(command.Email);
        User? user = await _userRepository.GetByEmailAsync(email, cancellationToken);
        if (user is null)
        {
            throw new IdentityApplicationException("Invalid credentials.");
        }

        if (user.Status != UserStatus.Active)
        {
            throw new IdentityApplicationException("User is inactive.");
        }

        if (!_passwordHasher.Verify(user.PasswordHash, command.Password))
        {
            throw new IdentityApplicationException("Invalid credentials.");
        }

        Role? role = await _roleRepository.GetByIdAsync(user.RoleId, cancellationToken);
        if (role is null)
        {
            throw new IdentityApplicationException("User role does not exist.");
        }

        string token = _tokenService.GenerateToken(user, role);
        return new LoginResult(
            token,
            user.Id.Value,
            user.Name,
            role.Name,
            role.Permissions.ToArray());
    }

    private static bool IsAdminRole(Role role)
    {
        return string.Equals(role.Name, IdentityDefaults.AdminRoleName, StringComparison.OrdinalIgnoreCase);
    }

    private async Task<User> GetUser(Guid userId, CancellationToken cancellationToken)
    {
        User? user = await _userRepository.GetByIdAsync(new UserId(userId), cancellationToken);
        if (user is null)
        {
            throw new IdentityApplicationException("User not found.");
        }

        return user;
    }
}
