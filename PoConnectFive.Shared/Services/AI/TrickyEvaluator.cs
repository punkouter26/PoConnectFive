using PoConnectFive.Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PoConnectFive.Shared.Services.AI
{
    /// <summary>
    /// Tricky board evaluator that prefers unconventional and unpredictable moves
    /// Adds randomness and favors edge/unusual positions
    /// </summary>
    public class TrickyEvaluator : IBoardEvaluator
    {
        private readonly Random _random = new Random();

        public int EvaluateBoard(GameBoard board, int aiPlayerId)
        {
            int score = 0;
            int opponentId = aiPlayerId == 1 ? 2 : 1;

            // Evaluate in all four directions
            score += EvaluateLines(board, aiPlayerId, opponentId, 0, 1);  // Horizontal
            score += EvaluateLines(board, aiPlayerId, opponentId, 1, 0);  // Vertical
            score += EvaluateLines(board, aiPlayerId, opponentId, 1, 1);  // Diagonal \
            score += EvaluateLines(board, aiPlayerId, opponentId, 1, -1); // Diagonal /

            // Add randomness to make it unpredictable (±10% variance)
            int variance = _random.Next(-score / 10, score / 10);
            score += variance;

            return score;
        }

        private int EvaluateLines(GameBoard board, int aiPlayerId, int opponentId, int rowDelta, int colDelta)
        {
            int score = 0;
            int edgeColumnBonus = 0;

            for (int row = 0; row < GameBoard.Rows; row++)
            {
                for (int col = 0; col < GameBoard.Columns; col++)
                {
                    // TRICKY: Bonus for edge columns (unconventional play)
                    if (col == 0 || col == GameBoard.Columns - 1)
                    {
                        if (board.GetCell(row, col) == aiPlayerId) edgeColumnBonus += 5;
                    }

                    var sequence = GetSequence(board, row, col, rowDelta, colDelta);
                    if (sequence.Count >= 5)
                    {
                        score += ScoreSequence(sequence, aiPlayerId, opponentId);
                    }
                }
            }

            return score + edgeColumnBonus;
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

            // TRICKY: Balanced but slightly unpredictable scoring
            if (aiCount == 5) score += 100000;
            else if (aiCount == 4 && emptyCount == 1) score += 4500; // Slightly reduced
            else if (aiCount == 3 && emptyCount == 2) score += 450; // Slightly reduced
            else if (aiCount == 2 && emptyCount == 3) score += 45; // Slightly reduced
            else if (aiCount == 1 && emptyCount == 4) score += 4; // Slightly reduced

            // TRICKY: Standard defensive weights but still blocks critical
            if (opponentCount == 5) score -= 80000;
            else if (opponentCount == 4 && emptyCount == 1) score -= 3500; // Slightly reduced blocking
            else if (opponentCount == 3 && emptyCount == 2) score -= 350;
            else if (opponentCount == 2 && emptyCount == 3) score -= 35;
            else if (opponentCount == 1 && emptyCount == 4) score -= 3;

            // Add small random factor to break ties unpredictably
            score += _random.Next(-10, 11);

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
