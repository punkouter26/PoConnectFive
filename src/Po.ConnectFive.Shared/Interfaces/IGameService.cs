using System.Threading.Tasks;
using PoConnectFive.Shared.Models;

namespace PoConnectFive.Shared.Interfaces;

/// <summary>
/// Interface for game service operations
///
/// SOLID Principles:
/// - Interface Segregation: Focused on core game operations
/// - Dependency Inversion: Allows different implementations.
/// </summary>
public interface IGameService
{
    public Task<GameState> StartNewGame(string player1Name, string player2Name, bool isAIOpponent = false, AIDifficulty? aiDifficulty = null, AIPersonality? aiPersonality = null);
    public Task<GameState> MakeMove(GameState currentState, int column);
    public Task<bool> IsValidMove(GameState currentState, int column);
    public Task<int> GetAIMove(GameState currentState);
}
