using EFScaffold;
using EFScaffold.EntityFramework;
using Fleck;
using WebSocketBoilerplate;

namespace Api.EventHandlers;

public class ClientWantsToStartAGameDto : BaseDto
{
    public string TemplateId { get; set; }   
}

public class ClientWantsToStartAGame(KahootContext ctx, IConnectionManager connectionManager)
    : BaseEventHandler<ClientWantsToStartAGameDto>
{
    public override async Task Handle(ClientWantsToStartAGameDto dto, IWebSocketConnection socket)
    {
        var clientId = await connectionManager.GetClientIdFromSocketId(socket.ConnectionInfo.Id.ToString());

        var gameId = Guid.NewGuid().ToString();
        var game = new Game()
        {
            Templateid = ctx.Gametemplates.First().Id,
            Id = gameId
        };
        ctx.Games.Add(game);
        ctx.SaveChanges();

        //Broadcast to players browsing lobby

        //Add client to game
        await connectionManager.AddToTopic("games/" + gameId, clientId);
        var result = new ServerAddsClientToGame()
        {
            GameId = gameId,
            requestId = dto.requestId,
        };
        socket.SendDto(result);
    }
}

public class ServerAddsClientToGame : BaseDto
{
    public string GameId { get; set; }
}