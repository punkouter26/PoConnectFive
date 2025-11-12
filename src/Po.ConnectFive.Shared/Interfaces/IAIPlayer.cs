using System.Threading.Tasks;
using PoConnectFive.Shared.Models;

namespace PoConnectFive.Shared.Interfaces;

/// <summary>
/// Defines the contract for AI player operations
/// Strategy pattern implementation for different AI difficulties.
/// </summary>
public interface IAIPlayer
{
    public Task<int> GetNextMove(GameState gameState);
    public AIDifficulty Difficulty { get; }
}
