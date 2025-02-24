using Api;
using Api.EventHandlers.Dtos;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using WebSocketBoilerplate;

namespace NUnit;

[TestFixture(typeof(RedisConnectionManager))]
public class RedisSpecificTests(Type connectionManagerType) : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            services.Remove(services.SingleOrDefault(d => d.ServiceType == typeof(IConnectionManager)) ??
                            throw new Exception("Could not find instance of " + nameof(IConnectionManager)));
            services.AddSingleton(typeof(IConnectionManager), connectionManagerType);
        });
    }

    [Theory]
    public async Task Redis_Retains_State_Upon_Server_Shutdown()
    {
        _ = CreateClient();
        var wsPort = Environment.GetEnvironmentVariable("PORT");

        if (string.IsNullOrEmpty(wsPort)) throw new Exception("Environment variable WS_PORT is not set");

        var clientId = "client" + Guid.NewGuid();
        var url = "ws://localhost:" + wsPort + "?id=" + clientId;


        var client = new WsRequestClient(
            new[] { typeof(ClientWantsToAuthenticateDto).Assembly },
            url
        );

        await client.ConnectAsync();

        await Task.Delay(1000);

        var manager = Services.GetRequiredService<IConnectionManager>();
        var wsServer = Services.GetRequiredService<CustomWebSocketServer>();

    }
}
