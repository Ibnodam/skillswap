using MessagingService.Api.Data;
using MessagingService.Api.Hubs;
using MessagingService.Api.Middleware;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// === База данных ===
builder.Services.AddDbContext<ChatDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("ChatDb")));

// === SignalR + Redis ===
builder.Services.AddSignalR()
       .AddStackExchangeRedis("localhost:6379", options =>
       {
           options.Configuration.ChannelPrefix = "SkillSwap.Chat";
       });

// === CORS ===
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("http://localhost:5173", "http://localhost:5000")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// === JWT Authentication (важно для SignalR!) ===
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Secret"]!)),
            ValidateIssuer = false,
            ValidateAudience = false,
            ClockSkew = TimeSpan.Zero
        };

        // Поддержка токена через Query String для SignalR
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];
                if (!string.IsNullOrEmpty(accessToken))
                {
                    context.Token = accessToken;
                }
                return Task.CompletedTask;
            }
        };
    });

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors();
app.UseAuthentication();           // ← Важно!
app.UseMiddleware<JwtMiddleware>(); // можно оставить как fallback
app.UseAuthorization();

app.MapControllers();
app.MapHub<ChatHub>("/hubs/chat");

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ChatDbContext>();
    db.Database.EnsureCreated();
}

app.Run();









//using Microsoft.EntityFrameworkCore;
//using MessagingService.Api.Data;
//using MessagingService.Api.Hubs;
//using MessagingService.Api.Middleware;

//var builder = WebApplication.CreateBuilder(args);

//builder.Services.AddControllers();
//builder.Services.AddEndpointsApiExplorer();
//builder.Services.AddSwaggerGen();

//builder.Services.AddDbContext<ChatDbContext>(options =>
//    options.UseNpgsql(builder.Configuration.GetConnectionString("ChatDb")));

//builder.Services.AddSignalR().AddStackExchangeRedis("localhost:6379");

//// Messaging Service - Program.cs
////builder.Services.AddCors(options =>
////{
////    options.AddDefaultPolicy(policy =>
////    {
////        policy.WithOrigins(
////                "http://localhost:5173",   // фронтенд напрямую
////                "http://localhost:5000"    // ← шлюз!
////            )
////            .AllowAnyHeader()
////            .AllowAnyMethod()
////            .AllowCredentials();
////    });
////});

////builder.Services.AddCors(options =>
////{
////    options.AddDefaultPolicy(policy =>
////    {
////        policy.WithOrigins("http://localhost:5173")
////              .AllowAnyHeader()
////              .AllowAnyMethod()
////              .AllowCredentials();
////    });
////});


//builder.Services.AddCors(options =>
//{
//    options.AddDefaultPolicy(policy =>
//    {
//        policy.WithOrigins("http://localhost:5173", "http://localhost:5000")
//              .AllowAnyHeader()
//              .AllowAnyMethod()
//              .AllowCredentials();
//    });
//});


//var app = builder.Build();

//if (app.Environment.IsDevelopment())
//{
//    app.UseSwagger();
//    app.UseSwaggerUI();
//}
//app.UseCors();
//app.UseMiddleware<JwtMiddleware>();
//app.MapControllers();
//app.MapHub<ChatHub>("/hubs/chat");


//using (var scope = app.Services.CreateScope())
//{
//    var db = scope.ServiceProvider.GetRequiredService<ChatDbContext>();
//    db.Database.EnsureCreated();
//}

//app.Run();






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
