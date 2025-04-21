using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PoConnectFive.Shared.Models;
using Microsoft.Extensions.Logging;

namespace PoConnectFive.Shared.Services
{
    public class GameStatisticsService
    {
        private readonly ILogger<GameStatisticsService> _logger;
        private readonly List<GameRecord> _gameHistory;

        public GameStatisticsService(ILogger<GameStatisticsService> logger)
        {
            _logger = logger;
            _gameHistory = new List<GameRecord>();
        }

        public void RecordGame(GameState finalState, TimeSpan duration, string player1Name, string player2Name, AIDifficulty? aiDifficulty)
        {
            var record = new GameRecord
            {
                GameId = Guid.NewGuid(),
                Timestamp = DateTime.UtcNow,
                Duration = duration,
                Player1Name = player1Name,
                Player2Name = player2Name,
                Winner = finalState.Status switch
                {
                    GameStatus.Player1Won => player1Name,
                    GameStatus.Player2Won => player2Name,
                    _ => "Draw"
                },
                AIDifficulty = aiDifficulty,
                MoveCount = CalculateMoveCount(finalState.Board),
                WinningPattern = GetWinningPattern(finalState)
            };

            _gameHistory.Add(record);
            _logger.LogInformation("Recorded game statistics: {GameId}", record.GameId);
        }

        public GameStatistics GetStatistics()
        {
            if (_gameHistory.Count == 0)
                return new GameStatistics();

            return new GameStatistics
            {
                TotalGames = _gameHistory.Count,
                AverageGameDuration = TimeSpan.FromTicks((long)_gameHistory.Average(g => g.Duration.Ticks)),
                WinRates = CalculateWinRates(),
                AverageMovesPerGame = _gameHistory.Average(g => g.MoveCount),
                MostCommonWinningPatterns = GetMostCommonWinningPatterns(),
                RecentGames = _gameHistory.OrderByDescending(g => g.Timestamp).Take(10).ToList()
            };
        }

        private Dictionary<string, double> CalculateWinRates()
        {
            var winRates = new Dictionary<string, double>();
            var totalGames = _gameHistory.Count;

            foreach (var game in _gameHistory)
            {
                if (game.Winner != "Draw")
                {
                    if (!winRates.ContainsKey(game.Winner))
                        winRates[game.Winner] = 0;
                    winRates[game.Winner]++;
                }
            }

            foreach (var player in winRates.Keys.ToList())
            {
                winRates[player] = (winRates[player] / totalGames) * 100;
            }

            return winRates;
        }

        private List<WinningPattern> GetMostCommonWinningPatterns()
        {
            return _gameHistory
                .Where(g => g.WinningPattern != null)
                .GroupBy(g => g.WinningPattern)
                .Select(g => new WinningPattern
                {
                    Pattern = g.Key,
                    Count = g.Count(),
                    Percentage = (double)g.Count() / _gameHistory.Count * 100
                })
                .OrderByDescending(p => p.Count)
                .Take(5)
                .ToList();
        }

        private int CalculateMoveCount(GameBoard board)
        {
            int count = 0;
            for (int row = 0; row < GameBoard.Rows; row++)
            {
                for (int col = 0; col < GameBoard.Columns; col++)
                {
                    if (board.GetCell(row, col) != 0)
                        count++;
                }
            }
            return count;
        }

        private string? GetWinningPattern(GameState finalState)
        {
            if (finalState.WinningMove == null)
                return null;

            // This is a simplified version - in reality, you'd want to analyze the board
            // to determine the exact winning pattern (horizontal, vertical, diagonal)
            return "Pattern"; // TODO: Implement actual pattern detection
        }
    }

    public class GameRecord
    {
        public Guid GameId { get; set; }
        public DateTime Timestamp { get; set; }
        public TimeSpan Duration { get; set; }
        public string Player1Name { get; set; } = string.Empty;
        public string Player2Name { get; set; } = string.Empty;
        public string Winner { get; set; } = string.Empty;
        public AIDifficulty? AIDifficulty { get; set; }
        public int MoveCount { get; set; }
        public string? WinningPattern { get; set; }
    }

    public class GameStatistics
    {
        public int TotalGames { get; set; }
        public TimeSpan AverageGameDuration { get; set; }
        public Dictionary<string, double> WinRates { get; set; } = new();
        public double AverageMovesPerGame { get; set; }
        public List<WinningPattern> MostCommonWinningPatterns { get; set; } = new();
        public List<GameRecord> RecentGames { get; set; } = new();
    }

    public class WinningPattern
    {
        public string Pattern { get; set; } = string.Empty;
        public int Count { get; set; }
        public double Percentage { get; set; }
    }
} 