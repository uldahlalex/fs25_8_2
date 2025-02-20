using Api;
using Fleck;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;

namespace NUnit;

[TestFixture(typeof(RedisConnectionManager))]
[TestFixture(typeof(DictionaryConnectionManager))]
public class ConnectionManagerTests(Type connectionManagerType) : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(IConnectionManager));
            if (descriptor != null) services.Remove(descriptor);

            services.AddSingleton(typeof(IConnectionManager), connectionManagerType);
        });
    }

    [Test]
    public async Task OnConnect_Can_Add_Socket_And_Client_To_Storage()
    {
        // arrange
        var manager = Services.GetRequiredService<IConnectionManager>();

        var connectionId = Guid.NewGuid().ToString();
        var socketId = Guid.NewGuid();
        var wsMock = new Mock<IWebSocketConnection>();
        wsMock.SetupGet(ws => ws.ConnectionInfo.Id).Returns(socketId);
        var ws = wsMock.Object;

        // act
        await manager.OnOpen(ws, connectionId);

        // assert
        if (!manager.ConnectionIdToSocket.Values.Contains(ws))
            throw new Exception("The dictionary " + nameof(manager.ConnectionIdToSocket) +
                                " should contain the websocket with guid " + ws.ConnectionInfo.Id +
                                " as the first value");
        if (!manager.SocketToConnectionId.Values.Contains(connectionId))
            throw new Exception("The dictionary " + nameof(manager.SocketToConnectionId) +
                                " should contain the connectionId with guid " + connectionId +
                                " as the first value");
    }

    [Test]
    public async Task OnClose_Can_Remove_Socket_And_Client_From_Storage()
    {
        // arrange
        var manager = Services.GetRequiredService<IConnectionManager>();

        var connectionId = Guid.NewGuid().ToString();
        var socketId = Guid.NewGuid();
        var wsMock = new Mock<IWebSocketConnection>();
        wsMock.SetupGet(ws => ws.ConnectionInfo.Id).Returns(socketId);
        var ws = wsMock.Object;
        await manager.OnOpen(ws, connectionId);

        // act
        await manager.OnClose(ws, connectionId);

        // assert
        if (manager.ConnectionIdToSocket.Values.Contains(ws))
            throw new Exception("The dictionary " + nameof(manager.ConnectionIdToSocket) +
                                " should not contain the websocket with guid " + ws.ConnectionInfo.Id);
        if (manager.SocketToConnectionId.Values.Contains(connectionId))
            throw new Exception("The dictionary " + nameof(manager.SocketToConnectionId) +
                                " should not contain the connectionId with guid " + connectionId);
    }
}