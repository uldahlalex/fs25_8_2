using System.Text.Json;
using System.Text.Json.Serialization;
using EFScaffold;
using EFScaffold.EntityFramework;
using Fleck;
using Microsoft.EntityFrameworkCore;
using WebSocketBoilerplate;

namespace Api.EventHandlers;

public class ClientWantsToGoToQuestionPhaseDto : BaseDto
{
    public string GameId { get; set; }
}

public class ServerSendsQuestionDto : BaseDto
{
    public Question Question { get; set; }
}

public class PlayerWithAnswersForGame
{
    public Player Player { get; set; }
    public List<Playeranswer> Answers { get; set; }
    public string GameId { get; set; }
}

public class ServerEndsGameRoundDto : BaseDto
{
    public List<PlayerWithAnswersForGame> GameState { get; set; }
    public bool LastRound { get; set; }
}

public class ClientWantsToGoToQuestionPhase(
    IConnectionManager connectionManager,
    ILogger<ClientWantsToGoToQuestionPhase> logger,
    KahootContext ctx, 
    IGameTimeProvider gameTimeProvider)
    : BaseEventHandler<ClientWantsToGoToQuestionPhaseDto>
{
    public override async Task Handle(ClientWantsToGoToQuestionPhaseDto dto, IWebSocketConnection socket)
    {
        var game = ctx.Games 
            .Include(g => g.Gamerounds)
            .Include(g => g.Template)
            .ThenInclude(t => t.Questions)
            .ThenInclude(q => q.Questionoptions)
           .First(g => g.Id == dto.GameId);
        
        var answeredQuestions = game.Gamerounds.Select(g => g.Roundquestionid);
        var nextQuestion = game.Template.Questions.Where(q => !answeredQuestions.Contains(q.Id)).FirstOrDefault();

        if (nextQuestion is not null)
        {
                 var nextQuestionDto = new ServerSendsQuestionDto()
                    {
                        Question = JsonSerializer.Deserialize<Question>(JsonSerializer.Serialize(nextQuestion, new JsonSerializerOptions() {ReferenceHandler = ReferenceHandler.IgnoreCycles})),
                    };
                    await connectionManager.BroadcastToTopic("games/" + dto.GameId, nextQuestionDto);  
                                                                                                            
                    await Task.Delay(gameTimeProvider.MilliSeconds);
        } 
        
        
        ServerEndsGameRoundDto serverEndsGameRound = new ServerEndsGameRoundDto()
        {
            LastRound = nextQuestion is null,
            GameState = ctx.Players
                .Include(p => p.Games)
                .Include(p => p.Playeranswers)
                .Where(p => p.Games.Any(g => g.Id == dto.GameId))
                .Select(p =>
                    new PlayerWithAnswersForGame()
                    {
                        Player = p,
                        Answers = p.Playeranswers.Where(pa => pa.Gameid == dto.GameId).ToList(),
                        GameId = dto.GameId
                    }
                ).ToList()
        };
        var serialized = JsonSerializer.Serialize(serverEndsGameRound, new JsonSerializerOptions()
        {
            ReferenceHandler = ReferenceHandler.IgnoreCycles
        });
        logger.LogInformation("Server ends game round: "+serialized);
        await connectionManager.BroadcastToTopic<ServerEndsGameRoundDto>("games/" + dto.GameId, 
            JsonSerializer.Deserialize<ServerEndsGameRoundDto>(serialized));

    }
}