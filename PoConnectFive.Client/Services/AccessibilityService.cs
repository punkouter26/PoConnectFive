using Microsoft.JSInterop;
using System.Threading.Tasks;

namespace PoConnectFive.Client.Services
{
    /// <summary>
    /// Service for managing accessibility features including screen reader announcements
    /// </summary>
    public class AccessibilityService
    {
        private readonly IJSRuntime _jsRuntime;

        public AccessibilityService(IJSRuntime jsRuntime)
        {
            _jsRuntime = jsRuntime;
        }

        /// <summary>
        /// Announce a message to screen readers using ARIA live region
        /// </summary>
        public async Task AnnounceAsync(string message, bool assertive = false)
        {
            try
            {
                await _jsRuntime.InvokeVoidAsync("announceToScreenReader", message, assertive);
            }
            catch
            {
                // Silently fail if JavaScript interop not available
            }
        }

        /// <summary>
        /// Announce a game move
        /// </summary>
        public async Task AnnounceMoveAsync(string playerName, int column, int row)
        {
            var message = $"{playerName} placed a piece in column {column + 1}, row {row + 1}";
            await AnnounceAsync(message);
        }

        /// <summary>
        /// Announce turn change
        /// </summary>
        public async Task AnnounceTurnAsync(string playerName)
        {
            var message = $"It is now {playerName}'s turn";
            await AnnounceAsync(message);
        }

        /// <summary>
        /// Announce game end
        /// </summary>
        public async Task AnnounceGameEndAsync(string message)
        {
            await AnnounceAsync(message, assertive: true);
        }

        /// <summary>
        /// Announce invalid move
        /// </summary>
        public async Task AnnounceInvalidMoveAsync()
        {
            await AnnounceAsync("Invalid move. Please select a different column.", assertive: true);
        }
    }
}
