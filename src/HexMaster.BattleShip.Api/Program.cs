using System.Text;
using System.Text.Json;
using HexMaster.BattleShip.Api.Infrastructure;
using HexMaster.BattleShip.Core.Eventing;
using HexMaster.BattleShip.Games;
using HexMaster.BattleShip.Games.Endpoints;
using HexMaster.BattleShip.Profiles.Endpoints;
using HexMaster.BattleShip.Profiles;
using HexMaster.BattleShip.Profiles.Abstractions.Claims;
using HexMaster.BattleShip.Profiles.Abstractions.Configuration;
using HexMaster.BattleShip.Realtime;
using HexMaster.BattleShip.Realtime.Endpoints;
using HexMaster.BattleShip.Realtime.Hubs;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddOpenTelemetry()
    .WithTracing(tracing => tracing
        .AddSource("HexMaster.BattleShip.Profiles")
        .AddSource("HexMaster.BattleShip.Games")
        .AddSource("HexMaster.BattleShip.Realtime"))
    .WithMetrics(metrics => metrics
        .AddMeter("HexMaster.BattleShip.Profiles")
        .AddMeter("HexMaster.BattleShip.Games")
        .AddMeter("HexMaster.BattleShip.Realtime"));

builder.Services.AddOpenApi();
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    options.SerializerOptions.PropertyNameCaseInsensitive = true;
});
builder.Services.AddGamesModule();
builder.Services.AddProfilesModule(builder.Configuration);
builder.Services.AddRealtimeModule();
builder.Services.AddSingleton<IEventBus, DaprEventBus>();

var anonymousPlayerSessionOptions = builder.Configuration
    .GetRequiredSection(AnonymousPlayerSessionOptions.SectionName)
    .Get<AnonymousPlayerSessionOptions>()
    ?? throw new InvalidOperationException("Anonymous player session configuration is missing.");

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.MapInboundClaims = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ClockSkew = TimeSpan.Zero,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(anonymousPlayerSessionOptions.JwtSigningKey)),
            NameClaimType = AnonymousPlayerClaimNames.PlayerName,
            ValidAudience = anonymousPlayerSessionOptions.Audience,
            ValidIssuer = anonymousPlayerSessionOptions.Issuer,
            ValidateAudience = true,
            ValidateIssuer = true,
            ValidateIssuerSigningKey = true,
            ValidateLifetime = true
        };
    });

builder.Services.AddAuthorization();

if (builder.Environment.IsDevelopment())
{
    builder.Services.AddCors(options =>
        options.AddDefaultPolicy(policy => policy
            .SetIsOriginAllowed(origin => new Uri(origin).Host == "localhost")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials()));
}
else
{
    var allowedOrigins = builder.Configuration
        .GetSection("Cors:AllowedOrigins")
        .Get<string[]>() ?? [];
    builder.Services.AddCors(options =>
        options.AddDefaultPolicy(policy => policy
            .WithOrigins(allowedOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials()));
}

var app = builder.Build();

app.MapDefaultEndpoints();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseCloudEvents();
// Needs to be disabled for Dapr
// app.UseHttpsRedirection();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();

app.MapSubscribeHandler();
app.MapHub<GameHub>("/hubs/game");
app.MapGamesEndpoints();
app.MapGamesSubscriptions();
app.MapProfilesEndpoints();
app.MapRealtimeSubscriptions();

app.Run();
