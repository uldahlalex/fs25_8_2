using EFScaffold;
using Fleck;
using WebSocketBoilerplate;

namespace Api.EventHandlers;

public class ClientWantsToJoinGameDto : BaseDto
{
    public string GameId { get; set; }
}

public class ServerHasAddedPlayerToGame : BaseDto
{
    public string GameId { get; set; }
    public string PlayerId { get; set; }
}

public class ClientWantsToJoinGame(IConnectionManager connectionManager, KahootContext ctx) : BaseEventHandler<ClientWantsToJoinGameDto>
{
    public override async Task Handle(ClientWantsToJoinGameDto dto, IWebSocketConnection socket)
    {
        var clientId = await connectionManager.GetClientIdFromSocketId(socket.ConnectionInfo.Id.ToString());
        var game = ctx.Games.First(g => g.Id == dto.GameId);
        await connectionManager.AddToTopic("games/" + dto.GameId, clientId);
        var player = ctx.Players.First(p => p.Id == clientId);
        player.Games.Add(game);
        ctx.SaveChanges();
        socket.SendDto(new ServerHasAddedPlayerToGame()
        {
            PlayerId = player.Id,
            GameId = game.Id,
        });
    }
}