using EFScaffold;
using EFScaffold.EntityFramework;
using Fleck;
using WebSocketBoilerplate;

namespace Api.EventHandlers;

public class AdminWantsToStartGameDto : BaseDto
{
    public string Password { get; set; }
}

public class AdminWantsToStartGame(
    ILogger<AdminWantsToStartGame> logger,
    KahootContext ctx,
    Seeder seeder,
    IConnectionManager connectionManager)
    : BaseEventHandler<AdminWantsToStartGameDto>
{
    public override async Task Handle(AdminWantsToStartGameDto dto, IWebSocketConnection socket)
    {
        if (dto.Password != "ilovewebsockets")
            throw new Exception("Invalid pass");

        var gameId = await seeder.SeedDefaultGameReturnId();
        var game = ctx.Games.First(id => id.Id == gameId);
        var clients = await connectionManager.GetMembersFromTopicId("lobby");
        foreach (var client in clients)
        {
            await connectionManager.AddToTopic("games/" + game.Id, client);
            await connectionManager.RemoveFromTopic("lobby", client);
            if (ctx.Players.Find(client) == null)
            {
                ctx.Players.Add(new Player() { Id = client, GameId = game.Id, Nickname = client });
                ctx.SaveChanges();
            }
        }
        var serverAddsClientToGameDto = new ServerAddsClientToGameDto() { GameId = game.Id, };
        await connectionManager.BroadcastToTopic("games/" + game.Id, serverAddsClientToGameDto);
        socket.SendDto(new AdminHasStartedGameDto() { GameId = game.Id, requestId = dto.requestId });
    }
}

public class ServerAddsClientToGameDto : BaseDto
{
    public string GameId { get; set; }
}

public class AdminHasStartedGameDto : BaseDto
{
    public string GameId { get; set; }
}