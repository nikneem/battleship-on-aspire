using System.Text;
using HexMaster.BattleShip.Games;
using HexMaster.BattleShip.Games.Endpoints;
using HexMaster.BattleShip.Profiles.Endpoints;
using HexMaster.BattleShip.Profiles;
using HexMaster.BattleShip.Profiles.Abstractions.Claims;
using HexMaster.BattleShip.Profiles.Abstractions.Configuration;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddOpenApi();
builder.Services.AddGamesModule();
builder.Services.AddProfilesModule(builder.Configuration);

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

var app = builder.Build();

app.MapDefaultEndpoints();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapGamesEndpoints();
app.MapProfilesEndpoints();

app.Run();
