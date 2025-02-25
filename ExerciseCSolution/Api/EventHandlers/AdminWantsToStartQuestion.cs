using System.Text.Json;
using System.Text.Json.Serialization;
using Api.EventHandlers.Dtos;
using EFScaffold;
using EFScaffold.EntityFramework;
using Fleck;
using Microsoft.EntityFrameworkCore;
using WebSocketBoilerplate;

namespace Api.EventHandlers;

public class AdminWantsToStartQuestionDto : BaseDto
{
    public string Password { get; set; }
    public string GameId { get; set; }
}

public class ServerSendsQuestionDto : BaseDto
{
    public QuestionDTO Question { get; set; }
}

public class ServerEndsGameRoundDto : BaseDto
{
    public GameStateDTO GameStateDto { get; set; }
}

public class ServerEndsGameDto : BaseDto
{
    public GameStateDTO GameStateDto { get; set; }
}

public class AdminWantsToStartQuestion(
    KahootContext ctx,
    IConnectionManager connectionManager,
    ILogger<AdminWantsToStartQuestion> logger,
    IGameTimeProvider gameTimeProvider
) : BaseEventHandler<AdminWantsToStartQuestionDto>
{
    public override async Task Handle(AdminWantsToStartQuestionDto dto, IWebSocketConnection socket)
    {
        if(dto.Password != "ilovewebsockets")
            throw new Exception("Invalid pass");
        var game = ctx.Games
            .Include(g => g.Players)
            .Include(g => g.Questions)
            .ThenInclude(q => q.QuestionOptions)
            .Include(g => g.Questions)
            .ThenInclude(q => q.PlayerAnswers)
            .ThenInclude(pa => pa.Player)
            .Include(g => g.Questions)
            .ThenInclude(q => q.PlayerAnswers)
            .ThenInclude(pa => pa.SelectedOption).First(g => g.Id == dto.GameId);
        var possibleNextQuestions = game.Questions
            .Where(q => q.Answered == false).ToList();

        if (!possibleNextQuestions.Any())
        {
            var serverEndsGame = new ServerEndsGameDto()
            {
                GameStateDto = await GetGameState(dto.GameId)
            };
            await connectionManager.BroadcastToTopic("games/" + dto.GameId, serverEndsGame);
            socket.SendDto(new ServerConfirmsDto()
            {
                Success = true,
                requestId = dto.requestId
            });

            return;
        }

        var nextQuestion = await MapFromQuestionEntity(possibleNextQuestions.First());
        var serverSendsQuestion = new ServerSendsQuestionDto()
        {
            Question = nextQuestion
        };
        
        await connectionManager.BroadcastToTopic("games/" + dto.GameId, serverSendsQuestion);
        socket.SendDto(new ServerConfirmsDto()
        {
            Success = true,
            requestId = dto.requestId
        });
        var question = ctx.Questions.First(q => q.Id == nextQuestion.QuestionId);
        question.Answered = true;
        ctx.SaveChanges();
        await Task.Delay(gameTimeProvider.MilliSeconds);
        await connectionManager.BroadcastToTopic("games/" + dto.GameId, new ServerEndsGameRoundDto()
        {
            GameStateDto = await GetGameState(dto.GameId)
        });
        socket.SendDto(new ServerConfirmsDto()
        {
            Success = true,
            requestId = dto.requestId
        });
    }

    private async Task<QuestionDTO> MapFromQuestionEntity(Question q)
    {
        return new QuestionDTO
        {
            QuestionId = q.Id,
            QuestionText = q.QuestionText,
            IsAnswered = q.Answered,
            Options = q.QuestionOptions
                .Select(qo => new QuestionOptionDTO
                {
                    OptionId = qo.Id,
                    OptionText = qo.OptionText,
                    IsCorrect = qo.IsCorrect
                }).ToList(),
        };
    }

    private async Task<GameStateDTO> GetGameState(string gameId)
    {
        var result = await ctx.Players
            .Where(p => p.GameId == gameId)
            .Select(p => new PlayerScoreDTO
            {
                PlayerId = p.Id,
                Nickname = p.Nickname,
                CorrectAnswers = p.PlayerAnswers.Count(pa => pa.SelectedOption.IsCorrect)
            })
            .ToListAsync();

        return new GameStateDTO
        {
            GameId = gameId,
            PlayerScores = result
        };
    
    }
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
    public bool IsAnswered { get; set; }
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
    public string? SelectedOptionId { get; set; }
    public bool IsCorrect { get; set; }
    public DateTime? AnswerTimestamp { get; set; }
}

// Updated DTOs
public class GameStateDTO
{
    public string GameId { get; set; }
    public List<PlayerScoreDTO> PlayerScores { get; set; }
}

public class PlayerScoreDTO
{
    public string PlayerId { get; set; }
    public string Nickname { get; set; }
    public int CorrectAnswers { get; set; }
}