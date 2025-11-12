using System.Collections.Generic;
using System.Linq;
using PoConnectFive.Shared.Models;

namespace PoConnectFive.Shared.Services.AI;

/// <summary>
/// Defensive board evaluator that heavily prioritizes blocking opponent threats
/// Scores defensive blocking much higher than offensive opportunities.
/// </summary>
public class DefensiveEvaluator : BaseBoardEvaluator
{
    protected override int CalculatePositionBonus(GameBoard board, int row, int col, int aiPlayerId)
    {
        // Defensive: No position bonus - doesn't prioritize center
        return 0;
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

        // DEFENSIVE: Reduced AI offensive weights (still wins if possible)
        if (aiCount == 5)
        {
            score += 100000; // Keep same - must take wins
        }
        else if (aiCount == 4 && emptyCount == 1)
        {
            score += 3000; // Reduced from 5000
        }
        else if (aiCount == 3 && emptyCount == 2)
        {
            score += 300; // Reduced from 500
        }
        else if (aiCount == 2 && emptyCount == 3)
        {
            score += 30; // Reduced from 50
        }
        else if (aiCount == 1 && emptyCount == 4)
        {
            score += 3; // Reduced from 5
        }

        // DEFENSIVE: Heavily boost blocking opponent threats
        if (opponentCount == 5)
        {
            score -= 120000; // +50% penalty
        }
        else if (opponentCount == 4 && emptyCount == 1)
        {
            score -= 6500; // +62.5% penalty
        }
        else if (opponentCount == 3 && emptyCount == 2)
        {
            score -= 650; // +62.5% penalty
        }
        else if (opponentCount == 2 && emptyCount == 3)
        {
            score -= 65; // +62.5% penalty
        }
        else if (opponentCount == 1 && emptyCount == 4)
        {
            score -= 6; // +50% penalty
        }

        return score;
    }
}
