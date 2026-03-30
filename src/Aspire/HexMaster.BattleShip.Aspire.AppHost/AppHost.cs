using Aspire.Hosting.ApplicationModel;
using CommunityToolkit.Aspire.Hosting.Dapr;

var builder = DistributedApplication.CreateBuilder(args);

var anonymousPlayerStateStore = builder.AddDaprStateStore("statestore");

builder.AddProject<Projects.HexMaster_BattleShip_Api>("hexmaster-battleship-api")
    .WithDaprSidecar(sidecar => sidecar.WithReference(anonymousPlayerStateStore));

var battleship = builder.AddJavaScriptApp("battleship", @"..\..\App", "start")
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
