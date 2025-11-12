using Microsoft.JSInterop;

namespace PoConnectFive.Client.Services;

/// <summary>
/// Service for managing application themes including dark mode and high contrast.
/// </summary>
public class ThemeService
{
    private readonly IJSRuntime _jsRuntime;
    private const string THEMEKEY = "app-theme";
    private string _currentTheme = "auto"; // auto, light, dark
    private bool _isHighContrast = false;

    public event Action? OnThemeChanged;

    public ThemeService(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    public bool IsHighContrast => _isHighContrast;
    public string CurrentTheme => _currentTheme;

    /// <summary>
    /// Load theme preference from local storage.
    /// </summary>
    public async Task LoadThemePreferenceAsync()
    {
        try
        {
            var savedTheme = await _jsRuntime.InvokeAsync<string?>("localStorage.getItem", THEMEKEY);
            _currentTheme = savedTheme ?? "auto";
            await ApplyThemeAsync();

            // Load high contrast separately
            var highContrastStored = await _jsRuntime.InvokeAsync<string?>("localStorage.getItem", "theme-high-contrast");
            _isHighContrast = highContrastStored == "true";
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading theme preference: {ex.Message}");
            _currentTheme = "auto";
        }
    }

    /// <summary>
    /// Set theme to auto, light, or dark.
    /// </summary>
    public async Task SetThemeAsync(string theme)
    {
        if (theme != "auto" && theme != "light" && theme != "dark")
        {
            theme = "auto";
        }

        _currentTheme = theme;
        await ApplyThemeAsync();
        await SaveThemePreferenceAsync(theme);
        OnThemeChanged?.Invoke();
    }

    /// <summary>
    /// Cycle through theme options: auto -> light -> dark -> auto.
    /// </summary>
    public async Task CycleThemeAsync()
    {
        var nextTheme = _currentTheme switch
        {
            "auto" => "light",
            "light" => "dark",
            "dark" => "auto",
            _ => "auto"
        };

        await SetThemeAsync(nextTheme);
    }

    /// <summary>
    /// Toggle high contrast mode (legacy support).
    /// </summary>
    public async Task ToggleHighContrastAsync()
    {
        _isHighContrast = !_isHighContrast;
        await ApplyThemeAsync();

        try
        {
            await _jsRuntime.InvokeVoidAsync("localStorage.setItem", "theme-high-contrast", _isHighContrast.ToString().ToLower());
        }
        catch
        {
        }

        OnThemeChanged?.Invoke();
    }

    /// <summary>
    /// Set high contrast mode (legacy support).
    /// </summary>
    public async Task SetHighContrastAsync(bool enabled)
    {
        if (_isHighContrast != enabled)
        {
            _isHighContrast = enabled;
            await ApplyThemeAsync();

            try
            {
                await _jsRuntime.InvokeVoidAsync("localStorage.setItem", "theme-high-contrast", _isHighContrast.ToString().ToLower());
            }
            catch
            {
            }

            OnThemeChanged?.Invoke();
        }
    }

    private async Task ApplyThemeAsync()
    {
        try
        {
            // Apply theme attribute
            if (_currentTheme == "auto")
            {
                await _jsRuntime.InvokeVoidAsync("eval",
                    "document.documentElement.removeAttribute('data-theme')");
            }
            else
            {
                await _jsRuntime.InvokeVoidAsync("eval",
                    $"document.documentElement.setAttribute('data-theme', '{_currentTheme}')");
            }

            // Apply high contrast class (legacy)
            if (_isHighContrast)
            {
                await _jsRuntime.InvokeVoidAsync("eval",
                    "document.body.classList.add('high-contrast')");
            }
            else
            {
                await _jsRuntime.InvokeVoidAsync("eval",
                    "document.body.classList.remove('high-contrast')");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error applying theme: {ex.Message}");
        }
    }

    private async Task SaveThemePreferenceAsync(string theme)
    {
        try
        {
            await _jsRuntime.InvokeVoidAsync("localStorage.setItem", THEMEKEY, theme);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error saving theme preference: {ex.Message}");
        }
    }

    public string GetThemeIcon()
    {
        return _currentTheme switch
        {
            "light" => "☀️",
            "dark" => "🌙",
            "auto" => "⚙️",
            _ => "⚙️"
        };
    }

    public string GetThemeLabel()
    {
        return _currentTheme switch
        {
            "light" => "Switch to Dark Mode",
            "dark" => "Switch to Auto Mode",
            "auto" => "Switch to Light Mode",
            _ => "Toggle Theme"
        };
    }
}
