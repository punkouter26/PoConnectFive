using System;

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
        // Value Object Pattern: Private immutable state
        private readonly int[,] _board;
        public const int Rows = 14;
        public const int Columns = 14;
        public const int WinningLength = 5;

        public GameBoard()
        {
            _board = new int[Rows, Columns];
        }

        // Value Object Pattern: Private constructor for cloning
        private GameBoard(int[,] board)
        {
            _board = (int[,])board.Clone();
        }

        // Value Object Pattern: Returns new instance instead of modifying state
        public GameBoard PlacePiece(int column, int playerId)
        {
            if (column < 0 || column >= Columns)
                throw new ArgumentOutOfRangeException(nameof(column));

            var newBoard = new GameBoard(_board);
            
            // Find the lowest empty row in the selected column
            for (int row = Rows - 1; row >= 0; row--)
            {
                if (_board[row, column] == 0)
                {
                    newBoard._board[row, column] = playerId;
                    return newBoard;
                }
            }

            throw new InvalidOperationException("Column is full");
        }

        public bool IsValidMove(int column)
        {
            if (column < 0 || column >= Columns)
                return false;

            // Check if the top cell in the column is empty
            return _board[0, column] == 0;
        }

        // Iterator Pattern: Traverses board in different directions to check for wins
        public bool CheckWin(int row, int column, int playerId)
        {
            Console.WriteLine($"Checking win for player {playerId} at position ({row}, {column})");
            // Check horizontal
            if (CheckDirection(row, column, 0, 1, playerId) ||
                // Check vertical
                CheckDirection(row, column, 1, 0, playerId) ||
                // Check diagonal (top-left to bottom-right)
                CheckDirection(row, column, 1, 1, playerId) ||
                // Check diagonal (top-right to bottom-left)
                CheckDirection(row, column, 1, -1, playerId))
            {
                Console.WriteLine($"Win detected for player {playerId}!");
                return true;
            }

            return false;
        }

        // Iterator Pattern: Directional traversal implementation
        private bool CheckDirection(int row, int column, int rowDelta, int colDelta, int playerId)
        {
            int count = 1;
            int r, c;

            Console.WriteLine($"Checking direction ({rowDelta}, {colDelta}) from ({row}, {column})");

            // Check forward direction
            r = row + rowDelta;
            c = column + colDelta;
            while (IsValidPosition(r, c) && _board[r, c] == playerId)
            {
                count++;
                Console.WriteLine($"Found matching piece at ({r}, {c}), count = {count}");
                r += rowDelta;
                c += colDelta;
            }

            // Check backward direction
            r = row - rowDelta;
            c = column - colDelta;
            while (IsValidPosition(r, c) && _board[r, c] == playerId)
            {
                count++;
                Console.WriteLine($"Found matching piece at ({r}, {c}), count = {count}");
                r -= rowDelta;
                c -= colDelta;
            }

            Console.WriteLine($"Final count in direction ({rowDelta}, {colDelta}): {count}");
            return count >= WinningLength;
        }

        private bool IsValidPosition(int row, int column)
        {
            return row >= 0 && row < Rows && column >= 0 && column < Columns;
        }

        public int GetCell(int row, int column)
        {
            return _board[row, column];
        }

        // Memento Pattern: Board state can be serialized for debugging
        public override string ToString()
        {
            var result = "";
            for (int i = 0; i < Rows; i++)
            {
                for (int j = 0; j < Columns; j++)
                {
                    result += _board[i, j] + " ";
                }
                result += Environment.NewLine;
            }
            return result;
        }
    }
}
