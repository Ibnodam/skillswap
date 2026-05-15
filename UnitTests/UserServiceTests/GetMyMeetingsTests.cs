using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UserService.Api.Controllers;
using UserService.Api.Data;
using UserService.Api.Models;

namespace SkillSwap.UnitTests.UserServiceTests;

public class GetAllUsersTests
{
    [Fact]
    public async Task GetAll_ShouldReturnPaginatedUsers()
    {
        var options = new DbContextOptionsBuilder<UserDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()).Options;

        using (var context = new UserDbContext(options))
        {
            for (int i = 1; i <= 15; i++)
                context.Users.Add(new User { Id = Guid.NewGuid(), Email = $"user{i}@mail.com", Name = $"User{i}" });
            await context.SaveChangesAsync();
        }

        using (var context = new UserDbContext(options))
        {
            var controller = new UsersController(context);
            var actionResult = await controller.GetAll(page: 2, limit: 5);

            var okResult = actionResult as OkObjectResult;
            okResult.Should().NotBeNull();
            var response = okResult!.Value;
            var users = response!.GetType().GetProperty("users")!.GetValue(response) as IEnumerable<object>;
            var pagination = response.GetType().GetProperty("pagination")!.GetValue(response);

            users.Should().HaveCount(5);
            pagination!.GetType().GetProperty("page")!.GetValue(pagination).Should().Be(2);
            pagination.GetType().GetProperty("limit")!.GetValue(pagination).Should().Be(5);
            pagination.GetType().GetProperty("total")!.GetValue(pagination).Should().Be(15);
            pagination.GetType().GetProperty("pages")!.GetValue(pagination).Should().Be(3);
        }
    }
}










//using AuthService.Domain.Entities;
//using FluentAssertions;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.EntityFrameworkCore;
//using UserService.Api.Controllers;
//using UserService.Api.Data;
//using UserService.Api.Models;
//using AuthService.Application.Commands.Register;
//using AuthService.Application.Interfaces;
//using AuthService.Domain.Repositories;
//using Microsoft.AspNetCore.Mvc;
//using Moq;
//using User = UserService.Api.Models.User;

//namespace SkillSwap.UnitTests.UserServiceTests;

//public class GetAllUsersTests
//{
//    [Fact]
//    public async Task GetAll_ShouldReturnPaginatedUsers()
//    {
//        // Arrange
//        var options = new DbContextOptionsBuilder<UserDbContext>()
//            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
//            .Options;

//        using (var context = new UserDbContext(options))
//        {
//            for (int i = 1; i <= 15; i++)
//                context.Users.Add(new User
//                {
//                    Id = Guid.NewGuid(),
//                    Email = $"user{i}@mail.com",
//                    Name = $"User{i}"
//                });
//            await context.SaveChangesAsync();
//        }

//        using (var context = new UserDbContext(options))
//        {
//            var controller = new UsersController(context);

//            // Act
//            var result = await controller.GetAll(page: 2, limit: 5);

//            // Assert
//            var okResult = result.Result as OkObjectResult;
//            okResult.Should().NotBeNull();
//            var response = okResult!.Value;
//            var users = response!.GetType().GetProperty("users")!.GetValue(response) as IEnumerable<object>;
//            var pagination = response.GetType().GetProperty("pagination")!.GetValue(response);

//            users.Should().HaveCount(5);
//            pagination!.GetType().GetProperty("page")!.GetValue(pagination).Should().Be(2);
//            pagination.GetType().GetProperty("limit")!.GetValue(pagination).Should().Be(5);
//            pagination.GetType().GetProperty("total")!.GetValue(pagination).Should().Be(15);
//            pagination.GetType().GetProperty("pages")!.GetValue(pagination).Should().Be(3);
//        }
//    }
//}