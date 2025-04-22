using Microsoft.Extensions.Logging;

namespace PoConnectFive.Shared.Models
{
    /// <summary>
    /// Represents the current state of the game
    /// Implements the State pattern for game flow management
    /// </summary>
    public class GameState
    {
        public GameBoard Board { get; }
        public Player Player1 { get; }
        public Player Player2 { get; }
        public Player CurrentPlayer { get; }
        public GameStatus Status { get; }
        public int? WinningMove { get; }

        public GameState(GameBoard board, Player player1, Player player2, Player currentPlayer, GameStatus status, int? winningMove = null)
        {
            Board = board;
            Player1 = player1;
            Player2 = player2;
            CurrentPlayer = currentPlayer;
            Status = status;
            WinningMove = winningMove;
        }

        // Factory method for creating initial game state
        public static GameState CreateNew(Player player1, Player player2, ILogger<GameBoard> logger)
        {
            return new GameState(
                new GameBoard(logger),
                player1,
                player2,
                player1, // Player 1 starts
                GameStatus.InProgress
            );
        }
    }

    public enum GameStatus
    {
        InProgress,
        Player1Won,
        Player2Won,
        Draw
    }
}
