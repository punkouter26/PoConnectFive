using System;
using System.Text;

namespace PoConnectFive.Shared.Models
{
    /// <summary>
    /// Represents the game board state for Connect Five
    /// 
    /// SOLID Principles:
    /// - Single Responsibility: Only manages board state and move validation
    /// - Open/Closed: New win conditions can be added without modifying existing code
    /// - Liskov Substitution: Could be extended for different board implementations
    /// - Interface Segregation: Focused on board-specific operations
    /// - Dependency Inversion: No direct dependencies on other components
    /// 
    /// Design Patterns:
    /// - Value Object Pattern: Immutable state management
    /// - Memento Pattern: Board state can be captured and restored
    /// - Iterator Pattern: Board traversal for win checking
    /// </summary>
    public class GameBoard
    {
        private readonly int[][] _board;
        public const int Rows = 9;
        public const int Columns = 9;
        public const int WinLength = 5;

        public GameBoard()
        {
            _board = new int[Rows][];
            for (int i = 0; i < Rows; i++)
            {
                _board[i] = new int[Columns];
            }
        }

        public int[][] GetBoard() => _board;

        public bool IsValidMove(int column)
        {
            if (column < 0 || column >= Columns)
            {
                return false;
            }

            return _board[0][column] == 0;
        }

        public int GetTargetRow(int column)
        {
            if (column < 0 || column >= Columns)
                throw new ArgumentOutOfRangeException(nameof(column));

            for (int row = Rows - 1; row >= 0; row--)
            {
                if (_board[row][column] == 0)
                {
                    return row;
                }
            }
            return -1;
        }

        public GameBoard PlacePiece(int column, int playerId)
        {
            return CloneWithMove(column, playerId);
        }

        public bool CheckWin(int row, int column, int playerId)
        {
            return CheckDirection(row, column, 0, 1, playerId) ||  // Horizontal
                   CheckDirection(row, column, 1, 0, playerId) ||  // Vertical
                   CheckDirection(row, column, 1, 1, playerId) ||  // Diagonal down-right
                   CheckDirection(row, column, 1, -1, playerId);   // Diagonal down-left
        }

        private bool CheckDirection(int startRow, int startCol, int rowStep, int colStep, int playerId)
        {
            var count = 1;

            // Check in positive direction
            for (int i = 1; i < WinLength; i++)
            {
                var row = startRow + i * rowStep;
                var col = startCol + i * colStep;

                if (row < 0 || row >= Rows || col < 0 || col >= Columns || _board[row][col] != playerId)
                {
                    break;
                }
                count++;
            }

            // Check in negative direction
            for (int i = 1; i < WinLength; i++)
            {
                var row = startRow - i * rowStep;
                var col = startCol - i * colStep;

                if (row < 0 || row >= Rows || col < 0 || col >= Columns || _board[row][col] != playerId)
                {
                    break;
                }
                count++;
            }

            return count >= WinLength;
        }

        public bool HasValidMoves()
        {
            for (int col = 0; col < Columns; col++)
            {
                if (IsValidMove(col))
                {
                    return true;
                }
            }
            return false;
        }

        public int this[int row, int column] => _board[row][column];

        public int GetCell(int row, int column)
        {
            return _board[row][column];
        }

        public GameBoard CloneWithMove(int column, int playerId)
        {
            if (column < 0 || column >= Columns)
                throw new ArgumentOutOfRangeException(nameof(column));

            var targetRow = GetTargetRow(column);
            if (targetRow == -1)
                throw new InvalidOperationException("No valid target row found");

            var newBoard = new GameBoard();
            for (int i = 0; i < Rows; i++)
            {
                Array.Copy(_board[i], newBoard._board[i], Columns);
            }

            newBoard._board[targetRow][column] = playerId;
            return newBoard;
        }

        // Memento Pattern: Board state can be serialized for debugging
        public override string ToString()
        {
            var sb = new StringBuilder(Rows * Columns * 2);
            for (int i = 0; i < Rows; i++)
            {
                for (int j = 0; j < Columns; j++)
                {
                    sb.Append(_board[i][j]);
                    sb.Append(' ');
                }
                sb.AppendLine();
            }
            return sb.ToString();
        }
    }
}
