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
            var player1 = new Player { Id = 1, Name = "Player 1", Type = PlayerType.Human };
            var player2 = new Player { Id = 2, Name = "AI", Type = PlayerType.AI };
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
            var player1 = new Player { Id = 1, Name = "AI", Type = PlayerType.AI };
            var player2 = new Player { Id = 2, Name = "Player 2", Type = PlayerType.Human };
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
            var player1 = new Player { Id = 1, Name = "AI", Type = PlayerType.AI };
            var player2 = new Player { Id = 2, Name = "Player 2", Type = PlayerType.Human };
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
            var player1 = new Player { Id = 1, Name = "AI", Type = PlayerType.AI };
            var player2 = new Player { Id = 2, Name = "Player 2", Type = PlayerType.Human };
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
            Assert.Equal(3, move); // Should block the potential threat
        }

        [Fact]
        public async Task HardAI_CreatesMultipleThreats()
        {
            // Arrange
            var board = new GameBoard();
            var player1 = new Player { Id = 1, Name = "AI", Type = PlayerType.AI };
            var player2 = new Player { Id = 2, Name = "Player 2", Type = PlayerType.Human };
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
            Assert.True(move == 4 || move == 5); // Should create a new threat
        }

        [Fact]
        public async Task EasyAI_ValidMoveWhenNoThreats()
        {
            // Arrange
            var board = new GameBoard();
            var player1 = new Player { Id = 1, Name = "AI", Type = PlayerType.AI };
            var player2 = new Player { Id = 2, Name = "Player 2", Type = PlayerType.Human };
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
            var player1 = new Player { Id = 1, Name = "AI", Type = PlayerType.AI };
            var player2 = new Player { Id = 2, Name = "Player 2", Type = PlayerType.Human };
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
            Assert.True(move == 3 || move == 9); // Should block one of the threats
        }
    }
} 