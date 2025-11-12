using System;
using System.Threading.Tasks;
using PoConnectFive.Shared.Interfaces;
using PoConnectFive.Shared.Models;

namespace PoConnectFive.Shared.Services.AI;

/// <summary>
/// Implements a strategic AI that can block opponent wins and attempt to win
///
/// SOLID Principles:
/// - Single Responsibility: Focused only on medium difficulty AI strategy
/// - Open/Closed: Strategy can be enhanced without modifying other AI implementations
/// - Liskov Substitution: Can be used anywhere IAIPlayer is expected
/// - Interface Segregation: Implements focused IAIPlayer interface
/// - Dependency Inversion: Depends on abstractions (GameState, GameBoard)
///
/// Design Patterns:
/// - Strategy Pattern: Implements one of multiple possible AI strategies
/// - Template Method: Defines basic algorithm structure with win/block detection
/// - State Pattern: Works with immutable game state.
/// </summary>
public class MediumAIPlayer : IAIPlayer
{
    private readonly Random _random = new Random();
    public AIDifficulty Difficulty => AIDifficulty.Medium;

    public Task<int> GetNextMove(GameState gameState)
    {
        // Template Method Pattern: Algorithm structure
        // 1. Try to win
        var winningMove = FindWinningMove(gameState, gameState.CurrentPlayer.Id);
        if (winningMove.HasValue)
        {
            return Task.FromResult(winningMove.Value);
        }

        // 2. Block opponent's winning move
        var opponentId = gameState.CurrentPlayer.Id == 1 ? 2 : 1;
        var blockingMove = FindWinningMove(gameState, opponentId);
        if (blockingMove.HasValue)
        {
            return Task.FromResult(blockingMove.Value);
        }

        // 3. Make random move if no critical moves found
        var validMoves = new System.Collections.Generic.List<int>();
        for (int col = 0; col < GameBoard.Columns; col++)
        {
            if (gameState.Board.IsValidMove(col))
            {
                validMoves.Add(col);
            }
        }

        return Task.FromResult(validMoves[_random.Next(validMoves.Count)]);
    }

    // Strategy Pattern: Win detection strategy
    private int? FindWinningMove(GameState gameState, int playerId)
    {
        // Try each column
        for (int col = 0; col < GameBoard.Columns; col++)
        {
            if (!gameState.Board.IsValidMove(col))
            {
                continue;
            }

            // State Pattern: Create new board state for move
            var newBoard = gameState.Board.PlacePiece(col, playerId);

            // Find where the piece landed
            int row = -1;
            for (int r = GameBoard.Rows - 1; r >= 0; r--)
            {
                if (newBoard.GetCell(r, col) == playerId)
                {
                    row = r;
                    break;
                }
            }

            // Check if this move wins
            if (newBoard.CheckWin(row, col, playerId))
            {
                return col;
            }
        }

        return null;
    }
}
