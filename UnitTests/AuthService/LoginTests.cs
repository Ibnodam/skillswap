using AuthService.Application.Commands.Login;
using AuthService.Application.Interfaces;
using AuthService.Domain.Entities;
using AuthService.Domain.Repositories;
using FluentAssertions;
using AuthService.Application.Commands.Register;
using AuthService.Application.Interfaces;
using AuthService.Domain.Entities;
using AuthService.Domain.Repositories;
using FluentAssertions;
using Moq;
using Moq;

namespace SkillSwap.UnitTests.AuthServiceTests;

public class LoginTests
{
    [Fact]
    public async Task Login_ShouldReturnAuthResponse_WhenCredentialsValid()
    {
        // Arrange
        var userRepoMock = new Mock<IUserRepository>();
        var passwordHasherMock = new Mock<IPasswordHasher>();
        var jwtProviderMock = new Mock<IJwtProvider>();
        var refreshTokenRepoMock = new Mock<IRefreshTokenRepository>();

        var user = User.Register("test@mail.com", "TestUser", "hash123");
        userRepoMock.Setup(r => r.GetByEmailAsync("test@mail.com", It.IsAny<CancellationToken>()))
                    .ReturnsAsync(user);
        passwordHasherMock.Setup(p => p.Verify("pass123", "hash123")).Returns(true);
        jwtProviderMock.Setup(j => j.GenerateAccessToken(user)).Returns("jwt_token");
        jwtProviderMock.Setup(j => j.GenerateRefreshToken(user.Id))
                       .Returns(new RefreshToken { Token = "refresh_hash", ExpiresAt = DateTime.UtcNow.AddDays(7) });

        var handler = new LoginHandler(userRepoMock.Object, passwordHasherMock.Object,
                                        jwtProviderMock.Object, refreshTokenRepoMock.Object);

        // Act
        var result = await handler.Handle(new LoginCommand("test@mail.com", "pass123"), CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Email.Should().Be("test@mail.com");
        result.Token.Should().Be("jwt_token");
        result.RefreshToken.Should().Be("refresh_hash");
        refreshTokenRepoMock.Verify(r => r.AddAsync(It.IsAny<RefreshToken>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}