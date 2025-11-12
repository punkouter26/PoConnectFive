using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Mvc;
using PoConnectFive.Server.Extensions;
using PoConnectFive.Server.Services;
using PoConnectFive.Shared.Models;

namespace PoConnectFive.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LeaderboardController : ControllerBase
{
    private readonly ITableStorageService _tableStorageService;
    private readonly ILogger<LeaderboardController> _logger;
    private readonly TelemetryClient _telemetryClient;

    public LeaderboardController(
        ITableStorageService tableStorageService,
        ILogger<LeaderboardController> logger,
        TelemetryClient telemetryClient)
    {
        _tableStorageService = tableStorageService;
        _logger = logger;
        _telemetryClient = telemetryClient;
    }

    // GET: api/leaderboard/{difficulty}
    [HttpGet("{difficulty}")]
    [ProducesResponseType(typeof(List<PlayerStatEntity>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<List<PlayerStatEntity>>> GetTopPlayers(AIDifficulty difficulty)
    {
        var stopwatch = Stopwatch.StartNew();
        _logger.LogInformation("Received request for top players for difficulty: {Difficulty}", difficulty);

        try
        {
            // We only want top 5 for the UI
            var topPlayers = await _tableStorageService.GetTopPlayersByDifficultyAsync(difficulty, 5);

            stopwatch.Stop();

            // Track performance metric using extension method
            _telemetryClient.TrackLeaderboardLoad(stopwatch.ElapsedMilliseconds, difficulty, topPlayers.Count);

            return Ok(topPlayers);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting top players for difficulty {Difficulty}", difficulty);
            _telemetryClient.TrackOperationException(ex, "GetTopPlayers", new Dictionary<string, string>
            {
                { "Difficulty", difficulty.ToString() }
            });
            return StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving leaderboard data.");
        }
    }

    // PUT: api/leaderboard/players/{playerName}/stats
    [HttpPut("players/{playerName}/stats")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdatePlayerStats(string playerName, [FromBody] PlayerStatUpdateDto updateDto)
    {
        _logger.LogInformation("Received player stat update for Player: {PlayerName}, Difficulty: {Difficulty}, Result: {Result}",
           updateDto.PlayerName, updateDto.Difficulty, updateDto.Result);

        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        // Validate playerName matches DTO
        if (playerName != updateDto.PlayerName)
        {
            return BadRequest("Player name in URL does not match player name in request body.");
        }

        try
        {
            // Convert game time from milliseconds (assuming DTO sends this) to TimeSpan
            var gameTime = TimeSpan.FromMilliseconds(updateDto.GameTimeMilliseconds);
            await _tableStorageService.UpsertPlayerStatAsync(updateDto.PlayerName, updateDto.Difficulty, updateDto.Result, gameTime);

            // Track game completion using extension methods
            _telemetryClient.TrackGameCompleted(
                updateDto.PlayerName,
                updateDto.Difficulty,
                updateDto.Result,
                updateDto.GameTimeMilliseconds);

            _telemetryClient.TrackGameDuration(
                updateDto.GameTimeMilliseconds,
                updateDto.Difficulty,
                updateDto.Result);

            return NoContent(); // Success
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating player stats for Player: {PlayerName}", updateDto.PlayerName);
            _telemetryClient.TrackOperationException(ex, "UpdatePlayerStats", new Dictionary<string, string>
            {
                { "PlayerName", updateDto.PlayerName }
            });
            return StatusCode(StatusCodes.Status500InternalServerError, "Error updating player statistics.");
        }
    }
}

// DTO moved to Shared project
