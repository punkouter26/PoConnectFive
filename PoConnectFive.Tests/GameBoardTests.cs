using PoConnectFive.Shared.Models;
using System;
using Xunit;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace PoConnectFive.Tests
{
    public class GameBoardTests
    {
        private readonly ILogger<GameBoard> _logger = NullLogger<GameBoard>.Instance;

        [Fact]
        public void PlacePiece_ValidMove_PiecePlacedCorrectly()
        {
            // Arrange
            var board = new GameBoard(_logger);
            int column = 0;
            int playerId = 1;

            // Act
            var newBoard = board.PlacePiece(column, playerId);

            // Assert
            Assert.Equal(playerId, newBoard.GetCell(GameBoard.Rows - 1, column));
        }

        [Fact]
        public void PlacePiece_InvalidColumn_ThrowsException()
        {
            // Arrange
            var board = new GameBoard(_logger);

            // Act & Assert
            Assert.Throws<ArgumentOutOfRangeException>(() => board.PlacePiece(-1, 1));
            Assert.Throws<ArgumentOutOfRangeException>(() => board.PlacePiece(GameBoard.Columns, 1));
        }

        [Fact]
        public void CheckWin_HorizontalWin_ReturnsTrue()
        {
            // Arrange
            var board = new GameBoard(_logger);
            int playerId = 1;
            var newBoard = board;

            // Act - Place 5 pieces horizontally
            for (int i = 0; i < 5; i++)
            {
                newBoard = newBoard.PlacePiece(i, playerId);
            }

            // Assert
            Assert.True(newBoard.CheckWin(GameBoard.Rows - 1, 4, playerId));
        }

        [Fact]
        public void CheckWin_VerticalWin_ReturnsTrue()
        {
            // Arrange
            var board = new GameBoard(_logger);
            int playerId = 1;
            var newBoard = board;

            // Act - Place 5 pieces vertically in the same column
            for (int i = 0; i < 5; i++)
            {
                newBoard = newBoard.PlacePiece(0, playerId);
            }

            // Assert
            Assert.True(newBoard.CheckWin(GameBoard.Rows - 5, 0, playerId));
        }

        [Fact]
        public void CheckWin_DiagonalWin_ReturnsTrue()
        {
            // Arrange
            var board = new GameBoard(_logger);
            int playerId = 1;
            var newBoard = board;

            // Act - Place pieces in a diagonal pattern
            for (int i = 0; i < 5; i++)
            {
                // Place pieces to create a diagonal
                for (int j = 0; j < i; j++)
                {
                    newBoard = newBoard.PlacePiece(i, playerId == 1 ? 2 : 1);
                }
                newBoard = newBoard.PlacePiece(i, playerId);
            }

            // Assert
            Assert.True(newBoard.CheckWin(GameBoard.Rows - 5, 4, playerId));
        }

        [Fact]
        public void CheckWin_NoWin_ReturnsFalse()
        {
            // Arrange
            var board = new GameBoard(_logger);
            int playerId = 1;
            var newBoard = board;

            // Act - Place 4 pieces horizontally (not enough for a win)
            for (int i = 0; i < 4; i++)
            {
                newBoard = newBoard.PlacePiece(i, playerId);
            }

            // Assert
            Assert.False(newBoard.CheckWin(GameBoard.Rows - 1, 3, playerId));
        }

        [Fact]
        public void IsValidMove_EmptyColumn_ReturnsTrue()
        {
            // Arrange
            var board = new GameBoard(_logger);

            // Act & Assert
            Assert.True(board.IsValidMove(0));
        }

        [Fact]
        public void IsValidMove_FullColumn_ReturnsFalse()
        {
            // Arrange
            var board = new GameBoard(_logger);
            var newBoard = board;

            // Fill entire column
            for (int i = 0; i < GameBoard.Rows; i++)
            {
                newBoard = newBoard.PlacePiece(0, 1);
            }

            // Act & Assert
            Assert.False(newBoard.IsValidMove(0));
        }

        [Fact]
        public void GetTargetRow_ValidColumn_ReturnsCorrectRow()
        {
            // Arrange
            var board = new GameBoard(_logger);
            int column = 0;

            // Act
            int targetRow = board.GetTargetRow(column);

            // Assert
            Assert.Equal(GameBoard.Rows - 1, targetRow);
        }

        [Fact]
        public void GetTargetRow_FullColumn_ReturnsMinusOne()
        {
            // Arrange
            var board = new GameBoard(_logger);
            var newBoard = board;

            // Fill entire column
            for (int i = 0; i < GameBoard.Rows; i++)
            {
                newBoard = newBoard.PlacePiece(0, 1);
            }

            // Act
            int targetRow = newBoard.GetTargetRow(0);

            // Assert
            Assert.Equal(-1, targetRow);
        }

        [Fact]
        public void GetTargetRow_InvalidColumn_ReturnsMinusOne()
        {
            // Arrange
            var board = new GameBoard(_logger);

            // Act & Assert
            Assert.Equal(-1, board.GetTargetRow(-1));
            Assert.Equal(-1, board.GetTargetRow(GameBoard.Columns));
        }

        [Fact]
        public void PlacePiece_MultiplePieces_StackedCorrectly()
        {
            // Arrange
            var board = new GameBoard(_logger);
            int column = 0;
            int player1Id = 1;
            int player2Id = 2;

            // Act - Place alternating pieces
            var newBoard = board;
            for (int i = 0; i < 3; i++)
            {
                newBoard = newBoard.PlacePiece(column, player1Id);
                newBoard = newBoard.PlacePiece(column, player2Id);
            }

            // Assert
            Assert.Equal(player1Id, newBoard.GetCell(GameBoard.Rows - 6, column));
            Assert.Equal(player2Id, newBoard.GetCell(GameBoard.Rows - 5, column));
            Assert.Equal(player1Id, newBoard.GetCell(GameBoard.Rows - 4, column));
            Assert.Equal(player2Id, newBoard.GetCell(GameBoard.Rows - 3, column));
            Assert.Equal(player1Id, newBoard.GetCell(GameBoard.Rows - 2, column));
            Assert.Equal(player2Id, newBoard.GetCell(GameBoard.Rows - 1, column));
        }
    }
}
