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
        private readonly List<GameSession> _gameSessions = new();
        private readonly Dictionary<string, PlayerStats> _playerStats = new();

        public GameStatisticsService(ILogger<GameStatisticsService> logger, IStorageService storageService)
        {
            _logger = logger;
            _storageService = storageService;
        }

        public async Task RecordGameSession(GameSession session)
        {
            _gameSessions.Add(session);
            UpdatePlayerStats(session);
        }

        public async Task<GameStatistics> GetStatistics()
        {
            return new GameStatistics
            {
                TotalGames = _gameSessions.Count,
                AverageGameDuration = CalculateAverageGameDuration(),
                WinDistribution = CalculateWinDistribution(),
                MostCommonWinningPositions = CalculateWinningPositions(),
                PlayerStats = _playerStats
            };
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

        private void UpdatePlayerStats(GameSession session)
        {
            var winner = session.Winner;
            if (winner != null)
            {
                if (!_playerStats.ContainsKey(winner.Name))
                {
                    _playerStats[winner.Name] = PlayerStats.CreateNew(winner.Id.ToString(), winner.Name);
                }
                _playerStats[winner.Name].Wins++;
                _playerStats[winner.Name].GamesPlayed++;
            }

            // Update stats for both players
            foreach (var player in new[] { session.Player1, session.Player2 })
            {
                if (!_playerStats.ContainsKey(player.Name))
                {
                    _playerStats[player.Name] = PlayerStats.CreateNew(player.Id.ToString(), player.Name);
                }
                _playerStats[player.Name].GamesPlayed++;
            }
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

        private TimeSpan CalculateAverageGameDuration()
        {
            if (_gameSessions.Count == 0) return TimeSpan.Zero;

            var totalDuration = TimeSpan.Zero;
            foreach (var session in _gameSessions)
            {
                totalDuration += session.Duration;
            }
            return TimeSpan.FromTicks(totalDuration.Ticks / _gameSessions.Count);
        }

        private Dictionary<string, int> CalculateWinDistribution()
        {
            var distribution = new Dictionary<string, int>();
            foreach (var session in _gameSessions)
            {
                if (session.Winner != null)
                {
                    if (!distribution.ContainsKey(session.Winner.Name))
                    {
                        distribution[session.Winner.Name] = 0;
                    }
                    distribution[session.Winner.Name]++;
                }
            }
            return distribution;
        }

        private List<(int row, int col)> CalculateWinningPositions()
        {
            var positions = new Dictionary<(int row, int col), int>();
            foreach (var session in _gameSessions)
            {
                if (session.WinningMove.HasValue)
                {
                    var pos = (session.WinningMove.Value.row, session.WinningMove.Value.col);
                    if (!positions.ContainsKey(pos))
                    {
                        positions[pos] = 0;
                    }
                    positions[pos]++;
                }
            }
            return positions.OrderByDescending(p => p.Value)
                          .Select(p => (p.Key.row, p.Key.col))
                          .ToList();
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