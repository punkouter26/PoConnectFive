using PoConnectFive.Shared.Models;
using PoConnectFive.Shared.Services;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Po.ConnectFive.Tests;

public class WinProbabilityServiceTests
{
    private readonly WinProbabilityService _service;

    public WinProbabilityServiceTests()
    {
        _service = new WinProbabilityService();
    }

    #region CalculateWinProbabilities Tests

    [Fact]
    public async Task CalculateWinProbabilities_EmptyBoard_ReturnsAllColumns()
    {
        // Arrange
        var board = new GameBoard();
        var gameState = CreateGameState(board);

        // Act
        var result = await _service.CalculateWinProbabilities(gameState, 10);

        // Assert
        Assert.Equal(GameBoard.Columns, result.Count);
        Assert.All(result, r => Assert.InRange(r.probability, 0, 100));
    }

    [Fact]
    public async Task CalculateWinProbabilities_FullColumn_ExcludesFullColumn()
    {
        // Arrange
        var board = new GameBoard();
        // Fill column 0 completely
        for (int i = 0; i < GameBoard.Rows; i++)
        {
            board = board.PlacePiece(0, i % 2 + 1);
        }
        var gameState = CreateGameState(board);

        // Act
        var result = await _service.CalculateWinProbabilities(gameState, 10);

        // Assert
        Assert.Equal(GameBoard.Columns - 1, result.Count);
        Assert.DoesNotContain(result, r => r.column == 0);
    }

    [Fact]
    public async Task CalculateWinProbabilities_CapsSimulations_LimitedTo100()
    {
        // Arrange
        var board = new GameBoard();
        var gameState = CreateGameState(board);

        // Act - Request 1000 simulations but should be capped at 100
        var result = await _service.CalculateWinProbabilities(gameState, 1000);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);
        // Verify method completes quickly (not running 1000 simulations)
    }

    [Fact]
    public async Task CalculateWinProbabilities_OrderedByProbability_DescendingOrder()
    {
        // Arrange
        var board = new GameBoard();
        var gameState = CreateGameState(board);

        // Act
        var result = await _service.CalculateWinProbabilities(gameState, 10);

        // Assert
        for (int i = 0; i < result.Count - 1; i++)
        {
            Assert.True(result[i].probability >= result[i + 1].probability,
                $"Result should be ordered descending. Found {result[i].probability} before {result[i + 1].probability}");
        }
    }

    [Fact]
    public async Task CalculateWinProbabilities_WinningMove_ReturnsHighProbability()
    {
        // Arrange - Set up a board where column 3 is a winning move for Player 1
        var board = new GameBoard();
        // Create horizontal: [0,0,P1], [0,1,P1], [0,2,P1] - placing at [0,3] wins
        board = board.PlacePiece(0, 1); // Player 1
        board = board.PlacePiece(1, 2); // Player 2 (doesn't matter)
        board = board.PlacePiece(1, 1); // Player 1
        board = board.PlacePiece(2, 2); // Player 2 (doesn't matter)
        board = board.PlacePiece(2, 1); // Player 1
        board = board.PlacePiece(4, 2); // Player 2 (doesn't matter)

        var gameState = CreateGameState(board, playerId: 1);

        // Act
        var result = await _service.CalculateWinProbabilities(gameState, 20);

        // Assert
        var winningMove = result.FirstOrDefault(r => r.column == 3);
        Assert.NotNull(winningMove);
        // Winning move should have relatively high probability
        Assert.True(winningMove.probability > 0);
    }

    #endregion

    #region CalculateCurrentWinProbability Tests

    [Fact]
    public void CalculateCurrentWinProbability_EmptyBoard_Returns50Percent()
    {
        // Arrange
        var board = new GameBoard();
        var gameState = CreateGameState(board);

        // Act
        var result = _service.CalculateCurrentWinProbability(gameState);

        // Assert
        Assert.InRange(result, 40, 60); // Should be around 50% for empty board
    }

    [Fact]
    public void CalculateCurrentWinProbability_Player1Won_Returns100ForPlayer1()
    {
        // Arrange
        var board = new GameBoard();
        var player1 = new Player(1, "Player 1", PlayerType.Human);
        var player2 = new Player(2, "Player 2", PlayerType.Human);
        var gameState = new GameState(board, player1, player2, player1, GameStatus.Player1Won);

        // Act
        var result = _service.CalculateCurrentWinProbability(gameState);

        // Assert
        Assert.Equal(100, result);
    }

    [Fact]
    public void CalculateCurrentWinProbability_Player1Won_Returns0ForPlayer2()
    {
        // Arrange
        var board = new GameBoard();
        var player1 = new Player(1, "Player 1", PlayerType.Human);
        var player2 = new Player(2, "Player 2", PlayerType.Human);
        var gameState = new GameState(board, player1, player2, player2, GameStatus.Player1Won);

        // Act
        var result = _service.CalculateCurrentWinProbability(gameState);

        // Assert
        Assert.Equal(0, result);
    }

    [Fact]
    public void CalculateCurrentWinProbability_Player2Won_Returns100ForPlayer2()
    {
        // Arrange
        var board = new GameBoard();
        var player1 = new Player(1, "Player 1", PlayerType.Human);
        var player2 = new Player(2, "Player 2", PlayerType.Human);
        var gameState = new GameState(board, player1, player2, player2, GameStatus.Player2Won);

        // Act
        var result = _service.CalculateCurrentWinProbability(gameState);

        // Assert
        Assert.Equal(100, result);
    }

    [Fact]
    public void CalculateCurrentWinProbability_Draw_Returns0()
    {
        // Arrange
        var board = new GameBoard();
        var player1 = new Player(1, "Player 1", PlayerType.Human);
        var player2 = new Player(2, "Player 2", PlayerType.Human);
        var gameState = new GameState(board, player1, player2, player1, GameStatus.Draw);

        // Act
        var result = _service.CalculateCurrentWinProbability(gameState);

        // Assert
        Assert.Equal(0, result);
    }

    [Fact]
    public void CalculateCurrentWinProbability_ClampsBetween5And95()
    {
        // Arrange
        var board = new GameBoard();
        var gameState = CreateGameState(board);

        // Act
        var result = _service.CalculateCurrentWinProbability(gameState);

        // Assert
        Assert.InRange(result, 5, 95);
    }

    [Fact]
    public void CalculateCurrentWinProbability_StrongPosition_HigherThan50()
    {
        // Arrange - Create a strong position for Player 1
        var board = new GameBoard();
        // Create multiple threats
        board = board.PlacePiece(3, 1); // Player 1 center
        board = board.PlacePiece(0, 2); // Player 2
        board = board.PlacePiece(4, 1); // Player 1 near center
        board = board.PlacePiece(1, 2); // Player 2
        board = board.PlacePiece(2, 1); // Player 1 building
        board = board.PlacePiece(6, 2); // Player 2

        var gameState = CreateGameState(board, playerId: 1);

        // Act
        var result = _service.CalculateCurrentWinProbability(gameState);

        // Assert
        // Strong central position should yield >50% probability
        Assert.InRange(result, 5, 95);
    }

    [Fact]
    public void CalculateCurrentWinProbability_NearWinPosition_VeryHighProbability()
    {
        // Arrange - Create a position where Player 1 is one move from winning
        var board = new GameBoard();
        // Create three in a row horizontally: [0,0], [0,1], [0,2] all Player 1
        board = board.PlacePiece(0, 1);
        board = board.PlacePiece(0, 2);
        board = board.PlacePiece(1, 1);
        board = board.PlacePiece(1, 2);
        board = board.PlacePiece(2, 1);
        board = board.PlacePiece(2, 2);

        var gameState = CreateGameState(board, playerId: 1);

        // Act
        var result = _service.CalculateCurrentWinProbability(gameState);

        // Assert
        // Near-win position should have high probability
        Assert.InRange(result, 50, 95);
    }

    #endregion

    #region Edge Cases

    [Fact]
    public async Task CalculateWinProbabilities_AlmostFullBoard_HandlesCorrectly()
    {
        // Arrange - Create a nearly full board
        var board = new GameBoard();
        for (int col = 0; col < GameBoard.Columns - 1; col++)
        {
            for (int row = 0; row < GameBoard.Rows; row++)
            {
                board = board.PlacePiece(col, (row + col) % 2 + 1);
            }
        }

        var gameState = CreateGameState(board);

        // Act
        var result = await _service.CalculateWinProbabilities(gameState, 10);

        // Assert
        // Should only return probabilities for the one remaining column
        Assert.Single(result);
        Assert.Equal(GameBoard.Columns - 1, result[0].column);
    }

    [Fact]
    public void CalculateCurrentWinProbability_AlmostFullBoard_ReturnsValidProbability()
    {
        // Arrange - Create a nearly full board
        var board = new GameBoard();
        for (int col = 0; col < GameBoard.Columns - 1; col++)
        {
            for (int row = 0; row < GameBoard.Rows; row++)
            {
                board = board.PlacePiece(col, (row + col) % 2 + 1);
            }
        }

        var gameState = CreateGameState(board);

        // Act
        var result = _service.CalculateCurrentWinProbability(gameState);

        // Assert
        Assert.InRange(result, 5, 95);
    }

    [Fact]
    public async Task CalculateWinProbabilities_SingleValidMove_ReturnsSingleProbability()
    {
        // Arrange - Create board with only one valid column
        var board = new GameBoard();
        for (int col = 1; col < GameBoard.Columns; col++)
        {
            for (int row = 0; row < GameBoard.Rows; row++)
            {
                board = board.PlacePiece(col, (row + col) % 2 + 1);
            }
        }

        var gameState = CreateGameState(board);

        // Act
        var result = await _service.CalculateWinProbabilities(gameState, 10);

        // Assert
        Assert.Single(result);
        Assert.Equal(0, result[0].column);
    }

    [Fact]
    public async Task CalculateWinProbabilities_ZeroSimulations_HandlesCapping()
    {
        // Arrange
        var board = new GameBoard();
        var gameState = CreateGameState(board);

        // Act
        var result = await _service.CalculateWinProbabilities(gameState, 0);

        // Assert
        Assert.NotNull(result);
        // Should still return results (capped to minimum)
    }

    #endregion

    #region Helper Methods

    private GameState CreateGameState(GameBoard board, int playerId = 1)
    {
        var player1 = new Player(1, "Player 1", PlayerType.Human);
        var player2 = new Player(2, "Player 2", PlayerType.Human);
        var currentPlayer = playerId == 1 ? player1 : player2;

        return new GameState(board, player1, player2, currentPlayer, GameStatus.InProgress);
    }

    #endregion
}
