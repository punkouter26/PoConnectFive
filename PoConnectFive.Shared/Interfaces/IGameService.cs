using PoConnectFive.Shared.Models;
using System.Threading.Tasks;

namespace PoConnectFive.Shared.Interfaces
{
    /// <summary>
    /// Interface for game service operations
    /// 
    /// SOLID Principles:
    /// - Interface Segregation: Focused on core game operations
    /// - Dependency Inversion: Allows different implementations
    /// </summary>
    public interface IGameService
    {
        Task<GameState> StartNewGame(string player1Name, string player2Name, bool isAIOpponent = false, AIDifficulty? aiDifficulty = null);
        Task<GameState> MakeMove(GameState currentState, int column);
        Task<bool> IsValidMove(GameState currentState, int column);
        Task<int> GetAIMove(GameState currentState);
    }
}
