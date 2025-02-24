using WebSocketBoilerplate;

namespace Api.EventHandlers.Dtos;

public class ClientWantsToUnsubscribeFromTopicDto : BaseDto
{
    public string TopicId { get; set; }
}