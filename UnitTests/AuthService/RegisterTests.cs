using AuthService.Application.Commands.Register;
using AuthService.Application.Interfaces;
using AuthService.Domain.Entities;
using AuthService.Domain.Repositories;
using FluentAssertions;
using Moq;
using System.Timers;

namespace SkillSwap.UnitTests.AuthServiceTests;

public class RegisterTests
{
    [Fact]
    public async Task Register_ShouldReturnAuthResponse_WhenDataIsValid()
    {
        // Arrange
        var userRepoMock = new Mock<IUserRepository>();
        var refreshTokenRepoMock = new Mock<IRefreshTokenRepository>();
        var passwordHasherMock = new Mock<IPasswordHasher>();
        var jwtProviderMock = new Mock<IJwtProvider>();
        var httpClientFactoryMock = new Mock<IHttpClientFactory>();

        userRepoMock.Setup(r => r.ExistsByEmailAsync("test@mail.com", It.IsAny<CancellationToken>()))
                    .ReturnsAsync(false);
        userRepoMock.Setup(r => r.ExistsByNameAsync("TestUser", It.IsAny<CancellationToken>()))
                    .ReturnsAsync(false);
        passwordHasherMock.Setup(p => p.Hash("password123"))
                          .Returns("hashed_password");
        jwtProviderMock.Setup(j => j.GenerateAccessToken(It.IsAny<User>()))
                       .Returns("access_token_xyz");
        jwtProviderMock.Setup(j => j.GenerateRefreshToken(It.IsAny<Guid>()))
                       .Returns(new RefreshToken
                       {
                           Id = Guid.NewGuid(),
                           Token = "hashed_refresh",
                           ExpiresAt = DateTime.UtcNow.AddDays(7)
                       });

        var handler = new RegisterHandler(
            userRepoMock.Object,
            passwordHasherMock.Object,
            jwtProviderMock.Object,
            refreshTokenRepoMock.Object,
            httpClientFactoryMock.Object);

        var command = new RegisterCommand("test@mail.com", "password123", "TestUser");

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Email.Should().Be("test@mail.com");
        result.Name.Should().Be("TestUser");
        result.Token.Should().Be("access_token_xyz");
        result.RefreshToken.Should().NotBeNull();
        userRepoMock.Verify(r => r.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Once);
        refreshTokenRepoMock.Verify(r => r.AddAsync(It.IsAny<RefreshToken>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}








//using System;
//using System.Collections.Generic;
//using System.Text;

//namespace UnitTests.AuthService
//{
//    internal class RegisterTests
//    {
//    }
//}
