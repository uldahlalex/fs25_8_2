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
    public GameStateDTO GameStateDto { get; set; }
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
        // Get game with necessary includes
        var game = await ctx.Games
            .Include(g => g.Players)
            .Include(g => g.Questions)
            .ThenInclude(q => q.QuestionOptions)
            .FirstAsync(g => g.Id == dto.GameId);

        var nextQuestion = game.Questions
            .OrderBy(q => q.QuestionIndex)
            .FirstOrDefault(q => q.QuestionIndex > (game.CurrentQuestionIndex ?? -1));
    
        if (nextQuestion is not null)
        {
            // Update the current question index
            game.CurrentQuestionIndex = nextQuestion.QuestionIndex;
            await ctx.SaveChangesAsync();

            // Send the next question to all clients
            var nextQuestionDto = new ServerSendsQuestionDto()
            {
                Question = JsonSerializer.Deserialize<Question>(
                    JsonSerializer.Serialize(nextQuestion, 
                        new JsonSerializerOptions() { ReferenceHandler = ReferenceHandler.IgnoreCycles }))
            };
            await connectionManager.BroadcastToTopic("games/" + dto.GameId, nextQuestionDto);

            // Wait for the specified time
            await Task.Delay(gameTimeProvider.MilliSeconds);
        }

        // Get and broadcast the updated game state
        var result = await GetGameState(dto.GameId);
    
        var serverEndsGameRound = new ServerEndsGameRoundDto()
        {
            GameStateDto = result
        };

        // Log and broadcast the game state
        var serialized = JsonSerializer.Serialize(serverEndsGameRound, 
            new JsonSerializerOptions() { ReferenceHandler = ReferenceHandler.IgnoreCycles });
        logger.LogInformation("Server ends game round: " + serialized);
    
        await connectionManager.BroadcastToTopic("games/" + dto.GameId, 
            JsonSerializer.Deserialize<ServerEndsGameRoundDto>(serialized));
    }
    private async Task<GameStateDTO> GetGameState(string gameId)
    {
        var result = await ctx.Games
            .Where(g => g.Id == gameId)
            .Select(g => new GameStateDTO
            {
                GameId = g.Id,
                GameName = g.Name,
                CurrentQuestionIndex = g.CurrentQuestionIndex ?? 0,
                Players = g.Players
                    .Select(p => new PlayerDTO
                    {
                        PlayerId = p.Id,
                        Nickname = p.Nickname
                    }).ToList(),
                Questions = g.Questions
                    .OrderBy(q => q.QuestionIndex)
                    .Select(q => new QuestionDTO
                    {
                        QuestionId = q.Id,
                        QuestionText = q.QuestionText,
                        QuestionIndex = q.QuestionIndex,
                        Options = q.QuestionOptions
                            .Select(qo => new QuestionOptionDTO
                            {
                                OptionId = qo.Id,
                                OptionText = qo.OptionText,
                                IsCorrect = qo.IsCorrect
                            }).ToList(),
                        PlayerAnswers = q.PlayerAnswers
                            .Select(pa => new PlayerAnswerDTO
                            {
                                PlayerId = pa.PlayerId,
                                PlayerNickname = pa.Player.Nickname,
                                SelectedOptionId = pa.SelectedOptionId,
                                IsCorrect = pa.SelectedOption.IsCorrect,
                                AnswerTimestamp = pa.AnswerTimestamp
                            }).ToList()
                    }).ToList()
            })
            .FirstOrDefaultAsync();

        return result;
    }
}

public class GameStateDTO
{
    public string GameId { get; set; }
    public string GameName { get; set; }
    public int CurrentQuestionIndex { get; set; }
    public List<PlayerDTO> Players { get; set; }
    public List<QuestionDTO> Questions { get; set; }
}

public class PlayerDTO
{
    public string PlayerId { get; set; }
    public string Nickname { get; set; }
}

public class QuestionDTO
{
    public string QuestionId { get; set; }
    public string QuestionText { get; set; }
    public int QuestionIndex { get; set; }
    public List<QuestionOptionDTO> Options { get; set; }
    public List<PlayerAnswerDTO> PlayerAnswers { get; set; }
}

public class QuestionOptionDTO
{
    public string OptionId { get; set; }
    public string OptionText { get; set; }
    public bool IsCorrect { get; set; }
}

public class PlayerAnswerDTO
{
    public string PlayerId { get; set; }
    public string PlayerNickname { get; set; }
    public string SelectedOptionId { get; set; }
    public bool IsCorrect { get; set; }
    public DateTime? AnswerTimestamp { get; set; }
}