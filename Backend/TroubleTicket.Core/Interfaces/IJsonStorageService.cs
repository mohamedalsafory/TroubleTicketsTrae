using System.Text.Json;

namespace TroubleTicket.Core.Interfaces;

public interface IJsonStorageService
{
    Task<T?> ReadAsync<T>(string fileName);
    Task WriteAsync<T>(string fileName, T data);
    Task<T> ReadOrCreateAsync<T>(string fileName) where T : new();
}