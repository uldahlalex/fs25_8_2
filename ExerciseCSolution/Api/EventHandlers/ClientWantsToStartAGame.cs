using System.Text.Json;
using EFScaffold;
using EFScaffold.EntityFramework;
using Fleck;
using WebSocketBoilerplate;

namespace Api.EventHandlers;

public class ClientWantsToStartAGameDto : BaseDto
{
    public string TemplateId { get; set; }   
}

public class ClientWantsToStartAGame(ILogger<ClientWantsToStartAGame> logger, KahootContext ctx, IConnectionManager connectionManager)
    : BaseEventHandler<ClientWantsToStartAGameDto>
{
    public override async Task Handle(ClientWantsToStartAGameDto dto, IWebSocketConnection socket)
    {
        var clientId = await connectionManager.GetClientIdFromSocketId(socket.ConnectionInfo.Id.ToString());
        var player = new Player()
        {
            Id = clientId,
            Nickname = "Bob"
        };
        ctx.Players.Add(player);

        var gameId = Guid.NewGuid().ToString();
        var game = new Game()
        {
            Templateid = ctx.Gametemplates.First().Id,
            Id = gameId,
            Players = new List<Player>() {player}
        };
        ctx.Games.Add(game);
        ctx.SaveChanges();
        
        //Add client to game
        await connectionManager.AddToTopic("games/" + gameId, clientId);
        var result = new ServerAddsClientToGameDto()
        {
            GameId = gameId,
            requestId = dto.requestId,
        };
        socket.SendDto(result);
    }
}

public class ServerAddsClientToGameDto : BaseDto
{
    public string GameId { get; set; }
}