using Dapr.Client;
using System.Reflection;
using HexMaster.BattleShip.Core.DependencyInjection;
using HexMaster.BattleShip.Profiles.Abstractions.Configuration;
using HexMaster.BattleShip.Profiles.Abstractions.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HexMaster.BattleShip.Profiles;

public static class AnonymousPlayerSessionServiceCollectionExtensions
{
    public static IServiceCollection AddProfilesModule(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddOptions<AnonymousPlayerSessionOptions>()
            .Bind(configuration.GetRequiredSection(AnonymousPlayerSessionOptions.SectionName))
            .ValidateDataAnnotations()
            .Validate(
                static options => options.PlayerRecordTimeToLive > TimeSpan.Zero,
                "Player record time-to-live must be greater than zero.")
            .Validate(
                static options => options.AccessTokenLifetime > TimeSpan.Zero,
                "Access token lifetime must be greater than zero.")
            .Validate(
                static options => options.RenewalWindow > TimeSpan.Zero,
                "Renewal window must be greater than zero.")
            .Validate(
                static options => options.AccessTokenLifetime <= options.PlayerRecordTimeToLive,
                "Access token lifetime must not exceed the player record time-to-live.")
            .Validate(
                static options => options.RenewalWindow < options.AccessTokenLifetime,
                "Renewal window must be shorter than the access token lifetime.")
            .ValidateOnStart();

        services.AddSingleton(TimeProvider.System);
        services.AddSingleton(static _ => new DaprClientBuilder().Build());
        services.AddScoped<IAnonymousPlayerSessionRepository, DaprAnonymousPlayerSessionRepository>();
        services.AddSingleton<IAnonymousPlayerTokenIssuer, JwtAnonymousPlayerTokenIssuer>();
        services.AddSingleton<AnonymousPlayerTokenReader>();
        services.AddHandlersFromAssembly(Assembly.GetExecutingAssembly());

        return services;
    }
}
