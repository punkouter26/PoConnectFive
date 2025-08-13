using Microsoft.Extensions.Logging;
using PoConnectFive.Shared.Models;
using PoConnectFive.Shared.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PoConnectFive.Shared.Services
{
    /// <summary>
    /// Service for analyzing game moves and providing strategic insights
    /// Implements advanced game analytics for Connect Five
    /// </summary>
    public class GameAnalyticsService
    {
        private readonly ILogger<GameAnalyticsService> _logger;
        private readonly Dictionary<string, List<int>> _openingBook;
        private readonly Dictionary<TacticalPattern, string> _tacticalPatterns;

        public GameAnalyticsService(ILogger<GameAnalyticsService> logger)
        {
            _logger = logger;
            _openingBook = InitializeOpeningBook();
            _tacticalPatterns = InitializeTacticalPatterns();
        }

        /// <summary>
        /// Analyzes a game move and provides insights
        /// </summary>
        public Task<MoveAnalysis> AnalyzeMove(GameState gameState, int column, int playerId)
        {
            var analysis = new MoveAnalysis
            {
                Column = column,
                PlayerId = playerId,
                MoveNumber = CountMoves(gameState.Board) + 1,
                Timestamp = DateTime.UtcNow
            };

            // Analyze move quality
            analysis.MoveQuality = EvaluateMoveQuality(gameState, column, playerId);

            // Check for tactical patterns
            analysis.TacticalPatterns = IdentifyTacticalPatterns(gameState, column, playerId);

            // Generate alternative suggestions
            analysis.AlternativeMoves = GenerateAlternativeMoves(gameState, playerId);

            // Evaluate opening principles
            if (analysis.MoveNumber <= 10)
            {
                analysis.OpeningEvaluation = EvaluateOpening(gameState, analysis.MoveNumber);
            }

            // Calculate position strength
            analysis.PositionStrength = CalculatePositionStrength(gameState, playerId);

            _logger.LogInformation("Analyzed move {Column} for player {PlayerId}: Quality={Quality}, Patterns={PatternCount}",
                column, playerId, analysis.MoveQuality, analysis.TacticalPatterns.Count);

            return Task.FromResult(analysis);
        }

        /// <summary>
        /// Provides comprehensive game analysis after completion
        /// </summary>
        public Task<GameAnalysisReport> AnalyzeGame(List<MoveAnalysis> moves, GameState finalState)
        {
            var report = new GameAnalysisReport
            {
                GameId = Guid.NewGuid().ToString(),
                TotalMoves = moves.Count,
                GameDuration = moves.LastOrDefault()?.Timestamp - moves.FirstOrDefault()?.Timestamp ?? TimeSpan.Zero,
                Winner = finalState.Status == GameStatus.Player1Won ? 1 :
                        finalState.Status == GameStatus.Player2Won ? 2 : (int?)null
            };

            // Analyze player performance
            report.Player1Analysis = AnalyzePlayerPerformance(moves.Where(m => m.PlayerId == 1).ToList());
            report.Player2Analysis = AnalyzePlayerPerformance(moves.Where(m => m.PlayerId == 2).ToList());

            // Identify key moments
            report.KeyMoments = IdentifyKeyMoments(moves);

            // Opening analysis
            report.OpeningName = IdentifyOpening(moves.Take(6).ToList());
            report.OpeningEvaluation = EvaluateOpeningPerformance(moves.Take(10).ToList());

            // Endgame analysis if applicable
            if (moves.Count > 20)
            {
                report.EndgameAnalysis = AnalyzeEndgame(moves.Skip(Math.Max(0, moves.Count - 10)).ToList(), finalState);
            }

            return Task.FromResult(report);
        }

        /// <summary>
        /// Identifies playing style patterns
        /// </summary>
        public Task<PlayingStyleAnalysis> AnalyzePlayingStyle(List<MoveAnalysis> recentMoves)
        {
            var styleAnalysis = new PlayingStyleAnalysis();

            if (!recentMoves.Any())
            {
                return Task.FromResult(styleAnalysis);
            }

            // Analyze aggression level
            var aggressiveMoves = recentMoves.Count(m => m.TacticalPatterns.Contains(TacticalPattern.Attack) ||
                                                        m.TacticalPatterns.Contains(TacticalPattern.DoubleAttack));
            styleAnalysis.AggressionLevel = (double)aggressiveMoves / recentMoves.Count;

            // Analyze defensive tendencies
            var defensiveMoves = recentMoves.Count(m => m.TacticalPatterns.Contains(TacticalPattern.Block) ||
                                                       m.TacticalPatterns.Contains(TacticalPattern.MultipleBlocks));
            styleAnalysis.DefensiveLevel = (double)defensiveMoves / recentMoves.Count;

            // Analyze center control preference
            var centerMoves = recentMoves.Count(m => Math.Abs(m.Column - GameBoard.Columns / 2) <= 1);
            styleAnalysis.CenterControlPreference = (double)centerMoves / recentMoves.Count;

            // Analyze move quality consistency
            var averageQuality = recentMoves.Average(m => (int)m.MoveQuality);
            styleAnalysis.ConsistencyRating = CalculateConsistency(recentMoves.Select(m => (int)m.MoveQuality).ToList());

            // Determine primary style
            styleAnalysis.PrimaryStyle = DeterminePrimaryStyle(styleAnalysis);

            return Task.FromResult(styleAnalysis);
        }

        /// <summary>
        /// Generates improvement recommendations
        /// </summary>
        public Task<List<ImprovementRecommendation>> GenerateRecommendations(PlayingStyleAnalysis styleAnalysis, List<MoveAnalysis> recentMoves)
        {
            var recommendations = new List<ImprovementRecommendation>();

            // Analyze weaknesses and suggest improvements
            if (styleAnalysis.DefensiveLevel < 0.3)
            {
                recommendations.Add(new ImprovementRecommendation
                {
                    Category = RecommendationCategory.Defense,
                    Priority = RecommendationPriority.High,
                    Title = "Improve Defensive Awareness",
                    Description = "Focus more on blocking opponent threats. Look for potential winning lines your opponent might be building.",
                    SpecificAdvice = "Before making an attacking move, always check if your opponent has any immediate threats that need blocking."
                });
            }

            if (styleAnalysis.CenterControlPreference < 0.4)
            {
                recommendations.Add(new ImprovementRecommendation
                {
                    Category = RecommendationCategory.Strategy,
                    Priority = RecommendationPriority.Medium,
                    Title = "Focus on Center Control",
                    Description = "Playing in the center columns gives you more opportunities to create winning lines in multiple directions.",
                    SpecificAdvice = "Try to place your first few pieces in the middle columns (3-5) when possible."
                });
            }

            if (styleAnalysis.ConsistencyRating < 0.6)
            {
                recommendations.Add(new ImprovementRecommendation
                {
                    Category = RecommendationCategory.Tactics,
                    Priority = RecommendationPriority.High,
                    Title = "Improve Move Consistency",
                    Description = "Your move quality varies significantly. Focus on analyzing each position more carefully.",
                    SpecificAdvice = "Take more time to consider all options before making a move. Look for immediate threats and opportunities."
                });
            }

            return Task.FromResult(recommendations);
        }

        #region Private Helper Methods

        private Dictionary<string, List<int>> InitializeOpeningBook()
        {
            return new Dictionary<string, List<int>>
            {
                ["Center Game"] = new List<int> { 4, 4, 3, 5 },
                ["Side Attack"] = new List<int> { 4, 2, 6, 1 },
                ["Balanced Opening"] = new List<int> { 4, 3, 5, 4 },
                ["Aggressive Opening"] = new List<int> { 4, 4, 4, 3 },
                ["Defensive Setup"] = new List<int> { 4, 1, 7, 2 }
            };
        }

        private Dictionary<TacticalPattern, string> InitializeTacticalPatterns()
        {
            return new Dictionary<TacticalPattern, string>
            {
                [TacticalPattern.Attack] = "Creating an immediate threat",
                [TacticalPattern.Block] = "Blocking opponent's threat",
                [TacticalPattern.DoubleAttack] = "Creating multiple threats simultaneously",
                [TacticalPattern.MultipleBlocks] = "Blocking multiple threats",
                [TacticalPattern.Fork] = "Creating a position with multiple winning options",
                [TacticalPattern.Sacrifice] = "Sacrificing material for positional advantage",
                [TacticalPattern.Tempo] = "Gaining initiative through forcing moves",
                [TacticalPattern.CenterControl] = "Establishing dominance in center columns"
            };
        }

        private MoveQuality EvaluateMoveQuality(GameState gameState, int column, int playerId)
        {
            // Simulate the move and evaluate its impact
            var newBoard = gameState.Board.PlacePiece(column, playerId);
            var targetRow = gameState.Board.GetTargetRow(column);

            // Check if it's a winning move
            if (newBoard.CheckWin(targetRow, column, playerId))
                return MoveQuality.Excellent;

            // Check if it blocks an immediate opponent win
            var opponentId = playerId == 1 ? 2 : 1;
            var opponentBoard = gameState.Board.PlacePiece(column, opponentId);
            if (opponentBoard.CheckWin(targetRow, column, opponentId))
                return MoveQuality.Good;

            // Evaluate based on strategic value
            var strategicValue = EvaluateStrategicValue(gameState, column, playerId);

            return strategicValue switch
            {
                > 80 => MoveQuality.Excellent,
                > 60 => MoveQuality.Good,
                > 40 => MoveQuality.Average,
                > 20 => MoveQuality.Poor,
                _ => MoveQuality.Blunder
            };
        }

        private List<TacticalPattern> IdentifyTacticalPatterns(GameState gameState, int column, int playerId)
        {
            var patterns = new List<TacticalPattern>();
            var newBoard = gameState.Board.PlacePiece(column, playerId);
            var targetRow = gameState.Board.GetTargetRow(column);

            // Check for immediate win (attack)
            if (newBoard.CheckWin(targetRow, column, playerId))
            {
                patterns.Add(TacticalPattern.Attack);
            }

            // Check for block
            var opponentId = playerId == 1 ? 2 : 1;
            var opponentBoard = gameState.Board.PlacePiece(column, opponentId);
            if (opponentBoard.CheckWin(targetRow, column, opponentId))
            {
                patterns.Add(TacticalPattern.Block);
            }

            // Check for center control
            if (Math.Abs(column - GameBoard.Columns / 2) <= 1)
            {
                patterns.Add(TacticalPattern.CenterControl);
            }

            // Check for double attack (creating multiple threats)
            if (CountThreats(newBoard, playerId) > CountThreats(gameState.Board, playerId) + 1)
            {
                patterns.Add(TacticalPattern.DoubleAttack);
            }

            return patterns;
        }

        private List<AlternativeMove> GenerateAlternativeMoves(GameState gameState, int playerId)
        {
            var alternatives = new List<AlternativeMove>();

            for (int col = 0; col < GameBoard.Columns; col++)
            {
                if (gameState.Board.IsValidMove(col))
                {
                    var evaluation = EvaluateStrategicValue(gameState, col, playerId);
                    var quality = evaluation switch
                    {
                        > 80 => MoveQuality.Excellent,
                        > 60 => MoveQuality.Good,
                        > 40 => MoveQuality.Average,
                        > 20 => MoveQuality.Poor,
                        _ => MoveQuality.Blunder
                    };

                    alternatives.Add(new AlternativeMove
                    {
                        Column = col,
                        Quality = quality,
                        Evaluation = evaluation,
                        Reasoning = GenerateReasoningForMove(gameState, col, playerId)
                    });
                }
            }

            return alternatives.OrderByDescending(a => a.Evaluation).Take(5).ToList();
        }

        private string GenerateReasoningForMove(GameState gameState, int column, int playerId)
        {
            var newBoard = gameState.Board.PlacePiece(column, playerId);
            var targetRow = gameState.Board.GetTargetRow(column);

            // Check for immediate win
            if (newBoard.CheckWin(targetRow, column, playerId))
                return "Winning move - creates five in a row";

            // Check for block
            var opponentId = playerId == 1 ? 2 : 1;
            var opponentBoard = gameState.Board.PlacePiece(column, opponentId);
            if (opponentBoard.CheckWin(targetRow, column, opponentId))
                return "Defensive move - blocks opponent's winning threat";

            // Check for center control
            if (Math.Abs(column - GameBoard.Columns / 2) <= 1)
                return "Strategic move - controls center position";

            // Check for creating threats
            var threatsCreated = CountThreats(newBoard, playerId) - CountThreats(gameState.Board, playerId);
            if (threatsCreated > 0)
                return $"Offensive move - creates {threatsCreated} new threat(s)";

            return "Positional move - maintains board presence";
        }

        private OpeningEvaluation EvaluateOpening(GameState gameState, int moveNumber)
        {
            var moves = GetMovesFromBoard(gameState.Board);
            var openingName = IdentifyOpening(moves);

            return new OpeningEvaluation
            {
                OpeningName = openingName,
                Evaluation = CalculateOpeningScore(moves),
                Principles = EvaluateOpeningPrinciples(moves)
            };
        }

        private double CalculatePositionStrength(GameState gameState, int playerId)
        {
            var threats = CountThreats(gameState.Board, playerId);
            var opponentThreats = CountThreats(gameState.Board, playerId == 1 ? 2 : 1);
            var centerControl = CountCenterPieces(gameState.Board, playerId);

            return (threats * 2.0 - opponentThreats * 1.5 + centerControl * 0.5) / 10.0;
        }

        private int CountMoves(GameBoard board)
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

        private int EvaluateStrategicValue(GameState gameState, int column, int playerId)
        {
            var value = 50; // Base value

            // Center preference
            var distanceFromCenter = Math.Abs(column - GameBoard.Columns / 2);
            value += (4 - distanceFromCenter) * 5;

            // Threat creation/blocking
            var newBoard = gameState.Board.PlacePiece(column, playerId);
            var threatsCreated = CountThreats(newBoard, playerId) - CountThreats(gameState.Board, playerId);
            value += threatsCreated * 20;

            // Check if it blocks opponent threats
            var opponentId = playerId == 1 ? 2 : 1;
            var opponentBoard = gameState.Board.PlacePiece(column, opponentId);
            var targetRow = gameState.Board.GetTargetRow(column);
            if (opponentBoard.CheckWin(targetRow, column, opponentId))
                value += 30;

            return Math.Max(0, Math.Min(100, value));
        }

        private int CountThreats(GameBoard board, int playerId)
        {
            int threats = 0;
            // Count sequences of 3 or 4 pieces that could become 5
            // This is a simplified implementation
            for (int row = 0; row < GameBoard.Rows; row++)
            {
                for (int col = 0; col < GameBoard.Columns; col++)
                {
                    if (board.GetCell(row, col) == playerId)
                    {
                        threats += CountSequencesFromPosition(board, row, col, playerId);
                    }
                }
            }
            return threats;
        }

        private int CountSequencesFromPosition(GameBoard board, int row, int col, int playerId)
        {
            int sequences = 0;
            int[][] directions = { new[] { 0, 1 }, new[] { 1, 0 }, new[] { 1, 1 }, new[] { 1, -1 } };

            foreach (var dir in directions)
            {
                int count = 1;
                int emptySpaces = 0;

                // Check in positive direction
                for (int i = 1; i < 5; i++)
                {
                    int newRow = row + i * dir[0];
                    int newCol = col + i * dir[1];

                    if (newRow < 0 || newRow >= GameBoard.Rows || newCol < 0 || newCol >= GameBoard.Columns)
                        break;

                    int cell = board.GetCell(newRow, newCol);
                    if (cell == playerId)
                        count++;
                    else if (cell == 0)
                        emptySpaces++;
                    else
                        break;
                }

                // Check in negative direction
                for (int i = 1; i < 5; i++)
                {
                    int newRow = row - i * dir[0];
                    int newCol = col - i * dir[1];

                    if (newRow < 0 || newRow >= GameBoard.Rows || newCol < 0 || newCol >= GameBoard.Columns)
                        break;

                    int cell = board.GetCell(newRow, newCol);
                    if (cell == playerId)
                        count++;
                    else if (cell == 0)
                        emptySpaces++;
                    else
                        break;
                }

                if (count >= 3 && count + emptySpaces >= 5)
                    sequences++;
            }

            return sequences;
        }

        private int CountCenterPieces(GameBoard board, int playerId)
        {
            int count = 0;
            int centerStart = GameBoard.Columns / 2 - 1;
            int centerEnd = GameBoard.Columns / 2 + 1;

            for (int row = 0; row < GameBoard.Rows; row++)
            {
                for (int col = centerStart; col <= centerEnd; col++)
                {
                    if (col >= 0 && col < GameBoard.Columns && board.GetCell(row, col) == playerId)
                        count++;
                }
            }
            return count;
        }

        private List<MoveAnalysis> GetMovesFromBoard(GameBoard board)
        {
            // Simplified: In a real implementation, you'd track moves throughout the game
            return new List<MoveAnalysis>();
        }

        private string IdentifyOpening(List<MoveAnalysis> moves)
        {
            if (moves.Count < 2) return "Early Game";

            var firstMoves = moves.Take(4).Select(m => m.Column).ToList();

            foreach (var opening in _openingBook)
            {
                if (firstMoves.Take(opening.Value.Count).SequenceEqual(opening.Value.Take(firstMoves.Count)))
                {
                    return opening.Key;
                }
            }

            return "Unknown Opening";
        }

        private OpeningEvaluation EvaluateOpeningPerformance(List<MoveAnalysis> moves)
        {
            if (!moves.Any())
            {
                return new OpeningEvaluation
                {
                    OpeningName = "Early Game",
                    Evaluation = 50,
                    Principles = new List<string> { "Game just started" }
                };
            }

            var score = 50;

            // Evaluate center control
            var centerMoves = moves.Take(6).Count(m => Math.Abs(m.Column - GameBoard.Columns / 2) <= 1);
            score += centerMoves * 8;

            // Evaluate move quality in opening (simplified - assume average quality)
            score += 10; // Placeholder since MoveAnalysis.Quality is not available yet

            // Penalty for very edge moves early
            var edgeMoves = moves.Take(4).Count(m => m.Column == 0 || m.Column == GameBoard.Columns - 1);
            score -= edgeMoves * 15;

            var finalScore = Math.Max(0, Math.Min(100, score));

            var principles = EvaluateOpeningPrinciples(moves);

            return new OpeningEvaluation
            {
                OpeningName = IdentifyOpening(moves),
                Evaluation = finalScore,
                Principles = principles
            };
        }

        private int CalculateOpeningScore(List<MoveAnalysis> moves)
        {
            if (!moves.Any()) return 50;

            var score = 50;

            // Prefer center control in opening
            var centerMoves = moves.Take(6).Count(m => Math.Abs(m.Column - GameBoard.Columns / 2) <= 1);
            score += centerMoves * 10;

            return Math.Max(0, Math.Min(100, score));
        }

        private List<string> EvaluateOpeningPrinciples(List<MoveAnalysis> moves)
        {
            var principles = new List<string>();

            if (moves.Any() && Math.Abs(moves.First().Column - GameBoard.Columns / 2) <= 1)
                principles.Add("✓ Started in center");
            else
                principles.Add("✗ Should start in center");

            var earlyMoves = moves.Take(3);
            if (earlyMoves.Count(m => Math.Abs(m.Column - GameBoard.Columns / 2) <= 2) >= 2)
                principles.Add("✓ Maintained center control");
            else
                principles.Add("✗ Lost center control early");

            return principles;
        }

        private PlayerPerformanceAnalysis AnalyzePlayerPerformance(List<MoveAnalysis> playerMoves)
        {
            if (!playerMoves.Any())
                return new PlayerPerformanceAnalysis();

            return new PlayerPerformanceAnalysis
            {
                AverageMoveQuality = playerMoves.Average(m => (int)m.MoveQuality),
                TotalTacticalPatterns = playerMoves.SelectMany(m => m.TacticalPatterns).Count(),
                BlunderCount = playerMoves.Count(m => m.MoveQuality == MoveQuality.Blunder),
                ExcellentMoveCount = playerMoves.Count(m => m.MoveQuality == MoveQuality.Excellent),
                AverageThinkingTime = TimeSpan.FromSeconds(30) // Simplified
            };
        }

        private List<KeyMoment> IdentifyKeyMoments(List<MoveAnalysis> moves)
        {
            var keyMoments = new List<KeyMoment>();

            for (int i = 0; i < moves.Count; i++)
            {
                var move = moves[i];

                // Identify blunders
                if (move.MoveQuality == MoveQuality.Blunder)
                {
                    keyMoments.Add(new KeyMoment
                    {
                        MoveNumber = i + 1,
                        Type = KeyMomentType.Blunder,
                        Description = $"Player {move.PlayerId} made a critical error",
                        Impact = "Major advantage lost"
                    });
                }

                // Identify excellent moves
                if (move.MoveQuality == MoveQuality.Excellent)
                {
                    keyMoments.Add(new KeyMoment
                    {
                        MoveNumber = i + 1,
                        Type = KeyMomentType.BrilliantMove,
                        Description = $"Player {move.PlayerId} found the best move",
                        Impact = "Gained significant advantage"
                    });
                }
            }

            return keyMoments;
        }

        private EndgameAnalysis AnalyzeEndgame(List<MoveAnalysis> endgameMoves, GameState finalState)
        {
            return new EndgameAnalysis
            {
                AccuracyRating = endgameMoves.Average(m => (int)m.MoveQuality) / 5.0,
                CriticalMoments = endgameMoves.Count(m => m.TacticalPatterns.Any()),
                FinalEvaluation = finalState.Status switch
                {
                    GameStatus.Player1Won => "Player 1 successfully converted advantage",
                    GameStatus.Player2Won => "Player 2 successfully converted advantage",
                    GameStatus.Draw => "Game ended in a draw",
                    _ => "Game incomplete"
                }
            };
        }

        private double CalculateConsistency(List<int> values)
        {
            if (values.Count < 2) return 1.0;

            var average = values.Average();
            var variance = values.Select(v => Math.Pow(v - average, 2)).Average();
            var standardDeviation = Math.Sqrt(variance);

            // Convert to 0-1 scale where lower deviation = higher consistency
            return Math.Max(0, 1.0 - (standardDeviation / average));
        }

        private PlayingStyle DeterminePrimaryStyle(PlayingStyleAnalysis analysis)
        {
            if (analysis.AggressionLevel > 0.6)
                return PlayingStyle.Aggressive;
            else if (analysis.DefensiveLevel > 0.6)
                return PlayingStyle.Defensive;
            else if (analysis.CenterControlPreference > 0.7)
                return PlayingStyle.Positional;
            else
                return PlayingStyle.Balanced;
        }

        #endregion
    }

    #region Data Models

    public class MoveAnalysis
    {
        public int Column { get; set; }
        public int PlayerId { get; set; }
        public int MoveNumber { get; set; }
        public DateTime Timestamp { get; set; }
        public MoveQuality MoveQuality { get; set; }
        public List<TacticalPattern> TacticalPatterns { get; set; } = new();
        public List<AlternativeMove> AlternativeMoves { get; set; } = new();
        public OpeningEvaluation? OpeningEvaluation { get; set; }
        public double PositionStrength { get; set; }
    }

    public class AlternativeMove
    {
        public int Column { get; set; }
        public MoveQuality Quality { get; set; }
        public int Evaluation { get; set; }
        public string Reasoning { get; set; } = string.Empty;
    }

    public class OpeningEvaluation
    {
        public string OpeningName { get; set; } = string.Empty;
        public int Evaluation { get; set; }
        public List<string> Principles { get; set; } = new();
    }

    public class GameAnalysisReport
    {
        public string GameId { get; set; } = string.Empty;
        public int TotalMoves { get; set; }
        public TimeSpan GameDuration { get; set; }
        public int? Winner { get; set; }
        public PlayerPerformanceAnalysis Player1Analysis { get; set; } = new();
        public PlayerPerformanceAnalysis Player2Analysis { get; set; } = new();
        public List<KeyMoment> KeyMoments { get; set; } = new();
        public string OpeningName { get; set; } = string.Empty;
        public OpeningEvaluation OpeningEvaluation { get; set; } = new();
        public EndgameAnalysis? EndgameAnalysis { get; set; }
    }

    public class PlayerPerformanceAnalysis
    {
        public double AverageMoveQuality { get; set; }
        public int TotalTacticalPatterns { get; set; }
        public int BlunderCount { get; set; }
        public int ExcellentMoveCount { get; set; }
        public TimeSpan AverageThinkingTime { get; set; }
    }

    public class KeyMoment
    {
        public int MoveNumber { get; set; }
        public KeyMomentType Type { get; set; }
        public string Description { get; set; } = string.Empty;
        public string Impact { get; set; } = string.Empty;
    }

    public class EndgameAnalysis
    {
        public double AccuracyRating { get; set; }
        public int CriticalMoments { get; set; }
        public string FinalEvaluation { get; set; } = string.Empty;
    }

    public class PlayingStyleAnalysis
    {
        public double AggressionLevel { get; set; }
        public double DefensiveLevel { get; set; }
        public double CenterControlPreference { get; set; }
        public double ConsistencyRating { get; set; }
        public PlayingStyle PrimaryStyle { get; set; }
    }

    public class ImprovementRecommendation
    {
        public RecommendationCategory Category { get; set; }
        public RecommendationPriority Priority { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string SpecificAdvice { get; set; } = string.Empty;
    }

    public enum TacticalPattern
    {
        Attack,
        Block,
        DoubleAttack,
        MultipleBlocks,
        Fork,
        Sacrifice,
        Tempo,
        CenterControl
    }

    public enum KeyMomentType
    {
        Blunder,
        BrilliantMove,
        TurningPoint,
        MissedOpportunity
    }

    public enum PlayingStyle
    {
        Aggressive,
        Defensive,
        Positional,
        Balanced
    }

    public enum RecommendationCategory
    {
        Strategy,
        Tactics,
        Defense,
        Opening,
        Endgame
    }

    public enum RecommendationPriority
    {
        Low,
        Medium,
        High,
        Critical
    }

    #endregion
}
