using PoConnectFive.Shared.Models;
using System;
using Xunit;

namespace PoConnectFive.Tests
{
    public class GameBoardTests
    {
        [Fact]
        public void PlacePiece_ValidMove_PiecePlacedCorrectly()
        {
            // Arrange
            var board = new GameBoard();
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
            var board = new GameBoard();

            // Act & Assert
            Assert.Throws<ArgumentOutOfRangeException>(() => board.PlacePiece(-1, 1));
            Assert.Throws<ArgumentOutOfRangeException>(() => board.PlacePiece(GameBoard.Columns, 1));
        }

        [Fact]
        public void CheckWin_HorizontalWin_ReturnsTrue()
        {
            // Arrange
            var board = new GameBoard();
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
            var board = new GameBoard();
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
        public void IsValidMove_EmptyColumn_ReturnsTrue()
        {
            // Arrange
            var board = new GameBoard();

            // Act & Assert
            Assert.True(board.IsValidMove(0));
        }

        [Fact]
        public void IsValidMove_FullColumn_ReturnsFalse()
        {
            // Arrange
            var board = new GameBoard();
            var newBoard = board;

            // Fill entire column
            for (int i = 0; i < GameBoard.Rows; i++)
            {
                newBoard = newBoard.PlacePiece(0, 1);
            }

            // Act & Assert
            Assert.False(newBoard.IsValidMove(0));
        }
    }
}
