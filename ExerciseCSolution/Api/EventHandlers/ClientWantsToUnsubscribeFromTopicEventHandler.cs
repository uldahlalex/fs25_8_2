using Api.EventHandlers.Dtos;
using Fleck;
using WebSocketBoilerplate;

namespace Api.EventHandlers;

public class ClientWantsToUnsubscribeFromTopicEventHandler(IConnectionManager connectionManager)
    : BaseEventHandler<ClientWantsToUnsubscribeFromTopicDto>
{
    public override async Task Handle(ClientWantsToUnsubscribeFromTopicDto dto, IWebSocketConnection socket)
    {
        var clientId = await connectionManager.GetClientIdFromSocketId(socket.ConnectionInfo.Id.ToString());

        await connectionManager.RemoveFromTopic(dto.TopicId,
            clientId);
        var response = new ServerConfirmsDto
        {
            Success = true,
            requestId = dto.requestId
        };
        socket.SendDto(response);
    }
}