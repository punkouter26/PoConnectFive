namespace PoConnectFive.Shared.Models
{
    public class Player
    {
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
}
