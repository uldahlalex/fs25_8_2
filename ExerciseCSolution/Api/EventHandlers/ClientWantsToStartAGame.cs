using System.Text.Json;
using EFScaffold;
using EFScaffold.EntityFramework;
using Fleck;
using WebSocketBoilerplate;

namespace Api.EventHandlers;

public class ClientWantsToStartAGameDto : BaseDto
{
}

public class ClientWantsToStartAGame(ILogger<ClientWantsToStartAGame> logger, KahootContext ctx, IConnectionManager connectionManager)
    : BaseEventHandler<ClientWantsToStartAGameDto>
{
    public override async Task Handle(ClientWantsToStartAGameDto dto, IWebSocketConnection socket)
    {
        var clientId = await connectionManager.GetClientIdFromSocketId(socket.ConnectionInfo.Id.ToString());
        var game = ctx.Games.First();
        var player = new Player()
        {
            Id = clientId,
            Nickname = "Bob",
            GameId = game.Id
        };
        ctx.Players.Add(player);
        game.Players.Add(player);
        ctx.SaveChanges();
        
        await connectionManager.AddToTopic("games/" + game.Id, clientId);
        var result = new ServerAddsClientToGameDto()
        {
            GameId = game.Id,
            requestId = dto.requestId,
        };
        socket.SendDto(result);
    }
}

public class ServerAddsClientToGameDto : BaseDto
{
    public string GameId { get; set; }
}