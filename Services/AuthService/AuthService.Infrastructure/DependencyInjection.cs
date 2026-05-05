using AuthService.Application.Interfaces;
using AuthService.Domain.Repositories;
using AuthService.Infrastructure.Data;
using AuthService.Infrastructure.Repositories;
using AuthService.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AuthService.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<AuthDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("AuthDb")));

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<IJwtProvider, JwtProvider>();

        services.AddHttpClient("UserService", client =>
        {
            client.BaseAddress = new Uri(configuration["Services:UserService"]
                ?? "http://localhost:5002/");
        });

        return services;
    }
}






//using AuthService.Application.Interfaces;
//using AuthService.Domain.Repositories;
//using AuthService.Infrastructure.Data;
//using AuthService.Infrastructure.Repositories;
//using AuthService.Infrastructure.Services;
//using Microsoft.EntityFrameworkCore;
//using Microsoft.Extensions.Configuration;
//using Microsoft.Extensions.DependencyInjection;

//namespace AuthService.Infrastructure;

//public static class DependencyInjection
//{
//    public static IServiceCollection AddInfrastructure(

//        this IServiceCollection services,
//        IConfiguration configuration)
//    {
//        services.AddDbContext<AuthDbContext>(options =>
//            options.UseNpgsql(configuration.GetConnectionString("AuthDb")));

//        services.AddScoped<IUserRepository, UserRepository>();
//        services.AddScoped<IPasswordHasher, PasswordHasher>();
//        services.AddScoped<IJwtProvider, JwtProvider>();

//        return services;
//    }
//}