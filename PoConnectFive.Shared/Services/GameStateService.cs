using Microsoft.Extensions.Logging;
using PoConnectFive.Shared.Models;
using Serilog;

namespace PoConnectFive.Shared.Services
{
    public class GameStateService
    {
        private readonly ILogger<GameStateService> _logger;
        private readonly object _lock = new();
        private GameState _currentState;

        public GameStateService(ILogger<GameStateService> logger)
        {
            _logger = logger;
            _currentState = CreateNewGame();
            _logger.LogInformation("GameStateService initialized with new game");
        }

        public GameState CreateNewGame()
        {
            var board = new GameBoard();
            var player1 = Player.Red;
            var player2 = Player.Yellow;

            _logger.LogInformation("Creating new game with players: {Player1} vs {Player2}", player1.Name, player2.Name);

            return new GameState(
                board,
                player1,
                player2,
                player1, // Player 1 starts
                GameStatus.InProgress
            );
        }

        public GameState GetCurrentState() => _currentState;

        public GameState MakeMove(int column)
        {
            lock (_lock)
            {
                if (_currentState.Status != GameStatus.InProgress)
                {
                    _logger.LogWarning("Attempted move in completed game. Status: {Status}", _currentState.Status);
                    throw new InvalidOperationException("Game is already over");
                }

                if (!_currentState.Board.IsValidMove(column))
                {
                    _logger.LogWarning("Invalid move attempted in column {Column}", column);
                    throw new InvalidOperationException("Invalid move");
                }

                _logger.LogInformation("Player {PlayerId} making move in column {Column}", _currentState.CurrentPlayer.Id, column);

                var newBoard = _currentState.Board.PlacePiece(column, _currentState.CurrentPlayer.Id);
                var targetRow = _currentState.Board.GetTargetRow(column);

                var hasWon = newBoard.CheckWin(targetRow, column, _currentState.CurrentPlayer.Id);
                var nextPlayer = _currentState.CurrentPlayer.Id == 1 ? _currentState.Player2 : _currentState.Player1;
                var status = hasWon ?
                    (_currentState.CurrentPlayer.Id == 1 ? GameStatus.Player1Won : GameStatus.Player2Won) :
                    (!newBoard.HasValidMoves() ? GameStatus.Draw : GameStatus.InProgress);

                _logger.LogInformation("Move result - Win: {HasWon}, Next Player: {NextPlayer}, Status: {Status}",
                    hasWon, nextPlayer.Name, status);

                _currentState = new GameState(
                    newBoard,
                    _currentState.Player1,
                    _currentState.Player2,
                    nextPlayer,
                    status,
                    hasWon ? column : null
                );

                return _currentState;
            }
        }

        public void Reset()
        {
            _logger.LogInformation("Resetting game state");
            _currentState = CreateNewGame();
        }
    }
}
