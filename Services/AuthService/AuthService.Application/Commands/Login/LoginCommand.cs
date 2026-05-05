using AuthService.Application.DTOs;
using MediatR;

namespace AuthService.Application.Commands.Login;

public record LoginCommand(string Login, string Password)
    : IRequest<AuthResponse>;