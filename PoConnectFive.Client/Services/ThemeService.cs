using Microsoft.JSInterop;
using System;
using System.Threading.Tasks;

namespace PoConnectFive.Client.Services
{
    /// <summary>
    /// Service for managing application themes including high contrast mode
    /// </summary>
    public class ThemeService
    {
        private readonly IJSRuntime _jsRuntime;
        private bool _isHighContrast = false;

        public event Action? OnThemeChanged;

        public ThemeService(IJSRuntime jsRuntime)
        {
            _jsRuntime = jsRuntime;
        }

        public bool IsHighContrast => _isHighContrast;

        /// <summary>
        /// Toggle high contrast mode
        /// </summary>
        public async Task ToggleHighContrastAsync()
        {
            _isHighContrast = !_isHighContrast;
            await ApplyThemeAsync();
            OnThemeChanged?.Invoke();
        }

        /// <summary>
        /// Set high contrast mode
        /// </summary>
        public async Task SetHighContrastAsync(bool enabled)
        {
            if (_isHighContrast != enabled)
            {
                _isHighContrast = enabled;
                await ApplyThemeAsync();
                OnThemeChanged?.Invoke();
            }
        }

        /// <summary>
        /// Load theme preference from local storage
        /// </summary>
        public async Task LoadThemePreferenceAsync()
        {
            try
            {
                var stored = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", "theme-high-contrast");
                _isHighContrast = stored == "true";
                await ApplyThemeAsync();
            }
            catch
            {
                // Default to standard theme if storage unavailable
            }
        }

        private async Task ApplyThemeAsync()
        {
            try
            {
                if (_isHighContrast)
                {
                    await _jsRuntime.InvokeVoidAsync("document.body.classList.add", "high-contrast");
                }
                else
                {
                    await _jsRuntime.InvokeVoidAsync("document.body.classList.remove", "high-contrast");
                }

                // Save preference
                await _jsRuntime.InvokeVoidAsync("localStorage.setItem", "theme-high-contrast", _isHighContrast.ToString().ToLower());
            }
            catch
            {
                // Silently fail if JavaScript interop not available
            }
        }
    }
}
