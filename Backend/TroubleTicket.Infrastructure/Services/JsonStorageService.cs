using Microsoft.Extensions.Configuration;
using System.Text.Json;
using TroubleTicket.Core.Interfaces;

namespace TroubleTicket.Infrastructure.Services;

public class JsonStorageService : IJsonStorageService
{
    private readonly string _basePath;
    private static readonly JsonSerializerOptions _options = new()
    {
        WriteIndented = true
    };

    public JsonStorageService(IConfiguration configuration)
    {
        _basePath = Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory,
            configuration["Storage:BasePath"] ?? "Data"
        );
        
        if (!Directory.Exists(_basePath))
        {
            Directory.CreateDirectory(_basePath);
        }
    }

    public async Task<T?> ReadAsync<T>(string fileName)
    {
        var filePath = GetFilePath(fileName);
        if (!File.Exists(filePath))
        {
            return default;
        }

        var json = await File.ReadAllTextAsync(filePath);
        return JsonSerializer.Deserialize<T>(json, _options);
    }

    public async Task WriteAsync<T>(string fileName, T data)
    {
        var filePath = GetFilePath(fileName);
        var json = JsonSerializer.Serialize(data, _options);
        await File.WriteAllTextAsync(filePath, json);
    }

    public async Task<T> ReadOrCreateAsync<T>(string fileName) where T : new()
    {
        try
        {
            return await ReadAsync<T>(fileName) ?? new T();
        }
        catch (JsonException)
        {
            return new T();
        }
    }

    private string GetFilePath(string fileName)
    {
        return Path.Combine(_basePath, fileName);
    }
}