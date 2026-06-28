namespace DuskVillage;

public static class Program
{
    [System.STAThread]
    public static void Main()
    {
        using var game = new Game1();
        game.Run();
    }
}
