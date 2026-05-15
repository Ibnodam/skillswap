using AuthService.Application.Commands.Login;
using AuthService.Application.Commands.RefreshToken;
using AuthService.Application.Commands.Register;
using AuthService.Application.DTOs;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace AuthService.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;

    public AuthController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("register")]
    public async Task<ActionResult> Register([FromBody] RegisterRequest request, CancellationToken ct)
    {
        try
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Email) ||
                string.IsNullOrWhiteSpace(request.Password) || string.IsNullOrWhiteSpace(request.Name))
            {
                return BadRequest(new { message = "Email, пароль и имя обязательны" });
            }

            Console.WriteLine($"[REGISTER] Попытка регистрации: Email={request.Email}, Name={request.Name}");

            var command = new RegisterCommand(request.Email, request.Password, request.Name);
            var result = await _mediator.Send(command, ct);

            return Ok(new
            {
                message = "Регистрация прошла успешно",
                user = result,
                token = result.Token,
                refreshToken = result.RefreshToken
            });
        }
        catch (ApplicationException ex)
        {
            Console.WriteLine($"[REGISTER ERROR] ApplicationException: {ex.Message}");
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[REGISTER ERROR] Unexpected error: {ex.Message}");
            return BadRequest(new { message = "Внутренняя ошибка сервера при регистрации" });
        }
    }










    //[HttpPost("register")]
    //public async Task<ActionResult<AuthResponse>> Register(
    //    [FromBody] RegisterRequest request,
    //    CancellationToken ct)
    //{
    //    try
    //    {
    //        var command = new RegisterCommand(request.Email, request.Password, request.Name);
    //        var result = await _mediator.Send(command, ct);

    //        return Ok(new
    //        {
    //            message = "Регистрация прошла успешно",
    //            user = result,
    //            token = result.Token,
    //            refreshToken = result.RefreshToken
    //        });
    //    }
    //    catch (ApplicationException ex)
    //    {
    //        return BadRequest(new { message = ex.Message });
    //    }
    //}

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login(
        [FromBody] LoginRequest request,
        CancellationToken ct)
    {
        try
        {
            var command = new LoginCommand(request.Login, request.Password);
            var result = await _mediator.Send(command, ct);

            return Ok(new
            {
                message = "Вход выполнен успешно",
                user = result,
                token = result.Token,
                refreshToken = result.RefreshToken
            });
        }
        catch (ApplicationException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
    }

    [HttpPost("refresh")]
    public async Task<ActionResult<AuthResponse>> Refresh(
        [FromBody] RefreshTokenRequest request,
        CancellationToken ct)
    {
        try
        {
            var command = new RefreshTokenCommand(request.RefreshToken);
            var result = await _mediator.Send(command, ct);

            return Ok(new
            {
                message = "Токены успешно обновлены",
                token = result.Token,
                refreshToken = result.RefreshToken
            });
        }
        catch (ApplicationException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
    }
}











//using AuthService.Application.Commands.Login;
//using AuthService.Application.Commands.RefreshToken;
//using AuthService.Application.Commands.Register;
//using AuthService.Application.DTOs;
//using MediatR;
//using Microsoft.AspNetCore.Mvc;

//namespace AuthService.Api.Controllers;

//[ApiController]
//[Route("api/auth")]
//public class AuthController : ControllerBase
//{
//    private readonly IMediator _mediator;

//    public AuthController(IMediator mediator)
//    {
//        _mediator = mediator;
//    }

//    [HttpPost("register")]
//    public async Task<ActionResult<AuthResponse>> Register(
//        [FromBody] RegisterRequest request,
//        CancellationToken ct)
//    {
//        try
//        {
//            var command = new RegisterCommand(request.Email, request.Password, request.Name);
//            var result = await _mediator.Send(command, ct);
//            return Ok(new { message = "Пользователь успешно создан", user = result, token = result.Token });
//        }
//        catch (ApplicationException ex)
//        {
//            return BadRequest(new { message = ex.Message });
//        }
//    }

//    [HttpPost("login")]
//    public async Task<ActionResult<AuthResponse>> Login(
//        [FromBody] LoginRequest request,
//        CancellationToken ct)
//    {
//        try
//        {
//            var command = new LoginCommand(request.Login, request.Password);
//            var result = await _mediator.Send(command, ct);
//            return Ok(new { message = "Вход выполнен успешно", user = result, token = result.Token });
//        }
//        catch (ApplicationException ex)
//        {
//            return Unauthorized(new { message = ex.Message });
//        }
//    }

//    [HttpPost("refresh")]
//    public async Task<ActionResult<AuthResponse>> Refresh(
//    [FromBody] RefreshTokenRequest request,
//    CancellationToken ct)
//    {
//        try
//        {
//            var command = new RefreshTokenCommand(request.RefreshToken);
//            var result = await _mediator.Send(command, ct);
//            return Ok(new { message = "Токены обновлены", token = result.Token, refreshToken = result.RefreshToken });
//        }
//        catch (ApplicationException ex)
//        {
//            return Unauthorized(new { message = ex.Message });
//        }
//    }

//}