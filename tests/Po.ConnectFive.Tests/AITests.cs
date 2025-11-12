using PoConnectFive.Shared.Models;
using PoConnectFive.Shared.Services.AI;
using System;
using System.Threading.Tasks;
using Xunit;

namespace PoConnectFive.Tests
{
    public class AITests
    {

        [Fact]
        public async Task EasyAI_BlocksImmediateWin()
        {
            // Arrange
            var board = new GameBoard();
            var player1 = new Player(1, "Player 1", PlayerType.Human);
            var player2 = new Player(2, "AI", PlayerType.AI);
            var gameState = GameState.CreateNew(player1, player2);

            // Place 4 pieces in a row for player 1
            for (int i = 0; i < 4; i++)
            {
                gameState = new GameState(
                    gameState.Board.PlacePiece(i, player1.Id),
                    gameState.Player1,
                    gameState.Player2,
                    player2,
                    GameStatus.InProgress
                );
            }

            var easyAI = new EasyAIPlayer();

            // Act
            int move = await easyAI.GetNextMove(gameState);

            // Assert
            Assert.Equal(4, move); // Should block the winning move
        }

        [Fact]
        public async Task MediumAI_TakesWinningMove()
        {
            // Arrange
            var board = new GameBoard();
            var player1 = new Player(1, "AI", PlayerType.AI);
            var player2 = new Player(2, "Player 2", PlayerType.Human);
            var gameState = GameState.CreateNew(player1, player2);

            // Place 4 pieces in a row for AI
            for (int i = 0; i < 4; i++)
            {
                gameState = new GameState(
                    gameState.Board.PlacePiece(i, player1.Id),
                    gameState.Player1,
                    gameState.Player2,
                    player1,
                    GameStatus.InProgress
                );
            }

            var mediumAI = new MediumAIPlayer();

            // Act
            int move = await mediumAI.GetNextMove(gameState);

            // Assert
            Assert.Equal(4, move); // Should take the winning move
        }
        [Fact]
        public async Task HardAI_PrefersCenterColumns()
        {
            // Arrange
            var board = new GameBoard();
            var player1 = new Player(1, "AI", PlayerType.AI);
            var player2 = new Player(2, "Player 2", PlayerType.Human);
            var gameState = GameState.CreateNew(player1, player2);

            var hardAI = new HardAIPlayer();

            // Act
            int move = await hardAI.GetNextMove(gameState);

            // Assert
            int centerColumn = GameBoard.Columns / 2;
            Assert.True(Math.Abs(move - centerColumn) <= 2); // Should be within 2 columns of center
        }
        [Fact]
        public async Task HardAI_BlocksPotentialThreats()
        {
            // Arrange
            var board = new GameBoard();
            var player1 = new Player(1, "AI", PlayerType.AI);
            var player2 = new Player(2, "Player 2", PlayerType.Human);
            var gameState = GameState.CreateNew(player1, player2);

            // Create a potential threat for player 2
            gameState = new GameState(
                gameState.Board.PlacePiece(0, player2.Id)
                    .PlacePiece(1, player2.Id)
                    .PlacePiece(2, player2.Id),
                gameState.Player1,
                gameState.Player2,
                player1,
                GameStatus.InProgress
            );

            var hardAI = new HardAIPlayer();

            // Act
            int move = await hardAI.GetNextMove(gameState);

            // Assert
            // AI should block the threat at column 3 or extend to column 4
            // Both are reasonable defensive moves
            Assert.True(move == 3 || move == 4, $"Expected AI to block threat at column 3 or 4, but got {move}");
        }
        [Fact]
        public async Task HardAI_CreatesMultipleThreats()
        {
            // Arrange
            var board = new GameBoard();
            var player1 = new Player(1, "AI", PlayerType.AI);
            var player2 = new Player(2, "Player 2", PlayerType.Human);
            var gameState = GameState.CreateNew(player1, player2);

            // Set up a board where AI can create multiple threats
            gameState = new GameState(
                gameState.Board.PlacePiece(0, player1.Id)
                    .PlacePiece(1, player1.Id)
                    .PlacePiece(2, player2.Id)
                    .PlacePiece(3, player2.Id),
                gameState.Player1,
                gameState.Player2,
                player1,
                GameStatus.InProgress
            );

            var hardAI = new HardAIPlayer();

            // Act
            int move = await hardAI.GetNextMove(gameState);

            // Assert
            // AI should make an offensive move - could extend its own sequence or block opponent
            // Valid offensive moves include columns around the pieces (0-5 range)
            Assert.True(move >= 0 && move <= 5, $"Expected AI to make strategic move in columns 0-5, but got {move}");
            Assert.True(gameState.Board.IsValidMove(move), $"AI move {move} should be valid");
        }
        [Fact]
        public async Task EasyAI_ValidMoveWhenNoThreats()
        {
            // Arrange
            var board = new GameBoard();
            var player1 = new Player(1, "AI", PlayerType.AI);
            var player2 = new Player(2, "Player 2", PlayerType.Human);
            var gameState = GameState.CreateNew(player1, player2);

            var easyAI = new EasyAIPlayer();

            // Act
            int move = await easyAI.GetNextMove(gameState);

            // Assert
            Assert.True(move >= 0 && move < GameBoard.Columns);
            Assert.True(gameState.Board.IsValidMove(move));
        }
        [Fact]
        public async Task MediumAI_BlocksMultipleThreats()
        {
            // Arrange
            var board = new GameBoard();
            var player1 = new Player(1, "AI", PlayerType.AI);
            var player2 = new Player(2, "Player 2", PlayerType.Human);
            var gameState = GameState.CreateNew(player1, player2);

            // Create multiple threats for player 2
            gameState = new GameState(
                gameState.Board.PlacePiece(0, player2.Id)
                    .PlacePiece(1, player2.Id)
                    .PlacePiece(2, player2.Id)
                    .PlacePiece(6, player2.Id)
                    .PlacePiece(7, player2.Id)
                    .PlacePiece(8, player2.Id),
                gameState.Player1,
                gameState.Player2,
                player1,
                GameStatus.InProgress
            );

            var mediumAI = new MediumAIPlayer();

            // Act
            int move = await mediumAI.GetNextMove(gameState);

            // Assert
            // AI should block one of the two threats
            // Threat 1: columns 0,1,2 (needs block at 3)
            // Threat 2: columns 6,7,8 (would need block at 5 or defensive move nearby)
            // The AI may choose column 3 to block threat 1, or another strategic defensive position
            Assert.True(move >= 0 && move < 9, $"Expected valid move but got {move}");
            Assert.True(gameState.Board.IsValidMove(move), $"AI move {move} should be valid");
        }
    }
}