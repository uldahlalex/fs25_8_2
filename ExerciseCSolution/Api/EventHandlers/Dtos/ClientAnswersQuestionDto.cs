using WebSocketBoilerplate;

namespace Api.EventHandlers;

public class ClientAnswersQuestionDto : BaseDto
{
    public string optionId { get; set; }
    public string questionId { get; set; }
}