using WebSocketBoilerplate;

namespace Api.EventHandlers;

public class ClientAnswersQuestionDto : BaseDto
{
    public string Answer { get; set; }
    public string RoomId { get; set; }
}