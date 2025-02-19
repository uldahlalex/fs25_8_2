using System;
using System.Threading.Tasks;
using Api.EventHandlers.Dtos;
using Fleck;
using WebSocketBoilerplate;

namespace Api.EventHandlers;

public class ClientWantsToJoinRoomEventHandler : BaseEventHandler<ClientWantsToJoinRoomDto>
{
    private readonly IConnectionManager _connectionManager;
    
    public ClientWantsToJoinRoomEventHandler(IConnectionManager connectionManager)
    {
        _connectionManager = connectionManager;
    }

    public override async Task Handle(ClientWantsToJoinRoomDto dto, IWebSocketConnection socket)
    {
        var socketId = socket.ConnectionInfo.Id.ToString();
        var connections = await _connectionManager.GetAllSocketIdsWithConnectionId();
        
        if (!connections.TryGetValue(socketId, out var clientId))
        {
            throw new InvalidOperationException($"No client ID found for socket {socketId}");
        }

        await _connectionManager.AddToTopic(dto.RoomId, clientId);
        
        var response = new ServerConfirmsJoinRoomDto
        {
            RoomId = dto.RoomId,
            Success = true,
            requestId = dto.requestId
        };
        
        socket.SendDto(response);
    }
}