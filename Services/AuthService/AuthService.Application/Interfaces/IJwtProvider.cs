using AuthService.Domain.Entities;

namespace AuthService.Application.Interfaces;

public interface IJwtProvider
{
    string GenerateAccessToken(User user);
    RefreshToken GenerateRefreshToken(Guid userId);

    // Новый метод
    Guid? ValidateAccessToken(string token);   // возвращает UserId или null
}







//using AuthService.Domain.Entities;

//namespace AuthService.Application.Interfaces;

//public interface IJwtProvider
//{
//    string Generate(User user);
//}