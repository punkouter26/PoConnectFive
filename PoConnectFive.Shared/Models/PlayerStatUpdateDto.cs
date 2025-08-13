using System.ComponentModel.DataAnnotations;
using PoConnectFive.Shared.Models; // For AIDifficulty, GameResult

namespace PoConnectFive.Shared.Models // Changed namespace
{
    // --- Data Transfer Object (DTO) for POST request ---
    public class PlayerStatUpdateDto
    {
        [Required]
        [StringLength(50, MinimumLength = 1)]
        public string PlayerName { get; set; } = default!;

        [Required]
        public AIDifficulty Difficulty { get; set; }

        [Required]
        public PlayerGameResult Result { get; set; }

        [Required]
        [Range(0, double.MaxValue)] // Basic validation for game time
        public double GameTimeMilliseconds { get; set; }
    }
}
