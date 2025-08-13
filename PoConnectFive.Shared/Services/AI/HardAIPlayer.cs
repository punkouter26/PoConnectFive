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
        private const int MAX_DEPTH = 5; // Adjust depth based on performance testing
        private readonly Random _random = new Random();
        private readonly IBoardEvaluator _evaluator;
        public AIDifficulty Difficulty => AIDifficulty.Hard;

        public HardAIPlayer() : this(new BoardEvaluator())
        {
        }

        public HardAIPlayer(IBoardEvaluator evaluator)
        {
            _evaluator = evaluator ?? throw new ArgumentNullException(nameof(evaluator));
        }

        public Task<int> GetNextMove(GameState gameState)
        {
            var validMoves = GetValidMoves(gameState.Board);

            // If no moves, something is wrong, but return a default
            if (validMoves.Count == 0)
                return Task.FromResult(0);

            // If only one move available, take it
            if (validMoves.Count == 1)
                return Task.FromResult(validMoves[0]);

            int bestMove = validMoves[0]; // Default to first valid move
            int bestScore = int.MinValue;
            int alpha = int.MinValue;
            int beta = int.MaxValue;

            // Template Method Pattern: Core algorithm structure
            // Prioritize center columns slightly for initial moves
            var orderedMoves = validMoves.OrderBy(m => Math.Abs(m - GameBoard.Columns / 2)).ToList();

            foreach (var move in orderedMoves)
            {
                var newBoard = gameState.Board.PlacePiece(move, gameState.CurrentPlayer.Id);
                int row = FindPieceRow(newBoard, move, gameState.CurrentPlayer.Id);

                // Immediate win detection
                if (newBoard.CheckWin(row, move, gameState.CurrentPlayer.Id))
                    return Task.FromResult(move); // Found a winning move

                // Composite Pattern: Recursive position evaluation
                int score = Minimax(newBoard, MAX_DEPTH - 1, alpha, beta, false, gameState.CurrentPlayer.Id);

                // Add a small random factor to break ties and add unpredictability
                // score += _random.Next(-2, 3); 

                if (score > bestScore)
                {
                    bestScore = score;
                    bestMove = move;
                }

                // Alpha-Beta Pruning update
                alpha = Math.Max(alpha, bestScore);
                if (beta <= alpha)
                    break; // Prune the branch
            }

            // If bestMove wasn't updated (e.g., all moves lead to immediate loss), pick a random valid one
            if (bestMove == -1 && validMoves.Any())
            {
                bestMove = validMoves[_random.Next(validMoves.Count)];
            }


            return Task.FromResult(bestMove);
        }

        // Template Method Pattern: Core recursive algorithm
        private int Minimax(GameBoard board, int depth, int alpha, int beta, bool isMaximizing, int aiPlayerId)
        {
            var validMoves = GetValidMoves(board);
            int opponentId = aiPlayerId == 1 ? 2 : 1;

            // Check for terminal state (win/loss/draw) before evaluating depth
            // This requires adding win check logic within Minimax or EvaluateBoard
            // Simplified check: if a player won in the previous move that led here
            // Note: A more robust check would involve checking the current board state for wins.
            // This is computationally expensive, so EvaluateBoard handles terminal scoring for now.

            // Base cases
            if (depth == 0 || validMoves.Count == 0)
            {
                return EvaluateBoard(board, aiPlayerId);
            }

            if (isMaximizing) // AI's turn (Maximize score)
            {
                int maxScore = int.MinValue;
                foreach (var move in validMoves)
                {
                    var newBoard = board.PlacePiece(move, aiPlayerId);
                    // Check if this move resulted in a win for AI
                    int row = FindPieceRow(newBoard, move, aiPlayerId);
                    if (newBoard.CheckWin(row, move, aiPlayerId))
                    {
                        return 100000 + depth; // Prioritize faster wins
                    }

                    int score = Minimax(newBoard, depth - 1, alpha, beta, false, aiPlayerId);
                    maxScore = Math.Max(maxScore, score);
                    alpha = Math.Max(alpha, score);
                    if (beta <= alpha)
                        break; // Beta cut-off
                }
                return maxScore;
            }
            else // Opponent's turn (Minimize score)
            {
                int minScore = int.MaxValue;
                foreach (var move in validMoves)
                {
                    var newBoard = board.PlacePiece(move, opponentId);
                    // Check if this move resulted in a win for Opponent
                    int row = FindPieceRow(newBoard, move, opponentId);
                    if (newBoard.CheckWin(row, move, opponentId))
                    {
                        return -100000 - depth; // Prioritize blocking faster losses
                    }

                    int score = Minimax(newBoard, depth - 1, alpha, beta, true, aiPlayerId);
                    minScore = Math.Min(minScore, score);
                    beta = Math.Min(beta, score);
                    if (beta <= alpha)
                        break; // Alpha cut-off
                }
                return minScore;
            }
        }

        // Strategy Pattern: Board evaluation strategy
        private int EvaluateBoard(GameBoard board, int aiPlayerId)
        {
            // Delegate to injected evaluator for easier testing and strategy swapping
            return _evaluator.EvaluateBoard(board, aiPlayerId);
        }

        // Helper to evaluate all lines on the board
        private int EvaluateLines(GameBoard board, int aiPlayerId, int opponentId, int rowDelta, int colDelta)
        {
            int score = 0;
            int centerColumnBonus = 0; // Bonus for pieces in center columns

            for (int row = 0; row < GameBoard.Rows; row++)
            {
                for (int col = 0; col < GameBoard.Columns; col++)
                {
                    // Add small bonus for pieces in center columns (adjust range as needed)
                    // Example: Columns 6, 7, 8, 9 for a 14-column board (Indices 5, 6, 7, 8)
                    if (col >= GameBoard.Columns / 2 - 2 && col <= GameBoard.Columns / 2 + 1)
                    {
                        if (board.GetCell(row, col) == aiPlayerId) centerColumnBonus += 1;
                        // Optional: Penalize opponent for center control?
                        // else if (board.GetCell(row, col) == opponentId) centerColumnBonus -= 1;
                    }

                    // Check sequences starting from this cell
                    var sequence = GetSequence(board, row, col, rowDelta, colDelta);
                    if (sequence.Count >= 5) // Only score sequences of length 5
                    {
                        score += ScoreSequence(sequence, aiPlayerId, opponentId);
                    }
                }
            }
            return score + centerColumnBonus; // Add center bonus to the total line score
        }

        // More detailed scoring for a sequence of 5 cells
        private int ScoreSequence(List<int> sequence, int aiPlayerId, int opponentId)
        {
            int score = 0;
            int aiCount = sequence.Count(x => x == aiPlayerId);
            int opponentCount = sequence.Count(x => x == opponentId);
            int emptyCount = 5 - aiCount - opponentCount;

            // Rule out sequences blocked by both players (no potential)
            if (aiCount > 0 && opponentCount > 0)
            {
                return 0;
            }

            // AI Scoring - Prioritize longer sequences
            if (aiCount == 5) score += 100000; // AI Wins (Handled in Minimax, but good for evaluation)
            else if (aiCount == 4 && emptyCount == 1) score += 5000;   // AI has 4-in-a-row potential
            else if (aiCount == 3 && emptyCount == 2) score += 500;    // AI has 3-in-a-row potential
            else if (aiCount == 2 && emptyCount == 3) score += 50;     // AI has 2-in-a-row potential
            else if (aiCount == 1 && emptyCount == 4) score += 5;      // AI has 1 piece

            // Opponent Scoring (Negative) - Prioritize blocking longer sequences
            if (opponentCount == 5) score -= 80000; // Opponent Wins (Handled in Minimax)
            else if (opponentCount == 4 && emptyCount == 1) score -= 4000; // Opponent has 4-in-a-row potential (Urgent block)
            else if (opponentCount == 3 && emptyCount == 2) score -= 400;   // Opponent has 3-in-a-row potential
            else if (opponentCount == 2 && emptyCount == 3) score -= 40;    // Opponent has 2-in-a-row potential
            else if (opponentCount == 1 && emptyCount == 4) score -= 4;     // Opponent has 1 piece

            // TODO: Add bonus for "open ends" - requires checking cells beyond the sequence of 5

            return score;
        }


        // Gets a sequence of up to 5 cells in a given direction
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

        // Gets a list of valid columns where a piece can be placed
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

        // Finds the row where a piece landed in a specific column
        private int FindPieceRow(GameBoard board, int column, int playerId)
        {
            for (int row = GameBoard.Rows - 1; row >= 0; row--)
            {
                // Check the specific player ID OR if the cell is non-empty (if called after opponent move)
                if (board.GetCell(row, column) != 0)
                    return row;
            }
            // Should ideally not happen if called after a valid PlacePiece
            return GameBoard.Rows - 1; // Fallback: assume bottom row if somehow empty
            // throw new InvalidOperationException($"Piece not found in column {column} after placement.");
        }

        // Checks if a position is within the board boundaries
        private bool IsValidPosition(int row, int column)
        {
            return row >= 0 && row < GameBoard.Rows && column >= 0 && column < GameBoard.Columns;
        }
    }
}
