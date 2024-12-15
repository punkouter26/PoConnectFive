using System;
using System.Threading.Tasks;
using Microsoft.JSInterop;
using System.Text.Json;

namespace PoConnectFive.Client.Services
{
    /// <summary>
    /// Interface for storage operations
    /// Interface Segregation Principle: Focused interface for storage operations
    /// Strategy Pattern: Different storage mechanisms can be implemented
    /// </summary>
    public interface ILocalStorageService
    {
        Task<T?> GetItem<T>(string key);
        Task SetItem<T>(string key, T value);
        Task RemoveItem(string key);
        Task Clear();
    }

    /// <summary>
    /// Implements browser local storage operations
    /// 
    /// SOLID Principles:
    /// - Single Responsibility: Handles only browser storage operations
    /// - Open/Closed: New storage operations can be added without modifying existing code
    /// - Liskov Substitution: Can be replaced with different storage implementation
    /// - Interface Segregation: Implements focused storage interface
    /// - Dependency Inversion: Depends on IJSRuntime abstraction
    /// 
    /// Design Patterns:
    /// - Strategy Pattern: One of multiple possible storage implementations
    /// - Adapter Pattern: Adapts browser localStorage to .NET interface
    /// </summary>
    public class BrowserStorageService : ILocalStorageService
    {
        private readonly IJSRuntime _jsRuntime;
        private readonly JsonSerializerOptions _jsonOptions;

        public BrowserStorageService(IJSRuntime jsRuntime)
        {
            _jsRuntime = jsRuntime;
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
        }

        public async Task<T?> GetItem<T>(string key)
        {
            try
            {
                var json = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", key);
                if (string.IsNullOrEmpty(json))
                    return default;

                return JsonSerializer.Deserialize<T>(json, _jsonOptions);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error reading from local storage: {ex.Message}");
                return default;
            }
        }

        public async Task SetItem<T>(string key, T value)
        {
            try
            {
                var json = JsonSerializer.Serialize(value, _jsonOptions);
                await _jsRuntime.InvokeVoidAsync("localStorage.setItem", key, json);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error writing to local storage: {ex.Message}");
                throw;
            }
        }

        public async Task RemoveItem(string key)
        {
            try
            {
                await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", key);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error removing from local storage: {ex.Message}");
                throw;
            }
        }

        public async Task Clear()
        {
            try
            {
                await _jsRuntime.InvokeVoidAsync("localStorage.clear");
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error clearing local storage: {ex.Message}");
                throw;
            }
        }
    }
}
