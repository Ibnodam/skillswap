using AuthService.Application.DTOs;
using MediatR;

namespace AuthService.Application.Commands.RefreshToken;

public record RefreshTokenCommand(string RefreshToken)
    : IRequest<AuthResponse>;