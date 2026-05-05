using AuthService.Application.DTOs;
using AuthService.Application.Interfaces;
using AuthService.Domain.Repositories;
using MediatR;

namespace AuthService.Application.Commands.Login;

public class LoginHandler : IRequestHandler<LoginCommand, AuthResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtProvider _jwtProvider;

    public LoginHandler(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        IJwtProvider jwtProvider)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _jwtProvider = jwtProvider;
    }

    public async Task<AuthResponse> Handle(LoginCommand request, CancellationToken ct)
    {
        // Ищем по email или имени
        var user = await _userRepository.GetByEmailAsync(request.Login, ct)
                   ?? await _userRepository.GetByNameAsync(request.Login, ct);

        if (user is null)
            throw new ApplicationException("Неверный логин или пароль");

        if (!_passwordHasher.Verify(request.Password, user.PasswordHash))
            throw new ApplicationException("Неверный логин или пароль");

        var token = _jwtProvider.Generate(user);

        return new AuthResponse(user.Id, user.Email, user.Name, token);
    }
}