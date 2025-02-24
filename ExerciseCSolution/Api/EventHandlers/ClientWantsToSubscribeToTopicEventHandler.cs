using Api.EventHandlers.Dtos;
using Fleck;
using WebSocketBoilerplate;

namespace Api.EventHandlers;

public class ClientWantsToSubscribeToTopicEventHandler(IConnectionManager connectionManager)
    : BaseEventHandler<ClientWantsToSubscribeToTopicDto>
{
    public override async Task Handle(ClientWantsToSubscribeToTopicDto dto, IWebSocketConnection socket)
    {
        var socketId = socket.ConnectionInfo.Id.ToString();
        var connections = await connectionManager.GetAllSocketIdsWithConnectionId();

        if (!connections.TryGetValue(socketId, out var clientId))
            throw new InvalidOperationException($"No client ID found for socket {socketId}");

        await connectionManager.AddToTopic(dto.TopicId, clientId);

        var response = new ServerConfirmsDto
        {
            Success = true,
            requestId = dto.requestId
        };

        socket.SendDto(response);
    }
}