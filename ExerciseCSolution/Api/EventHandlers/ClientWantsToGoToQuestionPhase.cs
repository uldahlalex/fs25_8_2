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
}

public class ClientWantsToGoToQuestionPhase(IConnectionManager connectionManager, KahootContext ctx)
    : BaseEventHandler<ClientWantsToGoToQuestionPhaseDto>
{
    public async override Task Handle(ClientWantsToGoToQuestionPhaseDto dto, IWebSocketConnection socket)
    {
        var game = ctx.Games 
            .Include(g => g.Gamerounds)
            .Include(g => g.Template)
            .ThenInclude(t => t.Questions)
           .First(g => g.Id == dto.GameId);
        var questionsForGame = game.Template.Questions;
        var answeredQuestions = game.Gamerounds.Select(g => g.Roundquestionid);
        //where answeredQuestions does not contain question
        var unanseredQuestions = game.Template.Questions.Where(q => !answeredQuestions.Contains(q.Id));
        var question = unanseredQuestions.First();
        var result = new ServerSendsQuestionDto()
        {
            Question = JsonSerializer.Deserialize<Question>(JsonSerializer.Serialize(question, new JsonSerializerOptions() {ReferenceHandler = ReferenceHandler.IgnoreCycles})),
        };
        //broadcast to players in game
        await connectionManager.BroadcastToTopic("games/" + dto.GameId, result);

        //Broadcast game end after 30 seconds
        await Task.Delay(30000);
        ServerEndsGameRoundDto serverEndsGameRound = new ServerEndsGameRoundDto()
        {
            GameState = ctx.Players
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
        await connectionManager.BroadcastToTopic("games/" + dto.GameId, serverEndsGameRound);

    }
}