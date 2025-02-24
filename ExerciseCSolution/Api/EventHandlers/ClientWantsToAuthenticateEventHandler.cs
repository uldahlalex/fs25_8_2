using System.Security.Authentication;
using Api.EventHandlers.Dtos;
using Fleck;
using WebSocketBoilerplate;

namespace Api.EventHandlers;

public class ClientWantsToAuthenticateEventHandler(IConnectionManager manager, ILogger<ClientWantsToAuthenticateEventHandler> logger)
    : BaseEventHandler<ClientWantsToAuthenticateDto>
{
    public override async Task Handle(ClientWantsToAuthenticateDto dto, IWebSocketConnection socket)
    {
        var clientId = await manager.GetClientIdFromSocketId(socket.ConnectionInfo.Id.ToString());

        if (!Login(dto)) throw new AuthenticationException("Invalid login!");

        await manager.AddToTopic("authenticated", clientId);
        var topics = await manager.GetTopicsFromMemberId(clientId);

        var response = new ServerAuthenticatesClientDto { Topics = topics, requestId = dto.requestId };
        socket.SendDto(response);
    }

    private bool Login(ClientWantsToAuthenticateDto dto)
    {
        //imagine there is a login here
        return true;
    }
}