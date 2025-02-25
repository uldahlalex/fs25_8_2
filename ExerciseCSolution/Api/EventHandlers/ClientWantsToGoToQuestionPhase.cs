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

public class ServerEndsGameRoundDto : BaseDto
{
    public GameQuestionAnswersDTO GameQuestionAnswersDTO { get; set; }
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

        var result = await GetGameQuestionAnswers(dto.GameId);
        
        ServerEndsGameRoundDto serverEndsGameRound = new ServerEndsGameRoundDto()
        {
            GameQuestionAnswersDTO = result
            
        };
        var serialized = JsonSerializer.Serialize(serverEndsGameRound, new JsonSerializerOptions()
        {
            ReferenceHandler = ReferenceHandler.IgnoreCycles
        });
        logger.LogInformation("Server ends game round: "+serialized);
        await connectionManager.BroadcastToTopic<ServerEndsGameRoundDto>("games/" + dto.GameId, 
            JsonSerializer.Deserialize<ServerEndsGameRoundDto>(serialized));

    }
    
    private async Task<GameQuestionAnswersDTO> GetGameQuestionAnswers(string gameId)
    {
        var result = await ctx.Games
            .Include(g => g.Playeranswers)
            .Where(g => g.Id == gameId)
            .Select(g => new GameQuestionAnswersDTO
            {
                GameId = g.Id,
                Questions = g.Gamerounds
                    .Select(gr => gr.Roundquestion)
                    .Select(q => new QuestionAnswersDTO
                    {
                        QuestionId = q.Id,
                        QuestionText = q.Questiontext,
                        PlayerAnswers = q.Playeranswers
                            .Where(pa => pa.Gameid == gameId)
                            .Select(pa => new PlayerAnswerDTO
                            {
                                PlayerId = pa.Playerid,
                                PlayerNickname = pa.Player.Nickname,
                                SelectedOptionId = pa.Optionid,
                                IsCorrect = pa.Option.Iscorrect,
                                AnswerTimestamp = pa.Answertimestamp
                            }).ToList(),
                        Options = q.Questionoptions
                            .Select(qo => new QuestionOptionDTO
                            {
                                OptionId = qo.Id,
                                OptionText = qo.Optiontext,
                                IsCorrect = qo.Iscorrect
                            }).ToList()
                    }).ToList()
            })
            .FirstOrDefaultAsync();

        return result;
    }
}

public class GameQuestionAnswersDTO
{
    public string GameId { get; set; }
    public List<QuestionAnswersDTO> Questions { get; set; }
}

public class QuestionAnswersDTO
{
    public string QuestionId { get; set; }
    public string QuestionText { get; set; }
    public List<PlayerAnswerDTO> PlayerAnswers { get; set; }
    public List<QuestionOptionDTO> Options { get; set; }
}

public class PlayerAnswerDTO
{
    public string PlayerId { get; set; }
    public string PlayerNickname { get; set; }
    public string SelectedOptionId { get; set; }
    public bool IsCorrect { get; set; }
    public DateTime? AnswerTimestamp { get; set; }
}

public class QuestionOptionDTO
{
    public string OptionId { get; set; }
    public string OptionText { get; set; }
    public bool IsCorrect { get; set; }
}