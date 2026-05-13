using System.Net.Http.Json;
using AuthService.Application.DTOs;
using AuthService.Application.Interfaces;
using AuthService.Domain.Entities;
using AuthService.Domain.Repositories;
using MediatR;

namespace AuthService.Application.Commands.Register;

public class RegisterHandler : IRequestHandler<RegisterCommand, AuthResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtProvider _jwtProvider;
    private readonly IRefreshTokenRepository _refreshTokenRepository;   // ← Новый
    private readonly IHttpClientFactory _httpClientFactory;

    public RegisterHandler(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        IJwtProvider jwtProvider,
        IRefreshTokenRepository refreshTokenRepository,   // ← Добавили
        IHttpClientFactory httpClientFactory)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _jwtProvider = jwtProvider;
        _refreshTokenRepository = refreshTokenRepository;
        _httpClientFactory = httpClientFactory;
    }

    public async Task<AuthResponse> Handle(RegisterCommand request, CancellationToken ct)
    {
        if (await _userRepository.ExistsByEmailAsync(request.Email, ct))
            throw new ApplicationException("Пользователь с таким email уже существует");

        if (await _userRepository.ExistsByNameAsync(request.Name, ct))
            throw new ApplicationException("Пользователь с таким именем уже существует");

        var passwordHash = _passwordHasher.Hash(request.Password);
        var user = User.Register(request.Email, request.Name, passwordHash);

        await _userRepository.AddAsync(user, ct);

        // === Генерация токенов ===
        var accessToken = _jwtProvider.GenerateAccessToken(user);
        var refreshTokenEntity = _jwtProvider.GenerateRefreshToken(user.Id);

        // Сохраняем Refresh Token
        await _refreshTokenRepository.AddAsync(refreshTokenEntity, ct);

        // Синхронизация с UserService
        try
        {
            var client = _httpClientFactory.CreateClient("UserService");
            await client.PostAsJsonAsync("/api/users/internal", new
            {
                user.Id,
                user.Email,
                user.Name
            }, ct);
        }
        catch (Exception)
        {
            // Не критично
        }

        return new AuthResponse(
            user.Id,
            user.Email,
            user.Name,
            accessToken,
            refreshTokenEntity.Token   // ← Возвращаем Refresh Token
        );
    }
}







//using System.Net.Http.Json;
//using AuthService.Application.DTOs;
//using AuthService.Application.Interfaces;
//using AuthService.Domain.Entities;
//using AuthService.Domain.Repositories;
//using MediatR;

//namespace AuthService.Application.Commands.Register;

//public class RegisterHandler : IRequestHandler<RegisterCommand, AuthResponse>
//{
//    private readonly IUserRepository _userRepository;
//    private readonly IPasswordHasher _passwordHasher;
//    private readonly IJwtProvider _jwtProvider;
//    private readonly IHttpClientFactory _httpClientFactory;

//    public RegisterHandler(
//        IUserRepository userRepository,
//        IPasswordHasher passwordHasher,
//        IJwtProvider jwtProvider,
//        IHttpClientFactory httpClientFactory)
//    {
//        _userRepository = userRepository;
//        _passwordHasher = passwordHasher;
//        _jwtProvider = jwtProvider;
//        _httpClientFactory = httpClientFactory;
//    }

//    public async Task<AuthResponse> Handle(RegisterCommand request, CancellationToken ct)
//    {
//        if (await _userRepository.ExistsByEmailAsync(request.Email, ct))
//            throw new ApplicationException("Пользователь с таким email уже существует");

//        if (await _userRepository.ExistsByNameAsync(request.Name, ct))
//            throw new ApplicationException("Пользователь с таким именем уже существует");

//        var passwordHash = _passwordHasher.Hash(request.Password);
//        var user = User.Register(request.Email, request.Name, passwordHash);

//        await _userRepository.AddAsync(user, ct);

//        var token = _jwtProvider.Generate(user);

//        // Синхронизация с UserService
//        try
//        {
//            var client = _httpClientFactory.CreateClient("UserService");
//            await client.PostAsJsonAsync("/api/users/internal", new
//            {
//                user.Id,
//                user.Email,
//                user.Name
//            }, ct);
//        }
//        catch (Exception)
//        {
//            // Если UserService недоступен — пользователь всё равно создан в AuthService
//        }

//        return new AuthResponse(user.Id, user.Email, user.Name, token);
//    }
//}






//using AuthService.Application.DTOs;
//using AuthService.Application.Interfaces;
//using AuthService.Domain.Entities;
//using AuthService.Domain.Repositories;
//using MediatR;

//namespace AuthService.Application.Commands.Register;

//public class RegisterHandler : IRequestHandler<RegisterCommand, AuthResponse>
//{
//    private readonly IUserRepository _userRepository;
//    private readonly IPasswordHasher _passwordHasher;
//    private readonly IJwtProvider _jwtProvider;

//    public RegisterHandler(
//        IUserRepository userRepository,
//        IPasswordHasher passwordHasher,
//        IJwtProvider jwtProvider)
//    {
//        _userRepository = userRepository;
//        _passwordHasher = passwordHasher;
//        _jwtProvider = jwtProvider;
//    }

//    public async Task<AuthResponse> Handle(RegisterCommand request, CancellationToken ct)
//    {
//        // Проверка уникальности
//        if (await _userRepository.ExistsByEmailAsync(request.Email, ct))
//            throw new ApplicationException("Пользователь с таким email уже существует");

//        if (await _userRepository.ExistsByNameAsync(request.Name, ct))
//            throw new ApplicationException("Пользователь с таким именем уже существует");

//        // Создание пользователя
//        var passwordHash = _passwordHasher.Hash(request.Password);
//        var user = User.Register(request.Email, request.Name, passwordHash);

//        await _userRepository.AddAsync(user, ct);

//        // Генерация токена
//        var token = _jwtProvider.Generate(user);

//        return new AuthResponse(user.Id, user.Email, user.Name, token);
//    }
//}