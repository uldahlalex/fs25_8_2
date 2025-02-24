using Fleck;
using WebSocketBoilerplate;

namespace Api.EventHandlers;

public class ClientAnswersQuestionEventHandler(IConnectionManager connectionManager) : BaseEventHandler<ClientAnswersQuestionDto>
{
    public override Task Handle(ClientAnswersQuestionDto dto, IWebSocketConnection socket)
    {
        var newState = new GameState()
        {

        };
        connectionManager.BroadcastToTopic<GameState>(dto.RoomId, newState);
    }
}