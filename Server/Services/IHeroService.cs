namespace Server.Services;

public interface IHeroService
{
    void DoSomething();
}
public class PlayerService : IHeroService
{
    public void DoSomething()
    {
        Console.WriteLine("hey!");
    }
}