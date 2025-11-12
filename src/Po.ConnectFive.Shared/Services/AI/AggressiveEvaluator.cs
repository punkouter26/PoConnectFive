using System.Collections.Generic;
using System.Linq;
using PoConnectFive.Shared.Models;

namespace PoConnectFive.Shared.Services.AI;

/// <summary>
/// Aggressive board evaluator that heavily prioritizes offensive opportunities
/// Scores offensive sequences much higher than defensive blocking.
/// </summary>
public class AggressiveEvaluator : BaseBoardEvaluator
{
    protected override int CalculatePositionBonus(GameBoard board, int row, int col, int aiPlayerId)
    {
        // Aggressive: Higher center column bonus
        if (IsInCenterColumns(col) && board.GetCell(row, col) == aiPlayerId)
        {
            return 2;
        }

        return 0;
    }

    private bool IsInCenterColumns(int col)
    {
        int centerStart = GameBoard.Columns / 2 - 2;
        int centerEnd = GameBoard.Columns / 2 + 1;
        return col >= centerStart && col <= centerEnd;
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

        // AGGRESSIVE: Heavily boost AI offensive sequences
        if (aiCount == 5)
        {
            score += 150000; // +50% boost
        }
        else if (aiCount == 4 && emptyCount == 1)
        {
            score += 8000; // +60% boost
        }
        else if (aiCount == 3 && emptyCount == 2)
        {
            score += 800; // +60% boost
        }
        else if (aiCount == 2 && emptyCount == 3)
        {
            score += 80; // +60% boost
        }
        else if (aiCount == 1 && emptyCount == 4)
        {
            score += 8; // +60% boost
        }

        // AGGRESSIVE: Reduced defensive weights (only blocks critical threats)
        if (opponentCount == 5)
        {
            score -= 80000; // Keep same - must block wins
        }
        else if (opponentCount == 4 && emptyCount == 1)
        {
            score -= 3000; // Reduced from 4000
        }
        else if (opponentCount == 3 && emptyCount == 2)
        {
            score -= 200; // Reduced from 400
        }
        else if (opponentCount == 2 && emptyCount == 3)
        {
            score -= 20; // Reduced from 40
        }
        else if (opponentCount == 1 && emptyCount == 4)
        {
            score -= 2; // Reduced from 4
        }

        return score;
    }
}
