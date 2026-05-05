using AuthService.Application.DTOs;
using MediatR;

namespace AuthService.Application.Commands.Register;

public record RegisterCommand(string Email, string Password, string Name)
    : IRequest<AuthResponse>;