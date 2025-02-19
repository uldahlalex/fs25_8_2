using System.Collections;
using Api;
using Fleck;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;

namespace Tests;


public class ConnectionManagerTests(ITestOutputHelper outputHelper) : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureLogging(logging =>
        {
            logging.ClearProviders();
            logging.AddXUnit(outputHelper);
        });
    }

    [Fact]
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
        Assert.Equal(manager.ConnectionIdToSocket.Values.First(), ws);
        Assert.Equal(manager.SocketToConnectionId.Values.First(), connectionId);
    }

    [Fact]
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
        Assert.DoesNotContain(manager.ConnectionIdToSocket.Values, s => s.ConnectionInfo.Id == socketId);
        Assert.DoesNotContain(manager.SocketToConnectionId.Values, c => c == connectionId);
    }
}