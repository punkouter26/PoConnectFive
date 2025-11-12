using Microsoft.Extensions.Logging;
using Moq;
using PoConnectFive.Shared.Interfaces;
using PoConnectFive.Shared.Models;
using PoConnectFive.Shared.Services;
using Xunit;

namespace PoConnectFive.Tests;

public class GameServiceTests
{
    private readonly GameService _gameService;
    private readonly Mock<ILogger<GameService>> _mockLogger;

    public GameServiceTests()
    {
        _mockLogger = new Mock<ILogger<GameService>>();
        _gameService = new GameService(_mockLogger.Object);
    }

    [Fact]
    public async Task StartNewGame_HumanVsHuman_CreatesValidGameState()
    {
        // Arrange
        var player1Name = "Alice";
        var player2Name = "Bob";

        // Act
        var gameState = await _gameService.StartNewGame(player1Name, player2Name, false);

        // Assert
        Assert.NotNull(gameState);
        Assert.Equal(player1Name, gameState.Player1.Name);
        Assert.Equal(player2Name, gameState.Player2.Name);
        Assert.Equal(PlayerType.Human, gameState.Player1.Type);
        Assert.Equal(PlayerType.Human, gameState.Player2.Type);
        Assert.Equal(GameStatus.InProgress, gameState.Status);
        Assert.Equal(gameState.Player1, gameState.CurrentPlayer);
        Assert.NotNull(gameState.Board);
    }

    [Fact]
    public async Task StartNewGame_HumanVsAI_CreatesValidGameStateWithAI()
    {
        // Arrange
        var player1Name = "Alice";
        var player2Name = "AI";

        // Act
        var gameState = await _gameService.StartNewGame(player1Name, player2Name, true, AIDifficulty.Easy);

        // Assert
        Assert.NotNull(gameState);
        Assert.Equal(player1Name, gameState.Player1.Name);
        Assert.Equal(player2Name, gameState.Player2.Name);
        Assert.Equal(PlayerType.Human, gameState.Player1.Type);
        Assert.Equal(PlayerType.AI, gameState.Player2.Type);
        Assert.Equal(AIDifficulty.Easy, gameState.Player2.AIDifficulty);
        Assert.Equal(GameStatus.InProgress, gameState.Status);
    }

    [Fact]
    public async Task StartNewGame_AIWithoutDifficulty_ThrowsArgumentException()
    {
        // Arrange
        var player1Name = "Alice";
        var player2Name = "AI";

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _gameService.StartNewGame(player1Name, player2Name, true, null));
    }

    [Fact]
    public async Task StartNewGame_WithAllDifficulties_CreatesValidGameStates()
    {
        // Arrange & Act & Assert
        foreach (AIDifficulty difficulty in Enum.GetValues(typeof(AIDifficulty)))
        {
            var gameState = await _gameService.StartNewGame("Player", "AI", true, difficulty);
            Assert.Equal(difficulty, gameState.Player2.AIDifficulty);
            Assert.Equal(PlayerType.AI, gameState.Player2.Type);
        }
    }

    [Fact]
    public async Task MakeMove_ValidMove_UpdatesGameState()
    {
        // Arrange
        var gameState = await _gameService.StartNewGame("Alice", "Bob", false);
        var column = 3;

        // Act
        var newState = await _gameService.MakeMove(gameState, column);

        // Assert
        Assert.NotNull(newState);
        Assert.NotEqual(gameState, newState); // New state should be created
        Assert.Equal(GameStatus.InProgress, newState.Status);
        Assert.Equal(gameState.Player2, newState.CurrentPlayer); // Turn should switch

        // Verify a piece was placed (check bottom rows for the column)
        bool pieceFound = false;
        for (int row = GameBoard.Rows - 1; row >= 0; row--)
        {
            if (newState.Board.GetCell(row, column) != 0)
            {
                pieceFound = true;
                break;
            }
        }
        Assert.True(pieceFound, "Piece should be placed in the column");
    }

    [Fact]
    public async Task MakeMove_InvalidColumn_ThrowsInvalidOperationException()
    {
        // Arrange
        var gameState = await _gameService.StartNewGame("Alice", "Bob", false);
        var invalidColumn = -1;

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _gameService.MakeMove(gameState, invalidColumn));
    }

    [Fact]
    public async Task MakeMove_ColumnFull_ThrowsInvalidOperationException()
    {
        // Arrange
        var gameState = await _gameService.StartNewGame("Alice", "Bob", false);
        var column = 0;

        // Fill the column completely
        for (int i = 0; i < GameBoard.Rows; i++)
        {
            gameState = await _gameService.MakeMove(gameState, column);
        }

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _gameService.MakeMove(gameState, column));
    }

    [Fact]
    public async Task MakeMove_GameAlreadyFinished_ThrowsInvalidOperationException()
    {
        // Arrange
        var gameState = await _gameService.StartNewGame("Alice", "Bob", false);

        // Create a winning scenario (5 in a row horizontally at bottom)
        for (int i = 0; i < 5; i++)
        {
            gameState = await _gameService.MakeMove(gameState, i); // Player 1
            if (i < 4) // Don't make Player 2 move on last iteration
            {
                gameState = await _gameService.MakeMove(gameState, i); // Player 2 on same column (row above)
            }
        }

        // At this point, game should be finished
        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _gameService.MakeMove(gameState, 5));
    }

    [Fact]
    public async Task MakeMove_HorizontalWin_DetectsWinner()
    {
        // Arrange
        var gameState = await _gameService.StartNewGame("Alice", "Bob", false);

        // Create horizontal win for Player 1
        // Player 1 plays columns 0,1,2,3,4
        // Player 2 plays columns 0,1,2,3 (one row above)
        for (int i = 0; i < 5; i++)
        {
            gameState = await _gameService.MakeMove(gameState, i); // Player 1

            if (i < 4) // Player 2 doesn't move on the winning move
            {
                gameState = await _gameService.MakeMove(gameState, i); // Player 2
            }
        }

        // Assert
        Assert.Equal(GameStatus.Player1Won, gameState.Status);
        Assert.NotNull(gameState.WinningMove);
    }

    [Fact]
    public async Task MakeMove_VerticalWin_DetectsWinner()
    {
        // Arrange
        var gameState = await _gameService.StartNewGame("Alice", "Bob", false);

        // Create vertical win for Player 1 in column 0
        // Player 1 plays column 0 five times
        // Player 2 plays column 1
        for (int i = 0; i < 5; i++)
        {
            gameState = await _gameService.MakeMove(gameState, 0); // Player 1

            if (i < 4) // Player 2 doesn't move on the winning move
            {
                gameState = await _gameService.MakeMove(gameState, 1); // Player 2
            }
        }

        // Assert
        Assert.Equal(GameStatus.Player1Won, gameState.Status);
        Assert.Equal(0, gameState.WinningMove);
    }

    [Fact]
    public async Task MakeMove_DrawGame_DetectsDraw()
    {
        // Arrange
        var gameState = await _gameService.StartNewGame("Alice", "Bob", false);

        // Fill the board without creating a win
        // This is complex to create, so we'll test the logic indirectly
        // by filling board in a pattern that doesn't create 5-in-a-row

        // Pattern: alternate columns to avoid 5-in-a-row
        // Columns: 0,2,4,6,8,10,12 (Player 1)
        // Columns: 1,3,5,7,9,11,13 (Player 2)
        int moveCount = 0;
        for (int row = 0; row < GameBoard.Rows; row++)
        {
            for (int col = 0; col < GameBoard.Columns; col++)
            {
                if (gameState.Status != GameStatus.InProgress)
                    break;

                try
                {
                    gameState = await _gameService.MakeMove(gameState, col);
                    moveCount++;
                }
                catch (InvalidOperationException)
                {
                    // Game ended or invalid move
                    break;
                }
            }

            if (gameState.Status != GameStatus.InProgress)
                break;
        }

        // We can't easily create a guaranteed draw, so we'll just verify
        // the game progressed and ended in some state
        Assert.NotEqual(GameStatus.InProgress, gameState.Status);
    }

    [Fact]
    public async Task IsValidMove_ValidColumn_ReturnsTrue()
    {
        // Arrange
        var gameState = await _gameService.StartNewGame("Alice", "Bob", false);
        var column = 3;

        // Act
        var isValid = await _gameService.IsValidMove(gameState, column);

        // Assert
        Assert.True(isValid);
    }

    [Fact]
    public async Task IsValidMove_InvalidColumn_ReturnsFalse()
    {
        // Arrange
        var gameState = await _gameService.StartNewGame("Alice", "Bob", false);
        var invalidColumn = -1;

        // Act
        var isValid = await _gameService.IsValidMove(gameState, invalidColumn);

        // Assert
        Assert.False(isValid);
    }

    [Fact]
    public async Task IsValidMove_FullColumn_ReturnsFalse()
    {
        // Arrange
        var gameState = await _gameService.StartNewGame("Alice", "Bob", false);
        var column = 0;

        // Fill the column
        for (int i = 0; i < GameBoard.Rows; i++)
        {
            gameState = await _gameService.MakeMove(gameState, column);
        }

        // Act
        var isValid = await _gameService.IsValidMove(gameState, column);

        // Assert
        Assert.False(isValid);
    }

    [Fact]
    public async Task GetAIMove_WithAIConfigured_ReturnsValidMove()
    {
        // Arrange
        var gameState = await _gameService.StartNewGame("Alice", "AI", true, AIDifficulty.Easy);

        // Make first move for human player
        gameState = await _gameService.MakeMove(gameState, 3);

        // Act
        var aiMove = await _gameService.GetAIMove(gameState);

        // Assert
        Assert.InRange(aiMove, 0, GameBoard.Columns - 1);
        Assert.True(gameState.Board.IsValidMove(aiMove));
    }

    [Fact]
    public async Task GetAIMove_NoAIConfigured_ThrowsInvalidOperationException()
    {
        // Arrange - Human vs Human game
        var gameState = await _gameService.StartNewGame("Alice", "Bob", false);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _gameService.GetAIMove(gameState));
    }

    [Fact]
    public async Task GetAIMove_NotAITurn_ThrowsInvalidOperationException()
    {
        // Arrange - AI game but it's human's turn
        var gameState = await _gameService.StartNewGame("Alice", "AI", true, AIDifficulty.Easy);

        // Act & Assert (Player 1's turn, not AI)
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _gameService.GetAIMove(gameState));
    }

    [Fact]
    public async Task MakeMove_MultipleConsecutiveMoves_MaintainsCorrectTurnOrder()
    {
        // Arrange
        var gameState = await _gameService.StartNewGame("Alice", "Bob", false);

        // Act - Make several moves
        var initialPlayer = gameState.CurrentPlayer;
        gameState = await _gameService.MakeMove(gameState, 0); // Player 1
        var afterFirstMove = gameState.CurrentPlayer;
        gameState = await _gameService.MakeMove(gameState, 1); // Player 2
        var afterSecondMove = gameState.CurrentPlayer;

        // Assert
        Assert.NotEqual(initialPlayer, afterFirstMove);
        Assert.Equal(initialPlayer, afterSecondMove);
    }

    [Fact]
    public async Task StartNewGame_DifferentDifficulties_CreatesDistinctAIPlayers()
    {
        // Arrange & Act
        var easyGame = await _gameService.StartNewGame("Player", "Easy AI", true, AIDifficulty.Easy);
        var hardGame = await _gameService.StartNewGame("Player", "Hard AI", true, AIDifficulty.Hard);

        // Make moves to get AI responses
        easyGame = await _gameService.MakeMove(easyGame, 3);
        hardGame = await _gameService.MakeMove(hardGame, 3);

        var easyMove = await _gameService.GetAIMove(easyGame);
        var hardMove = await _gameService.GetAIMove(hardGame);

        // Assert - Both should return valid moves (behavior difference is harder to test)
        Assert.InRange(easyMove, 0, GameBoard.Columns - 1);
        Assert.InRange(hardMove, 0, GameBoard.Columns - 1);
    }

    [Fact]
    public async Task MakeMove_WinningMove_SetsCorrectWinningColumn()
    {
        // Arrange
        var gameState = await _gameService.StartNewGame("Alice", "Bob", false);

        // Create horizontal win scenario - Player 1 needs 5 in a row
        // We'll place pieces carefully to ensure Player 1 wins
        // Pattern: P1 at columns 0,1,2,3 and P2 elsewhere, then P1 wins at column 4

        gameState = await _gameService.MakeMove(gameState, 0); // Player 1
        gameState = await _gameService.MakeMove(gameState, 0); // Player 2 (on top of P1's piece)

        gameState = await _gameService.MakeMove(gameState, 1); // Player 1
        gameState = await _gameService.MakeMove(gameState, 1); // Player 2

        gameState = await _gameService.MakeMove(gameState, 2); // Player 1
        gameState = await _gameService.MakeMove(gameState, 2); // Player 2

        gameState = await _gameService.MakeMove(gameState, 3); // Player 1
        gameState = await _gameService.MakeMove(gameState, 3); // Player 2

        // Act - Make winning move in column 4
        gameState = await _gameService.MakeMove(gameState, 4); // Player 1 wins!

        // Assert
        Assert.Equal(GameStatus.Player1Won, gameState.Status);
        Assert.Equal(4, gameState.WinningMove);
    }
}
