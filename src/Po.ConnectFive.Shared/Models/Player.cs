namespace PoConnectFive.Shared.Models;

public class Player
{
    public static readonly Player Red = new Player(1, "Red", PlayerType.Human);
    public static readonly Player Yellow = new Player(2, "Yellow", PlayerType.Human);

    public int Id { get; }
    public string Name { get; }
    public PlayerType Type { get; }
    public AIDifficulty? AIDifficulty { get; }

    public Player(int id, string name, PlayerType type, AIDifficulty? aiDifficulty = null)
    {
        Id = id;
        Name = name;
        Type = type;
        AIDifficulty = aiDifficulty;
    }
}

public enum PlayerType
{
    Human,
    AI
}

public enum AIDifficulty
{
    Easy,
    Medium,
    Hard
}
