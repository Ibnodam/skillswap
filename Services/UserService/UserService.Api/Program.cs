using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using UserService.Api.Data;

var builder = WebApplication.CreateBuilder(args);

// ==================== CONTROLLERS + SWAGGER ====================
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpClient();
// ==================== JWT AUTH ====================
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Secret"]!))
        };
    });

builder.Services.AddAuthorization();

// ==================== DATABASE ====================
builder.Services.AddDbContext<UserDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("UserDb")));

var app = builder.Build();

// ==================== MIDDLEWARE ====================
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// ==================== DB INIT + SEED ====================
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<UserDbContext>();

    // ❗ правильно: миграции, а не EnsureCreated
    db.Database.Migrate();

    // если у тебя есть Seeder
    await SeedData.Initialize(db);
}

app.Run();

/////////////////////////////////////////

//////////////////////////
// работает нормально, но надо датасидер
//using Microsoft.AspNetCore.Authentication.JwtBearer;
//using Microsoft.EntityFrameworkCore;
//using Microsoft.IdentityModel.Tokens;
//using System.Text;
//using UserService.Api.Data;
//using UserService.Api.Middleware;

//var builder = WebApplication.CreateBuilder(args);

//builder.Services.AddControllers();
//builder.Services.AddEndpointsApiExplorer();
//builder.Services.AddSwaggerGen();

//// ==================== JWT АУТЕНТИФИКАЦИЯ ====================
//builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
//    .AddJwtBearer(options =>
//    {
//        options.TokenValidationParameters = new TokenValidationParameters
//        {
//            ValidateIssuer = false,
//            ValidateAudience = false,
//            ValidateLifetime = true,
//            ValidateIssuerSigningKey = true,
//            IssuerSigningKey = new SymmetricSecurityKey(
//                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Secret"]))
//        };

//        options.Events = new JwtBearerEvents
//        {
//            OnMessageReceived = context =>
//            {
//                var token = context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
//                if (!string.IsNullOrEmpty(token))
//                    context.Token = token;
//                return Task.CompletedTask;
//            }
//        };
//    });

//builder.Services.AddAuthorization();

//// ==================== DATABASE ====================
//builder.Services.AddDbContext<UserDbContext>(options =>
//    options.UseNpgsql(builder.Configuration.GetConnectionString("UserDb")));

//var app = builder.Build();

//if (app.Environment.IsDevelopment())
//{
//    app.UseSwagger();
//    app.UseSwaggerUI();
//}

//app.UseHttpsRedirection();

//app.UseAuthentication();   // ← обязательно
//app.UseAuthorization();    // ← обязательно

////app.UseMiddleware<JwtMiddleware>();   // если он у тебя ещё нужен

//app.MapControllers();

//using (var scope = app.Services.CreateScope())
//{
//    var db = scope.ServiceProvider.GetRequiredService<UserDbContext>();
//    db.Database.EnsureCreated();
//}

//app.Run();












//using Microsoft.EntityFrameworkCore;
//using Microsoft.AspNetCore.Authentication.JwtBearer;
//using Microsoft.IdentityModel.Tokens;
//using System.Text;
//using UserService.Api.Data;
//using UserService.Api.Middleware;

//var builder = WebApplication.CreateBuilder(args);

//builder.Services.AddControllers();
//builder.Services.AddEndpointsApiExplorer();
//builder.Services.AddSwaggerGen();

//// === JWT Аутентификация ===
//builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
//    .AddJwtBearer(options =>
//    {
//        options.TokenValidationParameters = new TokenValidationParameters
//        {
//            ValidateIssuer = false,
//            ValidateAudience = false,
//            ValidateLifetime = true,
//            ValidateIssuerSigningKey = true,
//            IssuerSigningKey = new SymmetricSecurityKey(
//                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Secret"]))
//        };

//        options.Events = new JwtBearerEvents
//        {
//            OnMessageReceived = context =>
//            {
//                var accessToken = context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
//                if (!string.IsNullOrEmpty(accessToken))
//                {
//                    context.Token = accessToken;
//                }
//                return Task.CompletedTask;
//            }
//        };
//    });

//builder.Services.AddAuthorization();

//builder.Services.AddDbContext<UserDbContext>(options =>
//    options.UseNpgsql(builder.Configuration.GetConnectionString("UserDb")));

//var app = builder.Build();

//if (app.Environment.IsDevelopment())
//{
//    app.UseSwagger();
//    app.UseSwaggerUI();
//}

//// Middleware
//app.UseHttpsRedirection();
//app.UseAuthentication();     // ← важно
//app.UseAuthorization();      // ← важно
//app.UseMiddleware<JwtMiddleware>();

//app.MapControllers();

//using (var scope = app.Services.CreateScope())
//{
//    var db = scope.ServiceProvider.GetRequiredService<UserDbContext>();
//    db.Database.EnsureCreated();
//}

//app.Run();










////using Microsoft.EntityFrameworkCore;
////using UserService.Api.Data;
////using UserService.Api.Middleware;

////var builder = WebApplication.CreateBuilder(args);

////builder.Services.AddControllers();
////builder.Services.AddEndpointsApiExplorer();
////builder.Services.AddSwaggerGen();

////builder.Services.AddDbContext<UserDbContext>(options =>
////    options.UseNpgsql(builder.Configuration.GetConnectionString("UserDb")));

////var app = builder.Build();

////if (app.Environment.IsDevelopment())
////{
////    app.UseSwagger();
////    app.UseSwaggerUI();
////}

////app.UseMiddleware<JwtMiddleware>();
////app.MapControllers();

////using (var scope = app.Services.CreateScope())
////{
////    var db = scope.ServiceProvider.GetRequiredService<UserDbContext>();
////    db.Database.EnsureCreated();
////}

////app.Run();





////var builder = WebApplication.CreateBuilder(args);

////// Add services to the container.
////// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
////builder.Services.AddOpenApi();

////var app = builder.Build();

////// Configure the HTTP request pipeline.
////if (app.Environment.IsDevelopment())
////{
////    app.MapOpenApi();
////}

////app.UseHttpsRedirection();

////var summaries = new[]
////{
////    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
////};

////app.MapGet("/weatherforecast", () =>
////{
////    var forecast =  Enumerable.Range(1, 5).Select(index =>
////        new WeatherForecast
////        (
////            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
////            Random.Shared.Next(-20, 55),
////            summaries[Random.Shared.Next(summaries.Length)]
////        ))
////        .ToArray();
////    return forecast;
////})
////.WithName("GetWeatherForecast");

////app.Run();

////record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
////{
////    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
////}
