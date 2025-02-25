using EFScaffold;
using EFScaffold.EntityFramework;
using Fleck;
using WebSocketBoilerplate;

namespace Api.EventHandlers;

public class ClientWantsToJoinGameDto : BaseDto
{
    public string GameId { get; set; }
}


public class ClientWantsToJoinGame(IConnectionManager connectionManager, KahootContext ctx) : BaseEventHandler<ClientWantsToJoinGameDto>
{
    public override async Task Handle(ClientWantsToJoinGameDto dto, IWebSocketConnection socket)
    {
        var clientId = await connectionManager.GetClientIdFromSocketId(socket.ConnectionInfo.Id.ToString());
        var game = ctx.Games.First(g => g.Id == dto.GameId);
        await connectionManager.AddToTopic("games/" + dto.GameId, clientId);
        var player = new Player()
        {
            Id = clientId,
            Nickname = "Bob"
        };
        ctx.Players.Add(player);
        player.Games.Add(game);
        ctx.SaveChanges();
        socket.SendDto(new ServerAddsClientToGameDto()
        {
            
            GameId = game.Id,
        });
    }
}