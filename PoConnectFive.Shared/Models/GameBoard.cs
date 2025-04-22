using System;
using Microsoft.Extensions.Logging;
using Serilog;

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
        private readonly ILogger<GameBoard> _logger;
        private readonly int[,] _board;
        public const int Rows = 9;
        public const int Columns = 9;
        public const int WinLength = 5;

        public GameBoard(ILogger<GameBoard> logger)
        {
            _logger = logger;
            _board = new int[Rows, Columns];
            _logger.LogInformation("Created new game board with dimensions {Rows}x{Columns}", Rows, Columns);
        }

        public int[,] GetBoard() => _board;

        public bool IsValidMove(int column)
        {
            if (column < 0 || column >= Columns)
            {
                _logger.LogWarning("Invalid column index: {Column}", column);
                return false;
            }

            var isValid = _board[0, column] == 0;
            _logger.LogDebug("Move validation for column {Column}: {IsValid}", column, isValid);
            return isValid;
        }

        public int GetTargetRow(int column)
        {
            for (int row = Rows - 1; row >= 0; row--)
            {
                if (_board[row, column] == 0)
                {
                    _logger.LogDebug("Target row for column {Column} is {Row}", column, row);
                    return row;
                }
            }
            _logger.LogWarning("No valid target row found for column {Column}", column);
            return -1;
        }

        public GameBoard PlacePiece(int column, int playerId)
        {
            _logger.LogInformation("Placing piece for player {PlayerId} in column {Column}", playerId, column);
            
            var newBoard = new GameBoard(_logger);
            Array.Copy(_board, newBoard._board, _board.Length);
            
            var targetRow = GetTargetRow(column);
            if (targetRow == -1)
            {
                _logger.LogError("Failed to place piece - no valid target row");
                throw new InvalidOperationException("No valid target row found");
            }

            newBoard._board[targetRow, column] = playerId;
            _logger.LogDebug("Piece placed at position ({Row}, {Column})", targetRow, column);
            
            return newBoard;
        }

        public bool CheckWin(int row, int column, int playerId)
        {
            _logger.LogInformation("Checking win condition for player {PlayerId} at ({Row}, {Column})", 
                playerId, row, column);

            return CheckDirection(row, column, 1, 0, playerId) ||  // Horizontal
                   CheckDirection(row, column, 0, 1, playerId) ||  // Vertical
                   CheckDirection(row, column, 1, 1, playerId) ||  // Diagonal down-right
                   CheckDirection(row, column, 1, -1, playerId);   // Diagonal down-left
        }

        private bool CheckDirection(int startRow, int startCol, int rowStep, int colStep, int playerId)
        {
            var count = 1;
            _logger.LogDebug("Checking direction ({RowStep}, {ColStep}) from ({StartRow}, {StartCol})", 
                rowStep, colStep, startRow, startCol);

            // Check in positive direction
            for (int i = 1; i < WinLength; i++)
            {
                var row = startRow + i * rowStep;
                var col = startCol + i * colStep;
                
                if (row < 0 || row >= Rows || col < 0 || col >= Columns || _board[row, col] != playerId)
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
                
                if (row < 0 || row >= Rows || col < 0 || col >= Columns || _board[row, col] != playerId)
                {
                    break;
                }
                count++;
            }

            var hasWon = count >= WinLength;
            _logger.LogDebug("Direction check result: {Count} pieces in a row, Win: {HasWon}", count, hasWon);
            return hasWon;
        }

        public bool HasValidMoves()
        {
            for (int col = 0; col < Columns; col++)
            {
                if (IsValidMove(col))
                {
                    _logger.LogDebug("Valid move found in column {Column}", col);
                    return true;
                }
            }
            _logger.LogInformation("No valid moves remaining");
            return false;
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
