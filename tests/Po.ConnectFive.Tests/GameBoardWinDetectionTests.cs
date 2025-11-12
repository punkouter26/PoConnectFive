using PoConnectFive.Shared.Models;
using Xunit;

namespace PoConnectFive.Tests;

/// <summary>
/// Critical tests for win detection logic.
/// Tests all win conditions: horizontal, vertical, and both diagonal directions.
/// </summary>
public class GameBoardWinDetectionTests
{
    [Theory]
    [InlineData(new[] { 0, 1, 2, 3, 4 }, 8, 4, "Bottom row horizontal")]
    [InlineData(new[] { 2, 3, 4, 5, 6 }, 8, 6, "Middle columns horizontal")]
    [InlineData(new[] { 4, 5, 6, 7, 8 }, 8, 8, "Right side horizontal")]
    public void CheckWin_FiveInRowHorizontal_ReturnsTrue(int[] moves, int expectedRow, int expectedCol, string scenario)
    {
        // Arrange
        var board = new GameBoard();
        foreach (var col in moves)
        {
            board = board.PlacePiece(col, 1);
        }

        // Act
        var result = board.CheckWin(expectedRow, expectedCol, 1);

        // Assert
        Assert.True(result, $"Failed scenario: {scenario}");
    }

    [Fact]
    public void CheckWin_FourInRowHorizontal_ReturnsFalse()
    {
        // Arrange - Only 4 pieces in a row
        var board = new GameBoard();
        for (int i = 0; i < 4; i++)
        {
            board = board.PlacePiece(i, 1);
        }

        // Act
        var result = board.CheckWin(8, 3, 1);

        // Assert
        Assert.False(result, "Four in a row should not be a win (need 5)");
    }

    [Fact]
    public void CheckWin_VerticalFive_ReturnsTrue()
    {
        // Arrange - Stack 5 pieces in same column
        var board = new GameBoard();
        for (int i = 0; i < 5; i++)
        {
            board = board.PlacePiece(0, 1);
        }

        // Act - Check top piece of the stack
        var result = board.CheckWin(4, 0, 1);

        // Assert
        Assert.True(result, "Five pieces stacked vertically should be a win");
    }

    [Fact]
    public void CheckWin_VerticalFour_ReturnsFalse()
    {
        // Arrange - Stack only 4 pieces
        var board = new GameBoard();
        for (int i = 0; i < 4; i++)
        {
            board = board.PlacePiece(0, 1);
        }

        // Act
        var result = board.CheckWin(5, 0, 1);

        // Assert
        Assert.False(result, "Four pieces vertically should not be a win");
    }

    [Fact]
    public void CheckWin_DiagonalDownRight_ReturnsTrue()
    {
        // Arrange - Create diagonal ↘ pattern
        // Pattern:
        //   0 1 2 3 4
        // 8 X . . . .
        // 7 O X . . .
        // 6 O O X . .
        // 5 O O O X .
        // 4 O O O O X
        var board = new GameBoard();

        // Column 0
        board = board.PlacePiece(0, 1); // (8,0) - X

        // Column 1
        board = board.PlacePiece(1, 2); // (8,1) - O
        board = board.PlacePiece(1, 1); // (7,1) - X

        // Column 2
        board = board.PlacePiece(2, 2); // (8,2) - O
        board = board.PlacePiece(2, 2); // (7,2) - O
        board = board.PlacePiece(2, 1); // (6,2) - X

        // Column 3
        board = board.PlacePiece(3, 2); // (8,3) - O
        board = board.PlacePiece(3, 2); // (7,3) - O
        board = board.PlacePiece(3, 2); // (6,3) - O
        board = board.PlacePiece(3, 1); // (5,3) - X

        // Column 4
        board = board.PlacePiece(4, 2); // (8,4) - O
        board = board.PlacePiece(4, 2); // (7,4) - O
        board = board.PlacePiece(4, 2); // (6,4) - O
        board = board.PlacePiece(4, 2); // (5,4) - O
        board = board.PlacePiece(4, 1); // (4,4) - X

        // Act - Check the last piece placed
        var result = board.CheckWin(4, 4, 1);

        // Assert
        Assert.True(result, "Diagonal down-right should be detected as a win");
    }

    [Fact]
    public void CheckWin_DiagonalUpRight_ReturnsTrue()
    {
        // Arrange - Create diagonal ↗ pattern
        // Pattern:
        //   0 1 2 3 4
        // 4 O O O O X
        // 5 O O O X .
        // 6 O O X . .
        // 7 O X . . .
        // 8 X . . . .
        var board = new GameBoard();

        // Column 0
        for (int i = 0; i < 5; i++)
        {
            board = board.PlacePiece(0, 2); // Fill with O's
        }
        board = board.PlacePiece(0, 1); // (3,0) - X at row 3

        // Column 1
        for (int i = 0; i < 4; i++)
        {
            board = board.PlacePiece(1, 2); // Fill with O's
        }
        board = board.PlacePiece(1, 1); // (4,1) - X at row 4

        // Column 2
        for (int i = 0; i < 3; i++)
        {
            board = board.PlacePiece(2, 2); // Fill with O's
        }
        board = board.PlacePiece(2, 1); // (5,2) - X at row 5

        // Column 3
        for (int i = 0; i < 2; i++)
        {
            board = board.PlacePiece(3, 2); // Fill with O's
        }
        board = board.PlacePiece(3, 1); // (6,3) - X at row 6

        // Column 4
        board = board.PlacePiece(4, 2); // (8,4) - O
        board = board.PlacePiece(4, 1); // (7,4) - X

        // Act - Check the diagonal from (3,0) to (7,4)
        var result = board.CheckWin(7, 4, 1);

        // Assert
        Assert.True(result, "Diagonal up-right should be detected as a win");
    }

    [Fact]
    public void CheckWin_MixedPieces_ReturnsFalse()
    {
        // Arrange - Horizontal line with mixed player pieces
        var board = new GameBoard();
        board = board.PlacePiece(0, 1); // Player 1
        board = board.PlacePiece(1, 1); // Player 1
        board = board.PlacePiece(2, 2); // Player 2 - breaks the sequence
        board = board.PlacePiece(3, 1); // Player 1
        board = board.PlacePiece(4, 1); // Player 1

        // Act
        var result = board.CheckWin(8, 4, 1);

        // Assert
        Assert.False(result, "Mixed player pieces should not result in a win");
    }

    [Fact]
    public void CheckWin_EmptyBoard_ReturnsFalse()
    {
        // Arrange
        var board = new GameBoard();

        // Act
        var result = board.CheckWin(0, 0, 1);

        // Assert
        Assert.False(result, "Empty board should not have any wins");
    }

    [Fact]
    public void CheckWin_SinglePiece_ReturnsFalse()
    {
        // Arrange
        var board = new GameBoard();
        board = board.PlacePiece(4, 1);

        // Act
        var result = board.CheckWin(8, 4, 1);

        // Assert
        Assert.False(result, "Single piece should not be a win");
    }

    [Fact]
    public void CheckWin_DifferentPlayer_ReturnsFalse()
    {
        // Arrange - Create 5 in a row for player 1
        var board = new GameBoard();
        for (int i = 0; i < 5; i++)
        {
            board = board.PlacePiece(i, 1);
        }

        // Act - Check for player 2 instead of player 1
        var result = board.CheckWin(8, 4, 2);

        // Assert
        Assert.False(result, "Win check for wrong player should return false");
    }

    [Fact]
    public void CheckWin_EdgeCases_LeftEdgeVertical()
    {
        // Arrange - Vertical win at left edge (column 0)
        var board = new GameBoard();
        for (int i = 0; i < 5; i++)
        {
            board = board.PlacePiece(0, 1);
        }

        // Act
        var result = board.CheckWin(4, 0, 1);

        // Assert
        Assert.True(result, "Vertical win at left edge should be detected");
    }

    [Fact]
    public void CheckWin_EdgeCases_RightEdgeVertical()
    {
        // Arrange - Vertical win at right edge (column 8)
        var board = new GameBoard();
        for (int i = 0; i < 5; i++)
        {
            board = board.PlacePiece(8, 1);
        }

        // Act
        var result = board.CheckWin(4, 8, 1);

        // Assert
        Assert.True(result, "Vertical win at right edge should be detected");
    }

    [Fact]
    public void CheckWin_EdgeCases_TopRowHorizontal()
    {
        // Arrange - Fill columns 0-4 completely to get to top row
        var board = new GameBoard();
        for (int col = 0; col < 5; col++)
        {
            // Fill each column to the top (9 rows)
            for (int row = 0; row < GameBoard.Rows; row++)
            {
                // Place player 1 pieces for top row, player 2 for the rest
                int playerId = (row == GameBoard.Rows - 1) ? 1 : 2;
                board = board.PlacePiece(col, playerId);
            }
        }

        // Act - Check horizontal win at top row (row 0)
        var result = board.CheckWin(0, 4, 1);

        // Assert
        Assert.True(result, "Horizontal win at top row should be detected");
    }

    [Fact]
    public void CheckWin_LongerThanFive_StillReturnsTrue()
    {
        // Arrange - Create 7 in a row (more than required 5)
        var board = new GameBoard();
        for (int i = 0; i < 7; i++)
        {
            board = board.PlacePiece(i, 1);
        }

        // Act - Check any piece in the sequence
        var result = board.CheckWin(8, 3, 1);

        // Assert
        Assert.True(result, "More than 5 in a row should still be detected as a win");
    }
}
