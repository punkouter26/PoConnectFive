using Microsoft.AspNetCore.Mvc;
// using PoConnectFive.Server.Models; // Removed
using PoConnectFive.Server.Services;
using PoConnectFive.Shared.Models; // Now contains PlayerStatEntity and PlayerStatUpdateDto
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace PoConnectFive.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LeaderboardController : ControllerBase
    {
        private readonly ITableStorageService _tableStorageService;
        private readonly ILogger<LeaderboardController> _logger;

        public LeaderboardController(ITableStorageService tableStorageService, ILogger<LeaderboardController> logger)
        {
            _tableStorageService = tableStorageService;
            _logger = logger;
        }

        // GET: api/leaderboard/{difficulty}
        [HttpGet("{difficulty}")]
        [ProducesResponseType(typeof(List<PlayerStatEntity>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<List<PlayerStatEntity>>> GetTopPlayers(AIDifficulty difficulty)
        {
            _logger.LogInformation("Received request for top players for difficulty: {Difficulty}", difficulty);
            try
            {
                // We only want top 5 for the UI
                var topPlayers = await _tableStorageService.GetTopPlayersByDifficultyAsync(difficulty, 5);
                return Ok(topPlayers);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting top players for difficulty {Difficulty}", difficulty);
                return StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving leaderboard data.");
            }
        }

        // POST: api/leaderboard/playerstats
        [HttpPost("playerstats")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdatePlayerStats([FromBody] PlayerStatUpdateDto updateDto)
        {
            _logger.LogInformation("Received player stat update for Player: {PlayerName}, Difficulty: {Difficulty}, Result: {Result}",
               updateDto.PlayerName, updateDto.Difficulty, updateDto.Result);

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                // Convert game time from milliseconds (assuming DTO sends this) to TimeSpan
                var gameTime = TimeSpan.FromMilliseconds(updateDto.GameTimeMilliseconds);
                await _tableStorageService.UpsertPlayerStatAsync(updateDto.PlayerName, updateDto.Difficulty, updateDto.Result, gameTime);
                return NoContent(); // Success
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating player stats for Player: {PlayerName}", updateDto.PlayerName);
                return StatusCode(StatusCodes.Status500InternalServerError, "Error updating player statistics.");
            }
        }
    }

    // DTO moved to Shared project
}
