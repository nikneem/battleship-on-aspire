using Aspire.Hosting.ApplicationModel;
using CommunityToolkit.Aspire.Hosting.Dapr;

var builder = DistributedApplication.CreateBuilder(args);

var anonymousPlayerStateStore = builder.AddDaprStateStore("statestore");
var pubSub = builder.AddDaprPubSub("pubsub");

var api = builder.AddProject<Projects.HexMaster_BattleShip_Api>("hexmaster-battleship-api")
    .WithDaprSidecar(sidecar => sidecar
        .WithReference(anonymousPlayerStateStore)
        .WithReference(pubSub));

var battleship = builder.AddJavaScriptApp("battleship", @"..\..\App", "start")
    .WithEnvironment("BATTLESHIP_API_URL", api.GetEndpoint("https"))
    .WithHttpEndpoint(env: "PORT")
    .WithExternalHttpEndpoints();

battleship.WithArgs(context =>
{
    context.Args.Add("--");
    context.Args.Add("--host");
    context.Args.Add("0.0.0.0");
    context.Args.Add("--port");
    context.Args.Add(battleship.Resource.GetEndpoint("http").Property(EndpointProperty.TargetPort));
});

builder.Build().Run();
