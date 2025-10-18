using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PoConnectFive.Shared.Models;
using Microsoft.Extensions.Logging;
using System.Linq;

namespace PoConnectFive.Shared.Services
{
    public class GameStatisticsService
    {
        private readonly ILogger<GameStatisticsService> _logger;
        private readonly IStorageService _storageService;

        public GameStatisticsService(ILogger<GameStatisticsService> logger, IStorageService storageService)
        {
            _logger = logger;
            _storageService = storageService;
        }

        public async Task<GameStatistics> GetStatisticsAsync()
        {
            var sessions = await _storageService.GetAllAsync<GameSession>("GameSessions");
            var statistics = new GameStatistics();

            if (!sessions.Any())
            {
                return statistics;
            }

            statistics.TotalGames = sessions.Count;
            statistics.AverageGameDuration = TimeSpan.FromTicks(
                (long)sessions.Average(s => s.Duration.Ticks)
            );

            foreach (var session in sessions)
            {
                if (session.Winner != null)
                {
                    if (!statistics.WinDistribution.ContainsKey(session.Winner.Name))
                    {
                        statistics.WinDistribution[session.Winner.Name] = 0;
                    }
                    statistics.WinDistribution[session.Winner.Name]++;

                    if (session.WinningMove.HasValue)
                    {
                        statistics.MostCommonWinningPositions.Add(session.WinningMove.Value);
                    }
                }

                UpdatePlayerStats(statistics, session.Player1, session.Winner);
                UpdatePlayerStats(statistics, session.Player2, session.Winner);
            }

            return statistics;
        }

        private void UpdatePlayerStats(GameStatistics statistics, Player player, Player? winner)
        {
            if (!statistics.PlayerStats.ContainsKey(player.Name))
            {
                statistics.PlayerStats[player.Name] = PlayerStats.CreateNew(player.Id.ToString(), player.Name);
            }

            var stats = statistics.PlayerStats[player.Name];
            stats.GamesPlayed++;
            if (winner?.Id == player.Id)
            {
                stats.Wins++;
            }
        }
    }

    public class GameSession
    {
        public Player Player1 { get; set; } = new Player(1, "Player 1", PlayerType.Human, null);
        public Player Player2 { get; set; } = new Player(2, "Player 2", PlayerType.Human, null);
        public Player? Winner { get; set; }
        public TimeSpan Duration { get; set; }
        public (int row, int col)? WinningMove { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
    }

    public class GameStatistics
    {
        public int TotalGames { get; set; }
        public TimeSpan AverageGameDuration { get; set; }
        public Dictionary<string, int> WinDistribution { get; set; } = new();
        public List<(int row, int col)> MostCommonWinningPositions { get; set; } = new();
        public Dictionary<string, PlayerStats> PlayerStats { get; set; } = new();
    }
}