using System;
using System.Collections.Generic;
using System.Linq;
using PoConnectFive.Shared.Models;

namespace PoConnectFive.Shared.Services.AI;

/// <summary>
/// Tricky board evaluator that prefers unconventional and unpredictable moves
/// Adds randomness and favors edge/unusual positions.
/// </summary>
public class TrickyEvaluator : BaseBoardEvaluator
{
    private readonly Random _random = new Random();

    protected override int CalculatePositionBonus(GameBoard board, int row, int col, int aiPlayerId)
    {
        // TRICKY: Bonus for edge columns (unconventional play)
        if ((col == 0 || col == GameBoard.Columns - 1) && board.GetCell(row, col) == aiPlayerId)
        {
            return 5;
        }

        return 0;
    }

    protected override int ApplyFinalAdjustments(int score, GameBoard board, int aiPlayerId)
    {
        // Add randomness to make it unpredictable (±10% variance)
        int variance = _random.Next(-Math.Abs(score / 10), Math.Abs(score / 10) + 1);
        return score + variance;
    }

    protected override int ScoreSequence(List<int> sequence, int aiPlayerId, int opponentId)
    {
        int score = 0;
        int aiCount = CountPieces(sequence, aiPlayerId);
        int opponentCount = CountPieces(sequence, opponentId);
        int emptyCount = 5 - aiCount - opponentCount;

        // Mixed sequences have no potential
        if (IsSequenceBlocked(aiCount, opponentCount))
        {
            return 0;
        }

        // TRICKY: Balanced but slightly unpredictable scoring
        if (aiCount == 5)
        {
            score += 100000;
        }
        else if (aiCount == 4 && emptyCount == 1)
        {
            score += 4500; // Slightly reduced
        }
        else if (aiCount == 3 && emptyCount == 2)
        {
            score += 450; // Slightly reduced
        }
        else if (aiCount == 2 && emptyCount == 3)
        {
            score += 45; // Slightly reduced
        }
        else if (aiCount == 1 && emptyCount == 4)
        {
            score += 4; // Slightly reduced
        }

        // TRICKY: Standard defensive weights but still blocks critical
        if (opponentCount == 5)
        {
            score -= 80000;
        }
        else if (opponentCount == 4 && emptyCount == 1)
        {
            score -= 3500; // Slightly reduced blocking
        }
        else if (opponentCount == 3 && emptyCount == 2)
        {
            score -= 350;
        }
        else if (opponentCount == 2 && emptyCount == 3)
        {
            score -= 35;
        }
        else if (opponentCount == 1 && emptyCount == 4)
        {
            score -= 3;
        }

        // Add small random factor to break ties unpredictably
        score += _random.Next(-10, 11);

        return score;
    }
}
