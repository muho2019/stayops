using System.Linq;
using StayOps.Application.Access;
using StayOps.Application.Auth.Abstractions;
using StayOps.Application.Auth.Commands;
using StayOps.Application.Auth.Contracts;
using StayOps.Application.Users.Abstractions;
using StayOps.Domain.Users;

namespace StayOps.Application.Auth.Services;

public sealed class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IRoleRepository _roleRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITokenService _tokenService;

    public AuthService(
        IUserRepository userRepository,
        IRoleRepository roleRepository,
        IPasswordHasher passwordHasher,
        ITokenService tokenService)
    {
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _roleRepository = roleRepository ?? throw new ArgumentNullException(nameof(roleRepository));
        _passwordHasher = passwordHasher ?? throw new ArgumentNullException(nameof(passwordHasher));
        _tokenService = tokenService ?? throw new ArgumentNullException(nameof(tokenService));
    }

    public async Task<LoginResult> LoginAsync(LoginCommand command, CancellationToken cancellationToken)
    {
        Email email = Email.Create(command.Email);
        User? user = await _userRepository.GetByEmailAsync(email, cancellationToken);
        if (user is null)
        {
            throw new AccessManagementException("Invalid credentials.");
        }

        if (user.Status != UserStatus.Active)
        {
            throw new AccessManagementException("User is inactive.");
        }

        if (!_passwordHasher.Verify(user.PasswordHash, command.Password))
        {
            throw new AccessManagementException("Invalid credentials.");
        }

        Role? role = await _roleRepository.GetByIdAsync(user.RoleId, cancellationToken);
        if (role is null)
        {
            throw new AccessManagementException("User role does not exist.");
        }

        string token = _tokenService.GenerateToken(user, role);
        return new LoginResult(
            token,
            user.Id.Value,
            user.Name,
            role.Name,
            role.Permissions.ToArray());
    }
}
