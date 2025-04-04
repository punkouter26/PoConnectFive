using PoConnectFive.Shared.Interfaces;
using PoConnectFive.Shared.Models;
using System;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;

namespace PoConnectFive.Shared.Services.AI
{
    /// <summary>
    /// Implements an AI strategy slightly harder than random.
    /// Checks for immediate win/loss, otherwise makes a random move favoring the center.
    /// 
    /// SOLID Principles:
    /// - Single Responsibility: Focused only on easy difficulty AI strategy
    /// - Open/Closed: New AI strategies can be added without modifying this one
    /// - Liskov Substitution: Can be used anywhere IAIPlayer is expected
    /// - Interface Segregation: Implements focused IAIPlayer interface
    /// - Dependency Inversion: Depends on abstractions (GameState, GameBoard)
    /// 
    /// Design Patterns:
    /// - Strategy Pattern: Implements one of multiple possible AI strategies
    /// - State Pattern: Works with immutable game state
    /// </summary>
    public class EasyAIPlayer : IAIPlayer
    {
        private readonly Random _random = new Random();
        public AIDifficulty Difficulty => AIDifficulty.Easy;

        public Task<int> GetNextMove(GameState gameState)
        {
            // Enhancement: Check for immediate win/block, then random (prefer center)

            // 1. Check for winning move
            var winningMove = FindImmediateThreat(gameState, gameState.CurrentPlayer.Id);
            if (winningMove.HasValue)
                return Task.FromResult(winningMove.Value);

            // 2. Check for blocking move
            var opponentId = gameState.CurrentPlayer.Id == 1 ? 2 : 1;
            var blockingMove = FindImmediateThreat(gameState, opponentId);
            if (blockingMove.HasValue)
                return Task.FromResult(blockingMove.Value);
            
            // 3. Make random move, slightly preferring center columns
            var validMoves = new List<int>();
            for (int col = 0; col < GameBoard.Columns; col++)
            {
                if (gameState.Board.IsValidMove(col))
                {
                    validMoves.Add(col);
                }
            }
            
            if (!validMoves.Any()) return Task.FromResult(0); // Should not happen in a valid game state

            // Simple center preference: Add center columns multiple times to bias random choice
            var weightedMoves = new List<int>(validMoves);
            int centerStart = GameBoard.Columns / 2 - 2; // Adjust range as needed
            int centerEnd = GameBoard.Columns / 2 + 1;   // Adjust range as needed
            for (int col = centerStart; col <= centerEnd; col++) {
                if (validMoves.Contains(col)) {
                    // Add center columns again to increase their probability
                    weightedMoves.Add(col); 
                    weightedMoves.Add(col); 
                }
            }

            // Return random move from the potentially weighted list
            return Task.FromResult(weightedMoves[_random.Next(weightedMoves.Count)]);
        }

        // Helper to find if a player can win in the next move in a specific column
        private int? FindImmediateThreat(GameState gameState, int playerId)
        {
            for (int col = 0; col < GameBoard.Columns; col++)
            {
                if (!gameState.Board.IsValidMove(col))
                    continue;

                // Temporarily place the piece to check for a win
                var tempBoard = gameState.Board.PlacePiece(col, playerId);
                
                // Find the row where the piece actually landed
                // Use the GetTargetRow method we added earlier for accuracy
                int row = gameState.Board.GetTargetRow(col); 

                // Check if this move wins ONLY if a valid row was found
                if (row != -1 && tempBoard.CheckWin(row, col, playerId))
                {
                    return col; // This move is a winning/blocking move
                }
            }
            return null; // No immediate threat found
        }
    }
}
