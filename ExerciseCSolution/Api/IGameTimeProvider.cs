namespace Api;

public interface IGameTimeProvider
{
    public int MilliSeconds { get; set; }

}

public class GameTimeProvider : IGameTimeProvider
{
    public int MilliSeconds { get; set; } = 30_000;
}