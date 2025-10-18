using PoConnectFive.Shared.Models;
using System.Collections.Generic;
using System.Linq;

namespace PoConnectFive.Shared.Services.AI
{
    /// <summary>
    /// Aggressive board evaluator that heavily prioritizes offensive opportunities
    /// Scores offensive sequences much higher than defensive blocking
    /// </summary>
    public class AggressiveEvaluator : IBoardEvaluator
    {
        public int EvaluateBoard(GameBoard board, int aiPlayerId)
        {
            int score = 0;
            int opponentId = aiPlayerId == 1 ? 2 : 1;

            // Evaluate in all four directions with aggressive weighting
            score += EvaluateLines(board, aiPlayerId, opponentId, 0, 1);  // Horizontal
            score += EvaluateLines(board, aiPlayerId, opponentId, 1, 0);  // Vertical
            score += EvaluateLines(board, aiPlayerId, opponentId, 1, 1);  // Diagonal \
            score += EvaluateLines(board, aiPlayerId, opponentId, 1, -1); // Diagonal /

            return score;
        }

        private int EvaluateLines(GameBoard board, int aiPlayerId, int opponentId, int rowDelta, int colDelta)
        {
            int score = 0;
            int centerColumnBonus = 0;

            for (int row = 0; row < GameBoard.Rows; row++)
            {
                for (int col = 0; col < GameBoard.Columns; col++)
                {
                    // Center column bonus heuristic
                    if (col >= GameBoard.Columns / 2 - 2 && col <= GameBoard.Columns / 2 + 1)
                    {
                        if (board.GetCell(row, col) == aiPlayerId) centerColumnBonus += 2; // Increased from 1
                    }

                    var sequence = GetSequence(board, row, col, rowDelta, colDelta);
                    if (sequence.Count >= 5)
                    {
                        score += ScoreSequence(sequence, aiPlayerId, opponentId);
                    }
                }
            }

            return score + centerColumnBonus;
        }

        private int ScoreSequence(List<int> sequence, int aiPlayerId, int opponentId)
        {
            int score = 0;
            int aiCount = sequence.Count(x => x == aiPlayerId);
            int opponentCount = sequence.Count(x => x == opponentId);
            int emptyCount = 5 - aiCount - opponentCount;

            // Mixed sequences have no potential
            if (aiCount > 0 && opponentCount > 0)
            {
                return 0;
            }

            // AGGRESSIVE: Heavily boost AI offensive sequences
            if (aiCount == 5) score += 150000; // +50% boost
            else if (aiCount == 4 && emptyCount == 1) score += 8000; // +60% boost
            else if (aiCount == 3 && emptyCount == 2) score += 800; // +60% boost
            else if (aiCount == 2 && emptyCount == 3) score += 80; // +60% boost
            else if (aiCount == 1 && emptyCount == 4) score += 8; // +60% boost

            // AGGRESSIVE: Reduced defensive weights (only blocks critical threats)
            if (opponentCount == 5) score -= 80000; // Keep same - must block wins
            else if (opponentCount == 4 && emptyCount == 1) score -= 3000; // Reduced from 4000
            else if (opponentCount == 3 && emptyCount == 2) score -= 200; // Reduced from 400
            else if (opponentCount == 2 && emptyCount == 3) score -= 20; // Reduced from 40
            else if (opponentCount == 1 && emptyCount == 4) score -= 2; // Reduced from 4

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

        private bool IsValidPosition(int row, int column)
        {
            return row >= 0 && row < GameBoard.Rows && column >= 0 && column < GameBoard.Columns;
        }
    }
}
