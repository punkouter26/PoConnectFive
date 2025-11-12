using System.Collections.Generic;
using System.Linq;
using PoConnectFive.Shared.Models;

namespace PoConnectFive.Shared.Services.AI;

/// <summary>
/// Default balanced board evaluator with standard scoring weights.
/// </summary>
public class BoardEvaluator : BaseBoardEvaluator
{
    protected override int ScoreSequence(List<int> sequence, int aiPlayerId, int opponentId)
    {
        int aiCount = CountPieces(sequence, aiPlayerId);
        int opponentCount = CountPieces(sequence, opponentId);
        int emptyCount = 5 - aiCount - opponentCount;

        if (IsSequenceBlocked(aiCount, opponentCount))
        {
            return 0;
        }

        int aiScore = CalculatePlayerScore(aiCount, emptyCount, isAI: true);
        int opponentScore = CalculatePlayerScore(opponentCount, emptyCount, isAI: false);

        return aiScore + opponentScore;
    }

    private int CalculatePlayerScore(int pieceCount, int emptyCount, bool isAI)
    {
        int multiplier = isAI ? 1 : -1;
        int winBonus = isAI ? 100000 : -80000;

        return (pieceCount, emptyCount) switch
        {
            (5, 0) => winBonus,
            (4, 1) => multiplier * (isAI ? 5000 : 4000),
            (3, 2) => multiplier * (isAI ? 500 : 400),
            (2, 3) => multiplier * (isAI ? 50 : 40),
            (1, 4) => multiplier * (isAI ? 5 : 4),
            _ => 0
        };
    }
}
