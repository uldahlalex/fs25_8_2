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
        
        var player = ctx.Players.FirstOrDefault(p => p.Id == clientId);

        if (player is null)
        {
            var p = new Player()
            {
                Id = clientId,
                Nickname = "Bob "+Guid.NewGuid(),
                GameId = game.Id
            };
            player = p;
            ctx.Players.Add(player);
        }

        await connectionManager.AddToTopic("games/" + dto.GameId, clientId);

        ctx.Players.Add(player);
        game.Players.Add(player);
        
        ctx.SaveChanges();
        socket.SendDto(new ServerAddsClientToGameDto()
        {
            
            GameId = game.Id,
        });
    }
}