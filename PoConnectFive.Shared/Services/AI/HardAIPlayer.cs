using PoConnectFive.Shared.Interfaces;
using PoConnectFive.Shared.Models;
using System;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;

namespace PoConnectFive.Shared.Services.AI
{
    /// <summary>
    /// Implements an advanced AI using minimax algorithm with alpha-beta pruning
    /// 
    /// SOLID Principles:
    /// - Single Responsibility: Focused only on hard difficulty AI strategy
    /// - Open/Closed: New evaluation strategies can be added without modifying existing code
    /// - Liskov Substitution: Can be used anywhere IAIPlayer is expected
    /// - Interface Segregation: Implements focused IAIPlayer interface
    /// - Dependency Inversion: Depends on abstractions (GameState, GameBoard)
    /// 
    /// Design Patterns:
    /// - Strategy Pattern: Implements one of multiple possible AI strategies
    /// - Template Method: Base algorithm structure with customizable evaluation
    /// - State Pattern: Works with immutable game state
    /// - Composite Pattern: Evaluates board positions recursively
    /// </summary>
    public class HardAIPlayer : IAIPlayer
    {
        private const int MAX_DEPTH = 5;
        private readonly Random _random = new Random();
        public AIDifficulty Difficulty => AIDifficulty.Hard;

        public Task<int> GetNextMove(GameState gameState)
        {
            var validMoves = GetValidMoves(gameState.Board);
            
            // If only one move available, take it
            if (validMoves.Count == 1)
                return Task.FromResult(validMoves[0]);

            int bestMove = -1;
            int bestScore = int.MinValue;
            int alpha = int.MinValue;
            int beta = int.MaxValue;

            // Template Method Pattern: Core algorithm structure
            foreach (var move in validMoves)
            {
                var newBoard = gameState.Board.PlacePiece(move, gameState.CurrentPlayer.Id);
                int row = FindPieceRow(newBoard, move, gameState.CurrentPlayer.Id);
                
                // Immediate win detection
                if (newBoard.CheckWin(row, move, gameState.CurrentPlayer.Id))
                    return Task.FromResult(move);

                // Composite Pattern: Recursive position evaluation
                int score = Minimax(newBoard, MAX_DEPTH - 1, alpha, beta, false, gameState.CurrentPlayer.Id);
                
                if (score > bestScore)
                {
                    bestScore = score;
                    bestMove = move;
                }
                
                alpha = Math.Max(alpha, bestScore);
                if (beta <= alpha)
                    break;
            }

            return Task.FromResult(bestMove);
        }

        // Template Method Pattern: Core recursive algorithm
        private int Minimax(GameBoard board, int depth, int alpha, int beta, bool isMaximizing, int aiPlayerId)
        {
            var validMoves = GetValidMoves(board);
            
            // Base cases
            if (depth == 0 || validMoves.Count == 0)
                return EvaluateBoard(board, aiPlayerId);

            if (isMaximizing)
            {
                int maxScore = int.MinValue;
                foreach (var move in validMoves)
                {
                    var newBoard = board.PlacePiece(move, aiPlayerId);
                    int score = Minimax(newBoard, depth - 1, alpha, beta, false, aiPlayerId);
                    maxScore = Math.Max(maxScore, score);
                    alpha = Math.Max(alpha, score);
                    if (beta <= alpha)
                        break;
                }
                return maxScore;
            }
            else
            {
                int minScore = int.MaxValue;
                int opponentId = aiPlayerId == 1 ? 2 : 1;
                foreach (var move in validMoves)
                {
                    var newBoard = board.PlacePiece(move, opponentId);
                    int score = Minimax(newBoard, depth - 1, alpha, beta, true, aiPlayerId);
                    minScore = Math.Min(minScore, score);
                    beta = Math.Min(beta, score);
                    if (beta <= alpha)
                        break;
                }
                return minScore;
            }
        }

        // Strategy Pattern: Board evaluation strategy
        private int EvaluateBoard(GameBoard board, int aiPlayerId)
        {
            int score = 0;
            int opponentId = aiPlayerId == 1 ? 2 : 1;

            // Composite Pattern: Evaluates board in different directions
            score += EvaluateLines(board, aiPlayerId, opponentId, 0, 1);  // Horizontal
            score += EvaluateLines(board, aiPlayerId, opponentId, 1, 0);  // Vertical
            score += EvaluateLines(board, aiPlayerId, opponentId, 1, 1);  // Diagonal \
            score += EvaluateLines(board, aiPlayerId, opponentId, 1, -1); // Diagonal /

            return score;
        }

        private int EvaluateLines(GameBoard board, int aiPlayerId, int opponentId, int rowDelta, int colDelta)
        {
            int score = 0;
            
            for (int row = 0; row < GameBoard.Rows; row++)
            {
                for (int col = 0; col < GameBoard.Columns; col++)
                {
                    var sequence = GetSequence(board, row, col, rowDelta, colDelta);
                    if (sequence.Count >= 5)
                    {
                        int aiCount = sequence.Count(x => x == aiPlayerId);
                        int opponentCount = sequence.Count(x => x == opponentId);
                        int emptyCount = sequence.Count(x => x == 0);

                        // Heuristic scoring based on piece configurations
                        if (aiCount == 4 && emptyCount == 1) score += 1000;    // Almost win
                        else if (aiCount == 3 && emptyCount == 2) score += 100;  // Strong threat
                        else if (aiCount == 2 && emptyCount == 3) score += 10;   // Potential threat
                        
                        if (opponentCount == 4 && emptyCount == 1) score -= 800;  // Block urgent threat
                        else if (opponentCount == 3 && emptyCount == 2) score -= 80;  // Block developing threat
                    }
                }
            }

            return score;
        }

        private List<int> GetSequence(GameBoard board, int startRow, int startCol, int rowDelta, int colDelta)
        {
            var sequence = new List<int>();
            int row = startRow;
            int col = startCol;

            for (int i = 0; i < 5 && IsValidPosition(row, col); i++)
            {
                sequence.Add(board.GetCell(row, col));
                row += rowDelta;
                col += colDelta;
            }

            return sequence;
        }

        private List<int> GetValidMoves(GameBoard board)
        {
            var moves = new List<int>();
            for (int col = 0; col < GameBoard.Columns; col++)
            {
                if (board.IsValidMove(col))
                {
                    moves.Add(col);
                }
            }
            return moves;
        }

        private int FindPieceRow(GameBoard board, int column, int playerId)
        {
            for (int row = GameBoard.Rows - 1; row >= 0; row--)
            {
                if (board.GetCell(row, column) == playerId)
                    return row;
            }
            throw new InvalidOperationException("Piece not found in column");
        }

        private bool IsValidPosition(int row, int column)
        {
            return row >= 0 && row < GameBoard.Rows && column >= 0 && column < GameBoard.Columns;
        }
    }
}
