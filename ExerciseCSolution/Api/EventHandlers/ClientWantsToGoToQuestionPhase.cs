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

public class GameState
{
    
}

public class ServerEndsGameRoundDto : BaseDto
{
    public GameState GameState { get; set; }
}

public class ClientWantsToGoToQuestionPhase(IConnectionManager connectionManager, KahootContext ctx) : BaseEventHandler<ClientWantsToGoToQuestionPhaseDto>
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
        var gameEnd = new ServerEndsGameRoundDto()
        {
            GameState = new GameState()
        };
    }
}
