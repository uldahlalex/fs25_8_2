using WebSocketBoilerplate;

namespace Api.EventHandlers.Dtos;

public class ClientWantsToleaveRoomDto : BaseDto
{
    public string RoomId { get; set; }
}