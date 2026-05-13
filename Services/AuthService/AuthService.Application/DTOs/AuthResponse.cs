namespace AuthService.Application.DTOs;

public record AuthResponse(
    Guid Id,
    string Email,
    string Name,
    string Token,                    // Access Token
    string? RefreshToken = null      // ← Новый
);






//namespace AuthService.Application.DTOs;

//public record AuthResponse(
//    Guid Id,
//    string Email,
//    string Name,
//    string Token
//);