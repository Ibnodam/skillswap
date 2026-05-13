using AuthService.Application.DTOs;
using AuthService.Application.Interfaces;
using AuthService.Domain.Repositories;
using MediatR;

namespace AuthService.Application.Commands.RefreshToken;

public class RefreshTokenHandler : IRequestHandler<RefreshTokenCommand, AuthResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IJwtProvider _jwtProvider;

    public RefreshTokenHandler(
        IUserRepository userRepository,
        IRefreshTokenRepository refreshTokenRepository,
        IJwtProvider jwtProvider)
    {
        _userRepository = userRepository;
        _refreshTokenRepository = refreshTokenRepository;
        _jwtProvider = jwtProvider;
    }

    public async Task<AuthResponse> Handle(RefreshTokenCommand request, CancellationToken ct)
    {
        var refreshToken = await _refreshTokenRepository.GetByTokenAsync(request.RefreshToken, ct);

        if (refreshToken == null || !refreshToken.IsActive)
            throw new ApplicationException("Недействительный refresh token");

        var user = await _userRepository.GetByIdAsync(refreshToken.UserId, ct);
        if (user == null)
            throw new ApplicationException("Пользователь не найден");

        // Генерируем новые токены
        var newAccessToken = _jwtProvider.GenerateAccessToken(user);
        var newRefreshToken = _jwtProvider.GenerateRefreshToken(user.Id);

        // Отзываем старый refresh token
        refreshToken.RevokedAt = DateTime.UtcNow;
        refreshToken.ReplacedByToken = newRefreshToken.Token;

        await _refreshTokenRepository.AddAsync(newRefreshToken, ct);
        await _refreshTokenRepository.UpdateAsync(refreshToken, ct);

        return new AuthResponse(user.Id, user.Email, user.Name, newAccessToken);
    }
}