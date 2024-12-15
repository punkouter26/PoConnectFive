using PoConnectFive.Shared.Interfaces;
using PoConnectFive.Shared.Models;
using PoConnectFive.Shared.Services.AI;
using System;
using System.Threading.Tasks;

namespace PoConnectFive.Shared.Services
{
    /// <summary>
    /// Implements the core game service operations
    /// 
    /// SOLID Principles:
    /// - Single Responsibility: Handles only game state management and move validation
    /// - Open/Closed: New game rules or AI strategies can be added without modifying this class
    /// - Liskov Substitution: Different AI implementations can be used interchangeably
    /// - Interface Segregation: Uses focused interfaces (IGameService, IAIPlayer)
    /// - Dependency Inversion: Depends on abstractions (interfaces) not concrete implementations
    /// 
    /// Design Patterns:
    /// - Strategy Pattern: AI players implement different strategies via IAIPlayer
    /// - Factory Pattern: Uses AIPlayerFactory for creating AI instances
    /// - State Pattern: GameState represents different game states (in progress, won, draw)
    /// - Command Pattern: MakeMove implements game moves as commands
    /// </summary>
    public class GameService : IGameService
    {
        private IAIPlayer? _aiPlayer;

        public Task<GameState> StartNewGame(string player1Name, string player2Name, bool isAIOpponent = false, AIDifficulty? aiDifficulty = null)
        {
            // Factory pattern: Creating player instances
            var player1 = new Player(1, player1Name, PlayerType.Human);
            var player2 = new Player(2, player2Name, isAIOpponent ? PlayerType.AI : PlayerType.Human, aiDifficulty);

            if (isAIOpponent)
            {
                if (!aiDifficulty.HasValue)
                    throw new ArgumentException("AI difficulty must be specified for AI opponents");

                // Factory pattern: Creating AI player instance
                _aiPlayer = AIPlayerFactory.CreateAIPlayer(aiDifficulty.Value);
            }
            else
            {
                _aiPlayer = null;
            }

            // Factory pattern: Creating initial game state
            return Task.FromResult(GameState.CreateNew(player1, player2));
        }

        public Task<GameState> MakeMove(GameState currentState, int column)
        {
            // Command pattern: Move execution
            if (currentState.Status != GameStatus.InProgress)
                throw new InvalidOperationException("Game is already finished");

            if (!currentState.Board.IsValidMove(column))
                throw new InvalidOperationException("Invalid move");

            // State pattern: Immutable state transitions
            var newBoard = currentState.Board.PlacePiece(column, currentState.CurrentPlayer.Id);
            int row = FindPieceRow(newBoard, column, currentState.CurrentPlayer.Id);
            bool isWin = newBoard.CheckWin(row, column, currentState.CurrentPlayer.Id);
            bool isDraw = !isWin && IsBoardFull(newBoard);
            var status = DetermineGameStatus(currentState, isWin, isDraw);

            // State pattern: Creating new game state
            var nextPlayer = status == GameStatus.InProgress ? 
                (currentState.CurrentPlayer == currentState.Player1 ? currentState.Player2 : currentState.Player1) : 
                currentState.CurrentPlayer;

            var newState = new GameState(
                newBoard,
                currentState.Player1,
                currentState.Player2,
                nextPlayer,
                status,
                isWin ? column : null
            );

            return Task.FromResult(newState);
        }

        public Task<bool> IsValidMove(GameState currentState, int column)
        {
            return Task.FromResult(currentState.Board.IsValidMove(column));
        }

        public Task<int> GetAIMove(GameState currentState)
        {
            if (_aiPlayer == null)
                throw new InvalidOperationException("No AI player configured");

            if (currentState.CurrentPlayer.Type != PlayerType.AI)
                throw new InvalidOperationException("Not AI's turn");

            return _aiPlayer.GetNextMove(currentState);
        }

        private int FindPieceRow(GameBoard board, int column, int playerId)
        {
            for (int row = GameBoard.Rows - 1; row >= 0; row--)
            {
                if (board.GetCell(row, column) == playerId)
                    return row;
            }
            throw new InvalidOperationException("Piece not found in column");
        }

        private bool IsBoardFull(GameBoard board)
        {
            // Check if any column is still available
            for (int col = 0; col < GameBoard.Columns; col++)
            {
                if (board.IsValidMove(col))
                    return false;
            }
            return true;
        }

        private GameStatus DetermineGameStatus(GameState currentState, bool isWin, bool isDraw)
        {
            if (isWin)
            {
                return currentState.CurrentPlayer == currentState.Player1 ? 
                    GameStatus.Player1Won : 
                    GameStatus.Player2Won;
            }

            if (isDraw)
            {
                return GameStatus.Draw;
            }

            return GameStatus.InProgress;
        }
    }
}
