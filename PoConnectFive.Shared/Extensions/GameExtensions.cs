using System;
using System.Globalization;
using PoConnectFive.Shared.Models;

namespace PoConnectFive.Shared.Extensions
{
    /// <summary>
    /// Extension methods for enhanced game functionality
    /// </summary>
    public static class GameExtensions
    {
        /// <summary>
        /// Converts a string to title case (first letter of each word capitalized)
        /// </summary>
        public static string ToTitleCase(this string input)
        {
            if (string.IsNullOrEmpty(input))
                return string.Empty;

            var textInfo = CultureInfo.CurrentCulture.TextInfo;
            return textInfo.ToTitleCase(input.ToLower());
        }

        /// <summary>
        /// Gets the target row for a piece dropped in the specified column
        /// </summary>
        public static int GetTargetRow(this GameBoard board, int column)
        {
            for (int row = GameBoard.Rows - 1; row >= 0; row--)
            {
                if (board.GetCell(row, column) == 0)
                {
                    return row;
                }
            }
            return -1; // Column is full
        }

        /// <summary>
        /// Creates a copy of the game board with a piece placed in the specified column
        /// </summary>
        public static GameBoard PlacePiece(this GameBoard board, int column, int playerId)
        {
            // Use the existing CloneWithMove method for immutable board operations
            return board.CloneWithMove(column, playerId);
        }

        /// <summary>
        /// Checks if there's a win condition at the specified position
        /// </summary>
        public static bool CheckWin(this GameBoard board, int row, int column, int playerId)
        {
            // Check horizontal
            if (CheckDirection(board, row, column, 0, 1, playerId) >= 5) return true;

            // Check vertical
            if (CheckDirection(board, row, column, 1, 0, playerId) >= 5) return true;

            // Check diagonal (top-left to bottom-right)
            if (CheckDirection(board, row, column, 1, 1, playerId) >= 5) return true;

            // Check diagonal (top-right to bottom-left)
            if (CheckDirection(board, row, column, 1, -1, playerId) >= 5) return true;

            return false;
        }

        private static int CheckDirection(GameBoard board, int row, int column, int deltaRow, int deltaCol, int playerId)
        {
            int count = 1; // Count the current piece

            // Check in positive direction
            for (int i = 1; i < 5; i++)
            {
                int newRow = row + i * deltaRow;
                int newCol = column + i * deltaCol;

                if (newRow < 0 || newRow >= GameBoard.Rows || newCol < 0 || newCol >= GameBoard.Columns)
                    break;

                if (board.GetCell(newRow, newCol) == playerId)
                    count++;
                else
                    break;
            }

            // Check in negative direction
            for (int i = 1; i < 5; i++)
            {
                int newRow = row - i * deltaRow;
                int newCol = column - i * deltaCol;

                if (newRow < 0 || newRow >= GameBoard.Rows || newCol < 0 || newCol >= GameBoard.Columns)
                    break;

                if (board.GetCell(newRow, newCol) == playerId)
                    count++;
                else
                    break;
            }

            return count;
        }

        /// <summary>
        /// Formats a TimeSpan for display in statistics
        /// </summary>
        public static string ToDisplayString(this TimeSpan timeSpan)
        {
            if (timeSpan.TotalHours >= 1)
                return timeSpan.ToString(@"h\:mm\:ss");
            else
                return timeSpan.ToString(@"mm\:ss");
        }

        /// <summary>
        /// Gets a human-readable description of the move quality
        /// </summary>
        public static string GetDescription(this Models.MoveQuality quality)
        {
            return quality switch
            {
                Models.MoveQuality.Excellent => "Excellent move! This is optimal or near-optimal.",
                Models.MoveQuality.Good => "Good move. This strengthens your position.",
                Models.MoveQuality.Average => "Reasonable move. Not the best, but acceptable.",
                Models.MoveQuality.Poor => "Poor move. There were better alternatives available.",
                Models.MoveQuality.Blunder => "Blunder! This significantly weakens your position.",
                _ => "Move quality unknown."
            };
        }

        /// <summary>
        /// Gets a human-readable description of the playing style
        /// </summary>
        public static string GetDescription(this Models.PlayingStyle style)
        {
            return style switch
            {
                Models.PlayingStyle.Aggressive => "You prefer aggressive play, creating threats and forcing your opponent to respond.",
                Models.PlayingStyle.Defensive => "You focus on solid, defensive positions and blocking opponent threats.",
                Models.PlayingStyle.Balanced => "You maintain a good balance between attacking and defending.",
                Models.PlayingStyle.Tactical => "You excel at finding complex tactical combinations and patterns.",
                Models.PlayingStyle.Positional => "You focus on long-term positional advantages and strategic planning.",
                Models.PlayingStyle.Unpredictable => "Your playing style varies significantly between games, making you hard to read.",
                _ => "Playing style not yet determined."
            };
        }

        /// <summary>
        /// Gets the display color for a trend direction
        /// </summary>
        public static string GetDisplayColor(this Models.TrendDirection trend)
        {
            return trend switch
            {
                Models.TrendDirection.Improving => "success",
                Models.TrendDirection.Declining => "danger",
                Models.TrendDirection.Stable => "secondary",
                _ => "secondary"
            };
        }

        /// <summary>
        /// Gets the Font Awesome icon for a trend direction
        /// </summary>
        public static string GetFontAwesomeIcon(this Models.TrendDirection trend)
        {
            return trend switch
            {
                Models.TrendDirection.Improving => "fa-arrow-up",
                Models.TrendDirection.Declining => "fa-arrow-down",
                Models.TrendDirection.Stable => "fa-minus",
                _ => "fa-minus"
            };
        }

        /// <summary>
        /// Converts difficulty string to a standardized format
        /// </summary>
        public static string NormalizeDifficulty(this string difficulty)
        {
            if (string.IsNullOrEmpty(difficulty))
                return "Human";

            return difficulty.ToTitleCase();
        }

        /// <summary>
        /// Calculates the completion percentage for achievements
        /// </summary>
        public static double CalculateCompletionPercentage(int completed, int total)
        {
            return total > 0 ? (double)completed / total * 100 : 0;
        }

        /// <summary>
        /// Determines if a position is in the center area of the board
        /// </summary>
        public static bool IsInCenter(int column)
        {
            int center = GameBoard.Columns / 2;
            return Math.Abs(column - center) <= 1;
        }

        /// <summary>
        /// Gets the strategic value of a column position
        /// </summary>
        public static int GetColumnStrategicValue(int column)
        {
            int center = GameBoard.Columns / 2;
            int distanceFromCenter = Math.Abs(column - center);

            return distanceFromCenter switch
            {
                0 => 5, // Center column is most valuable
                1 => 4, // Adjacent to center
                2 => 3, // One away from adjacent
                3 => 2, // Near edges
                _ => 1  // Edge columns
            };
        }

        /// <summary>
        /// Formats a percentage for display
        /// </summary>
        public static string FormatPercentage(double percentage, int decimals = 1)
        {
            return percentage.ToString($"F{decimals}") + "%";
        }

        /// <summary>
        /// Gets a color class based on win rate
        /// </summary>
        public static string GetWinRateColorClass(double winRate)
        {
            return winRate switch
            {
                >= 75 => "text-success",
                >= 60 => "text-primary",
                >= 45 => "text-warning",
                >= 30 => "text-orange",
                _ => "text-danger"
            };
        }

        /// <summary>
        /// Gets a Bootstrap badge color for difficulty levels
        /// </summary>
        public static string GetDifficultyBadgeColor(string difficulty)
        {
            return difficulty?.ToLower() switch
            {
                "easy" => "success",
                "medium" => "warning",
                "hard" => "danger",
                _ => "secondary"
            };
        }

        /// <summary>
        /// Truncates text to a specified length with ellipsis
        /// </summary>
        public static string Truncate(this string text, int maxLength)
        {
            if (string.IsNullOrEmpty(text) || text.Length <= maxLength)
                return text;

            return text.Substring(0, maxLength - 3) + "...";
        }

        /// <summary>
        /// Converts milliseconds to a human-readable duration
        /// </summary>
        public static string ToHumanReadableDuration(double milliseconds)
        {
            var timeSpan = TimeSpan.FromMilliseconds(milliseconds);

            if (timeSpan.TotalHours >= 1)
                return $"{timeSpan.Hours}h {timeSpan.Minutes}m";
            else if (timeSpan.TotalMinutes >= 1)
                return $"{timeSpan.Minutes}m {timeSpan.Seconds}s";
            else
                return $"{timeSpan.Seconds}.{timeSpan.Milliseconds:D3}s";
        }
    }
}
