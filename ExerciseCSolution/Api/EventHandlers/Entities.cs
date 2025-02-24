using WebSocketBoilerplate;

namespace Api.EventHandlers;

public class GameState : BaseDto
{
    public List<Player> Players { get; set; }
    public Question CurrentQuestion { get; set; }
    public Dictionary<string, int> Scores { get; set; }
    public string Phase { get; set; }
    public int TimeLeft { get; set; }
    public string CorrectAnswer { get; set; }
}

public class Player : BaseDto
{
    public string Id { get; set; }
    public string Name { get; set; }
    public int Score { get; set; }
}

public class Question : BaseDto
{
    public string Id { get; set; }
    public string QuestionText { get; set; }
    public List<string> Options { get; set; }
}