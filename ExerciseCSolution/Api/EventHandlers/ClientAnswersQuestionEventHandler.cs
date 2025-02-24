using Api.EventHandlers.Dtos;
using EFScaffold;
using EFScaffold.EntityFramework;
using Fleck;
using Microsoft.EntityFrameworkCore;
using WebSocketBoilerplate;

namespace Api.EventHandlers;

public class ClientAnswersQuestionEventHandler(IConnectionManager connectionManager, KahootContext context) : BaseEventHandler<ClientAnswersQuestionDto>
{
    public override async Task Handle(ClientAnswersQuestionDto dto, IWebSocketConnection socket)
    {
        var clientId = await connectionManager.GetClientIdFromSocketId(socket.ConnectionInfo.Id.ToString());
        var answer = context.Questionoptions.First(option => option.Id == dto.optionId);
        context.Playeranswers.Add(new Playeranswer()
        {
            Questionid = dto.questionId,
            Answertimestamp = DateTime.Now,
            Playerid = clientId,
            Optionid = answer.Id,
            Gameid = dto.gameId,
        });
        context.SaveChanges();
        var confirm = new ServerConfirmsDto()
        {
            Success = true
        };
        socket.SendDto(confirm);

    }
}