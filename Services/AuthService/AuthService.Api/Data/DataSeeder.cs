using System.Net.Http.Json;
using AuthService.Domain.Entities;
using AuthService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Api.Data;

public static class DataSeeder
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AuthDbContext>();
        var httpClientFactory = scope.ServiceProvider.GetRequiredService<IHttpClientFactory>();

        if (await db.Users.AnyAsync())
            return;

        var passwordHash = BCrypt.Net.BCrypt.HashPassword("123456");

        var users = new[]
        {
            new { Id = Guid.Parse("a1000000-0000-0000-0000-000000000001"), Email = "anna@mail.com", Name = "Анна Кузнецова" },
            new { Id = Guid.Parse("a1000000-0000-0000-0000-000000000002"), Email = "dmitry@mail.com", Name = "Дмитрий Волков" },
            new { Id = Guid.Parse("a1000000-0000-0000-0000-000000000003"), Email = "elena@mail.com", Name = "Елена Морозова" },
            new { Id = Guid.Parse("a1000000-0000-0000-0000-000000000004"), Email = "mikhail@mail.com", Name = "Михаил Иванов" },
            new { Id = Guid.Parse("a1000000-0000-0000-0000-000000000005"), Email = "olga@mail.com", Name = "Ольга Петрова" },
            new { Id = Guid.Parse("a1000000-0000-0000-0000-000000000006"), Email = "sergey@mail.com", Name = "Сергей Соколов" },
            new { Id = Guid.Parse("a1000000-0000-0000-0000-000000000007"), Email = "maria@mail.com", Name = "Мария Белова" },
            new { Id = Guid.Parse("a1000000-0000-0000-0000-000000000008"), Email = "alexey@mail.com", Name = "Алексей Громов" },
            new { Id = Guid.Parse("a1000000-0000-0000-0000-000000000009"), Email = "tatiana@mail.com", Name = "Татьяна Волкова" },
            new { Id = Guid.Parse("a1000000-0000-0000-0000-000000000010"), Email = "igor@mail.com", Name = "Игорь Никитин" }
        };

        var client = httpClientFactory.CreateClient("UserService");

        foreach (var u in users)
        {

            db.Users.Add(new User
            {
                Id = u.Id,
                Email = u.Email,
                Name = u.Name,
                PasswordHash = passwordHash,
                CreatedAt = DateTime.UtcNow
            });

            try
            {
                await client.PostAsJsonAsync("/api/users/internal", new
                {
                    u.Id,
                    u.Email,
                    u.Name
                });
            }
            catch { }
        }

        await db.SaveChangesAsync();
    }
}