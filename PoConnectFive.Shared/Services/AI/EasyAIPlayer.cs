using PoConnectFive.Shared.Interfaces;
using PoConnectFive.Shared.Models;
using System;
using System.Threading.Tasks;

namespace PoConnectFive.Shared.Services.AI
{
    /// <summary>
    /// Implements a simple random-move AI strategy
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
    /// - Null Object Pattern: Provides simplest possible implementation
    /// </summary>
    public class EasyAIPlayer : IAIPlayer
    {
        private readonly Random _random = new Random();
        public AIDifficulty Difficulty => AIDifficulty.Easy;

        public Task<int> GetNextMove(GameState gameState)
        {
            // Strategy Pattern: Simplest possible move selection strategy
            // Null Object Pattern: Minimal implementation that still satisfies interface
            var validMoves = new System.Collections.Generic.List<int>();
            
            // Get all valid moves
            for (int col = 0; col < GameBoard.Columns; col++)
            {
                if (gameState.Board.IsValidMove(col))
                {
                    validMoves.Add(col);
                }
            }

            // Return random valid move
            return Task.FromResult(validMoves[_random.Next(validMoves.Count)]);
        }
    }
}
