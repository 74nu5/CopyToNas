using System.Text.Json;
using Microsoft.JSInterop;

namespace SftpCopyTool.Web.Services;

public interface ILocalStorageService
{
    Task<T?> GetItemAsync<T>(string key);
    Task SetItemAsync<T>(string key, T value);
    Task RemoveItemAsync(string key);
    Task ClearAsync();
}

public class LocalStorageService : ILocalStorageService
{
    private readonly IJSRuntime _jsRuntime;
    private readonly JsonSerializerOptions _jsonOptions;

    public LocalStorageService(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };
    }

    public async Task<T?> GetItemAsync<T>(string key)
    {
        try
        {
            var json = await _jsRuntime.InvokeAsync<string?>("localStorage.getItem", key);

            if (string.IsNullOrEmpty(json))
                return default;

            return JsonSerializer.Deserialize<T>(json, _jsonOptions);
        }
        catch (Exception)
        {
            // En cas d'erreur de désérialisation, retourner la valeur par défaut
            return default;
        }
    }

    public async Task SetItemAsync<T>(string key, T value)
    {
        try
        {
            var json = JsonSerializer.Serialize(value, _jsonOptions);
            await _jsRuntime.InvokeVoidAsync("localStorage.setItem", key, json);
        }
        catch (Exception)
        {
            // Ignorer les erreurs de sérialisation/stockage
            // (localStorage peut être désactivé ou plein)
        }
    }

    public async Task RemoveItemAsync(string key)
    {
        try
        {
            await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", key);
        }
        catch (Exception)
        {
            // Ignorer les erreurs
        }
    }

    public async Task ClearAsync()
    {
        try
        {
            await _jsRuntime.InvokeVoidAsync("localStorage.clear");
        }
        catch (Exception)
        {
            // Ignorer les erreurs
        }
    }
}
