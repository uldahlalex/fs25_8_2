using System.Text.Json;
using Api;
using Api.EventHandlers.Dtos;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using WebSocketBoilerplate;

namespace NUnit;

public class ConnectionWithWsClient : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            //remove the default connection manager
            var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(IConnectionManager));
            if (descriptor != null) services.Remove(descriptor);

            services.AddSingleton<IConnectionManager, DictionaryConnectionManager>();
        });
    }

    [Theory]
    public async Task Api_Can_Successfully_Add_Connection()
    {
        _ = CreateClient();
        var wsPort = Environment.GetEnvironmentVariable("PORT");

        if (string.IsNullOrEmpty(wsPort)) throw new Exception("Environment variable WS_PORT is not set");

        var clientId = "clientA";
        var url = "ws://localhost:" + wsPort + "?id=" + clientId;


        var client = new WsRequestClient(
            new[] { typeof(ClientWantsToAuthenticateDto).Assembly },
            url
        );

        await client.ConnectAsync();
        var manager = Services.GetRequiredService<IConnectionManager>();
        if (manager.GetAllConnectionIdsWithSocketId().Result.Keys.Count() != 1)
            throw new Exception("ConnectionIdToSocket should have 1 value, but state was: " +
                                "" + JsonSerializer.Serialize(manager.GetAllConnectionIdsWithSocketId()));
        if (manager.GetAllSocketIdsWithConnectionId().Result.Keys.Count() != 1)
            throw new Exception("SocketToConnectionId should have 1 value, but state was: " +
                                "" + JsonSerializer.Serialize(manager.GetAllSocketIdsWithConnectionId()));
    }
}