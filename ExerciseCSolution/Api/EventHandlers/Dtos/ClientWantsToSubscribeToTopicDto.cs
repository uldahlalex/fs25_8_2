using System.ComponentModel.DataAnnotations;
using WebSocketBoilerplate;

namespace Api.EventHandlers.Dtos;

public class ClientWantsToSubscribeToTopicDto : BaseDto
{
    [MinLength(1)] public string TopicId { get; set; }
}