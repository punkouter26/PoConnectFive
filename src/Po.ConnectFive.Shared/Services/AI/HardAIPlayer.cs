using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PoConnectFive.Shared.Interfaces;
using PoConnectFive.Shared.Models;

namespace PoConnectFive.Shared.Services.AI;

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
/// - Composite Pattern: Evaluates board positions recursively.
/// </summary>
public class HardAIPlayer : IAIPlayer
{
    private const int MAXDEPTH = 5; // Adjust depth based on performance testing
    private readonly Random _random = new Random();
    private readonly IBoardEvaluator _evaluator;
    private readonly AIPersonality _personality;
    public AIDifficulty Difficulty => AIDifficulty.Hard;
    public AIPersonality Personality => _personality;

    public HardAIPlayer()
        : this(new BoardEvaluator(), AIPersonality.Balanced)
    {
    }

    public HardAIPlayer(IBoardEvaluator evaluator)
        : this(evaluator, AIPersonality.Balanced)
    {
    }

    public HardAIPlayer(AIPersonality personality)
        : this(CreateEvaluatorForPersonality(personality), personality)
    {
    }

    public HardAIPlayer(IBoardEvaluator evaluator, AIPersonality personality)
    {
        _evaluator = evaluator ?? throw new ArgumentNullException(nameof(evaluator));
        _personality = personality;
    }

    private static IBoardEvaluator CreateEvaluatorForPersonality(AIPersonality personality)
    {
        return personality switch
        {
            AIPersonality.Aggressive => new AggressiveEvaluator(),
            AIPersonality.Defensive => new DefensiveEvaluator(),
            AIPersonality.Tricky => new TrickyEvaluator(),
            AIPersonality.Balanced => new BoardEvaluator(),
            _ => new BoardEvaluator()
        };
    }

    public Task<int> GetNextMove(GameState gameState)
    {
        var validMoves = GetValidMoves(gameState.Board);

        if (!HasValidMoves(validMoves))
        {
            return Task.FromResult(0);
        }

        if (HasOnlyOneMove(validMoves))
        {
            return Task.FromResult(validMoves[0]);
        }

        var bestMove = FindBestMoveUsingMinimax(gameState, validMoves);
        return Task.FromResult(bestMove);
    }

    private bool HasValidMoves(List<int> validMoves) => validMoves.Count > 0;

    private bool HasOnlyOneMove(List<int> validMoves) => validMoves.Count == 1;

    private int FindBestMoveUsingMinimax(GameState gameState, List<int> validMoves)
    {
        var orderedMoves = PrioritizeCenterColumns(validMoves);
        var (bestMove, bestScore, alpha) = InitializeBestMoveTracking(validMoves);

        foreach (var move in orderedMoves)
        {
            var newBoard = gameState.Board.PlacePiece(move, gameState.CurrentPlayer.Id);

            if (IsWinningMove(newBoard, move, gameState.CurrentPlayer.Id))
            {
                return move;
            }

            var (newBestMove, newBestScore, newAlpha) = EvaluateMoveWithPruning(
                newBoard, move, gameState.CurrentPlayer.Id, bestMove, bestScore, alpha);

            if (ShouldPruneBranch(newAlpha))
            {
                break;
            }

            bestMove = newBestMove;
            bestScore = newBestScore;
            alpha = newAlpha;
        }

        return FallbackToRandomMoveIfNeeded(bestMove, validMoves);
    }

    private List<int> PrioritizeCenterColumns(List<int> validMoves)
    {
        return validMoves.OrderBy(m => Math.Abs(m - GameBoard.Columns / 2)).ToList();
    }

    private (int bestMove, int bestScore, int alpha) InitializeBestMoveTracking(List<int> validMoves)
    {
        return (validMoves[0], int.MinValue, int.MinValue);
    }

    private bool IsWinningMove(GameBoard board, int column, int playerId)
    {
        int row = FindPieceRow(board, column);
        return board.CheckWin(row, column, playerId);
    }

    private (int bestMove, int bestScore, int alpha) EvaluateMoveWithPruning(
        GameBoard board, int move, int playerId, int currentBestMove, int currentBestScore, int alpha)
    {
        int score = Minimax(board, MAXDEPTH - 1, alpha, int.MaxValue, false, playerId);

        if (score > currentBestScore)
        {
            return (move, score, Math.Max(alpha, score));
        }

        return (currentBestMove, currentBestScore, alpha);
    }

    private bool ShouldPruneBranch(int alpha)
    {
        return alpha >= int.MaxValue; // Beta cut-off
    }

    private int FallbackToRandomMoveIfNeeded(int bestMove, List<int> validMoves)
    {
        if (bestMove == -1 && validMoves.Any())
        {
            return validMoves[_random.Next(validMoves.Count)];
        }
        return bestMove;
    }

    // Template Method Pattern: Core recursive algorithm
    private int Minimax(GameBoard board, int depth, int alpha, int beta, bool isMaximizing, int aiPlayerId)
    {
        var validMoves = GetValidMoves(board);

        // Base cases
        if (depth == 0 || validMoves.Count == 0)
        {
            return _evaluator.EvaluateBoard(board, aiPlayerId);
        }

        return isMaximizing
            ? MaximizeScore(board, depth, alpha, beta, aiPlayerId, validMoves)
            : MinimizeScore(board, depth, alpha, beta, aiPlayerId, validMoves);
    }

    private int MaximizeScore(GameBoard board, int depth, int alpha, int beta, int aiPlayerId, List<int> validMoves)
    {
        int maxScore = int.MinValue;

        foreach (var move in validMoves)
        {
            var newBoard = board.PlacePiece(move, aiPlayerId);
            int row = FindPieceRow(newBoard, move);

            // Check if this move resulted in a win for AI
            if (newBoard.CheckWin(row, move, aiPlayerId))
            {
                return 100000 + depth; // Prioritize faster wins
            }

            int score = Minimax(newBoard, depth - 1, alpha, beta, false, aiPlayerId);
            maxScore = Math.Max(maxScore, score);
            alpha = Math.Max(alpha, score);

            if (beta <= alpha)
            {
                break; // Beta cut-off
            }
        }

        return maxScore;
    }

    private int MinimizeScore(GameBoard board, int depth, int alpha, int beta, int aiPlayerId, List<int> validMoves)
    {
        int minScore = int.MaxValue;
        int opponentId = aiPlayerId == 1 ? 2 : 1;

        foreach (var move in validMoves)
        {
            var newBoard = board.PlacePiece(move, opponentId);
            int row = FindPieceRow(newBoard, move);

            // Check if this move resulted in a win for Opponent
            if (newBoard.CheckWin(row, move, opponentId))
            {
                return -100000 - depth; // Prioritize blocking faster losses
            }

            int score = Minimax(newBoard, depth - 1, alpha, beta, true, aiPlayerId);
            minScore = Math.Min(minScore, score);
            beta = Math.Min(beta, score);

            if (beta <= alpha)
            {
                break; // Alpha cut-off
            }
        }

        return minScore;
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
    private int FindPieceRow(GameBoard board, int column)
    {
        for (int row = GameBoard.Rows - 1; row >= 0; row--)
        {
            // Check the specific player ID OR if the cell is non-empty (if called after opponent move)
            if (board.GetCell(row, column) != 0)
            {
                return row;
            }
        }
        // Should ideally not happen if called after a valid PlacePiece
        return GameBoard.Rows - 1; // Fallback: assume bottom row if somehow empty
        // throw new InvalidOperationException($"Piece not found in column {column} after placement.");
    }
}
