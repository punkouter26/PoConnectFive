using PoConnectFive.Shared.Models;
using System.Threading.Tasks;

namespace PoConnectFive.Shared.Interfaces
{
    /// <summary>
    /// Defines the contract for AI player operations
    /// Strategy pattern implementation for different AI difficulties
    /// </summary>
    public interface IAIPlayer
    {
        Task<int> GetNextMove(GameState gameState);
        AIDifficulty Difficulty { get; }
    }
}
