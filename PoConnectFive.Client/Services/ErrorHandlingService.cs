using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;

namespace PoConnectFive.Client.Services
{
    /// <summary>
    /// Provides centralized error handling functionality for the application.
    /// </summary>
    public class ErrorHandlingService
    {
        private readonly ILogger<ErrorHandlingService> _logger;
        private readonly IJSRuntime _jsRuntime;

        public ErrorHandlingService(ILogger<ErrorHandlingService> logger, IJSRuntime jsRuntime)
        {
            _logger = logger;
            _jsRuntime = jsRuntime;
        }

        /// <summary>
        /// Handles exceptions in a consistent way across the application.
        /// </summary>
        /// <param name="ex">The exception to handle</param>
        /// <param name="context">Additional context about where the error occurred</param>
        /// <param name="showToUser">Whether to display an error message to the user</param>
        public async Task HandleExceptionAsync(Exception ex, string context, bool showToUser = false)
        {
            // Log all exceptions with consistent format
            LogException(ex, context);

            // Optionally show user-friendly message
            if (showToUser)
            {
                await ShowUserFriendlyErrorAsync(GetUserFriendlyMessage(ex));
            }
        }

        /// <summary>
        /// Attempts to execute an action, handling any exceptions that occur.
        /// </summary>
        public async Task<T?> TryExecuteAsync<T>(Func<Task<T>> action, string context, bool showToUser = false)
        {
            try
            {
                return await action();
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(ex, context, showToUser);
                return default;
            }
        }

        /// <summary>
        /// Attempts to execute an action, handling any exceptions that occur.
        /// </summary>
        public async Task TryExecuteAsync(Func<Task> action, string context, bool showToUser = false)
        {
            try
            {
                await action();
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(ex, context, showToUser);
            }
        }

        /// <summary>
        /// Logs exceptions in a consistent format.
        /// </summary>
        private void LogException(Exception ex, string context)
        {
            if (ex is HttpRequestException httpEx)
            {
                _logger.LogError(httpEx, "HTTP Error in {Context}: {StatusCode} - {Message}",
                    context, httpEx.StatusCode, httpEx.Message);
            }
            else
            {
                _logger.LogError(ex, "Error in {Context}: {Message}", context, ex.Message);
            }
        }

        /// <summary>
        /// Gets a user-friendly error message based on the exception type.
        /// </summary>
        private string GetUserFriendlyMessage(Exception ex)
        {
            return ex switch
            {
                HttpRequestException _ => "Unable to connect to the server. Please check your connection and try again.",
                TimeoutException _ => "The operation timed out. Please try again.",
                InvalidOperationException _ => ex.Message, // These often contain useful messages
                _ => "An unexpected error occurred. Please try again later."
            };
        }

        /// <summary>
        /// Shows a user-friendly error message.
        /// </summary>
        private async Task ShowUserFriendlyErrorAsync(string message)
        {
            await _jsRuntime.InvokeVoidAsync("alert", message);
        }
    }
}