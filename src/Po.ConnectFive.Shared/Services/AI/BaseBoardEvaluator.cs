using System.Collections.Generic;
using System.Linq;
using PoConnectFive.Shared.Models;

namespace PoConnectFive.Shared.Services.AI;

/// <summary>
/// Abstract base class for board evaluators implementing the Template Method pattern
/// Provides common evaluation logic while allowing subclasses to customize scoring.
/// </summary>
public abstract class BaseBoardEvaluator : IBoardEvaluator
{
    public int EvaluateBoard(GameBoard board, int aiPlayerId)
    {
        int score = 0;
        int opponentId = aiPlayerId == 1 ? 2 : 1;

        // Template Method: Evaluate in all four directions
        score += EvaluateLines(board, aiPlayerId, opponentId, 0, 1);  // Horizontal
        score += EvaluateLines(board, aiPlayerId, opponentId, 1, 0);  // Vertical
        score += EvaluateLines(board, aiPlayerId, opponentId, 1, 1);  // Diagonal \
        score += EvaluateLines(board, aiPlayerId, opponentId, 1, -1); // Diagonal /

        return ApplyFinalAdjustments(score, board, aiPlayerId);
    }

    private int EvaluateLines(GameBoard board, int aiPlayerId, int opponentId, int rowDelta, int colDelta)
    {
        int score = 0;
        int positionBonus = 0;

        for (int row = 0; row < GameBoard.Rows; row++)
        {
            for (int col = 0; col < GameBoard.Columns; col++)
            {
                positionBonus += CalculatePositionBonus(board, row, col, aiPlayerId);
                score += EvaluateSequenceFromPosition(board, row, col, rowDelta, colDelta, aiPlayerId, opponentId);
            }
        }

        return score + positionBonus;
    }

    private int EvaluateSequenceFromPosition(GameBoard board, int row, int col, int rowDelta, int colDelta, int aiPlayerId, int opponentId)
    {
        var sequence = GetSequence(board, row, col, rowDelta, colDelta);

        if (sequence.Count < 5)
        {
            return 0;
        }

        return ScoreSequence(sequence, aiPlayerId, opponentId);
    }

    protected List<int> GetSequence(GameBoard board, int startRow, int startCol, int rowDelta, int colDelta)
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

    protected bool IsValidPosition(int row, int column)
    {
        return row >= 0 && row < GameBoard.Rows && column >= 0 && column < GameBoard.Columns;
    }

    protected int CountPieces(List<int> sequence, int playerId)
    {
        return sequence.Count(x => x == playerId);
    }

    protected bool IsSequenceBlocked(int aiCount, int opponentCount)
    {
        return aiCount > 0 && opponentCount > 0;
    }

    /// <summary>
    /// Hook method: Calculate bonus/penalty for piece position on board
    /// Override to implement personality-specific position heuristics.
    /// </summary>
    protected virtual int CalculatePositionBonus(GameBoard board, int row, int col, int aiPlayerId)
    {
        // Default: Center column bonus
        if (IsInCenterColumns(col) && board.GetCell(row, col) == aiPlayerId)
        {
            return 1;
        }

        return 0;
    }

    /// <summary>
    /// Hook method: Apply final adjustments to total score
    /// Override to add randomness or other global adjustments.
    /// </summary>
    protected virtual int ApplyFinalAdjustments(int score, GameBoard board, int aiPlayerId)
    {
        return score;
    }

    /// <summary>
    /// Abstract method: Score a sequence of 5 cells
    /// Must be implemented by subclasses to define personality-specific scoring.
    /// </summary>
    protected abstract int ScoreSequence(List<int> sequence, int aiPlayerId, int opponentId);

    private bool IsInCenterColumns(int col)
    {
        int centerStart = GameBoard.Columns / 2 - 2;
        int centerEnd = GameBoard.Columns / 2 + 1;
        return col >= centerStart && col <= centerEnd;
    }
}
