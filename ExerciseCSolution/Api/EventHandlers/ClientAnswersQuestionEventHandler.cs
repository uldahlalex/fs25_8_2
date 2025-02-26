using Api.EventHandlers.Dtos;
using EFScaffold;
using EFScaffold.EntityFramework;
using Fleck;
using Microsoft.EntityFrameworkCore;
using WebSocketBoilerplate;

namespace Api.EventHandlers;

public class ClientAnswersQuestionEventHandler(IConnectionManager connectionManager, 
    KahootContext context) : BaseEventHandler<ClientAnswersQuestionDto>
{
    public override async Task Handle(ClientAnswersQuestionDto dto, IWebSocketConnection socket)
    {
        var clientId = await connectionManager.GetClientIdFromSocketId(
            socket.ConnectionInfo.Id.ToString());
        var answer = context.QuestionOptions.First(option => option.Id == dto.optionId);
        context.PlayerAnswers.Add(new PlayerAnswer()
        {
            QuestionId = dto.questionId,
            AnswerTimestamp = DateTime.UtcNow,
            PlayerId = clientId,
            SelectedOptionId = answer.Id,
        });
        context.SaveChanges();
        var confirm = new ServerConfirmsDto()
        {
            Success = true,
            requestId = dto.requestId
        };
        socket.SendDto(confirm);
    }
}