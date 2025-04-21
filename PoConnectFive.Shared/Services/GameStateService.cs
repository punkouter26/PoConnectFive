using PoConnectFive.Shared.Models;

namespace PoConnectFive.Shared.Services
{
    public class GameStateService
    {
        public GameBoard Board { get; private set; }
        public Player CurrentPlayer { get; private set; }
        public GameStatus Status { get; private set; }
        public Player Player1 { get; private set; }
        public Player Player2 { get; private set; }

        public GameStateService()
        {
            Board = new GameBoard();
            Player1 = new Player(1, "Player 1", PlayerType.Human);
            Player2 = new Player(2, "Player 2", PlayerType.Human);
            CurrentPlayer = Player1;
            Status = GameStatus.InProgress;
        }

        public bool MakeMove(int column)
        {
            if (Status != GameStatus.InProgress || !Board.IsValidMove(column))
                return false;

            int row = Board.GetTargetRow(column);
            Board = Board.PlacePiece(column, CurrentPlayer.Id);

            // Check for win
            if (Board.CheckWin(row, column, CurrentPlayer.Id))
            {
                Status = CurrentPlayer == Player1 ? GameStatus.Player1Won : GameStatus.Player2Won;
                return true;
            }

            // Check for draw
            if (!Board.HasValidMoves())
            {
                Status = GameStatus.Draw;
                return true;
            }

            // Switch players
            CurrentPlayer = CurrentPlayer == Player1 ? Player2 : Player1;
            return true;
        }

        public GameState GetGameState()
        {
            return new GameState(Board, Player1, Player2, CurrentPlayer, Status);
        }

        public void Reset()
        {
            Board = new GameBoard();
            CurrentPlayer = Player1;
            Status = GameStatus.InProgress;
        }
    }
} 