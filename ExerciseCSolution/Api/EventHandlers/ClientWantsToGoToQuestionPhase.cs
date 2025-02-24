using EFScaffold;
using EFScaffold.EntityFramework;
using Fleck;
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
        var game = ctx.Games.First(g => g.Id == dto.GameId);
        var questionsForGame = game.TemplateNavigation.Questions;
        var answeredQuestions = game.Gamerounds.Select(g => g.Roundquestionid);
        //where answeredQuestions does not contain question
        var unanseredQuestions = game.TemplateNavigation.Questions.Where(q => !answeredQuestions.Contains(q.Id));
        var question = unanseredQuestions.First();
        var result = new ServerSendsQuestionDto()
        {
            Question = question,
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