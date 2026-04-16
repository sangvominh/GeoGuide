using System.Text.Json;
using MauiApp1.Models;

namespace MauiApp1.Services;

public class PoiCacheService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        WriteIndented = false
    };

    private readonly string _cachePath = Path.Combine(FileSystem.Current.AppDataDirectory, "poi-cache.json");

    public async Task SaveAsync(IReadOnlyList<PointOfInterest> pois, CancellationToken cancellationToken = default)
    {
        await using var stream = File.Create(_cachePath);
        await JsonSerializer.SerializeAsync(stream, pois, JsonOptions, cancellationToken);
    }

    public async Task<IReadOnlyList<PointOfInterest>> LoadCachedAsync(CancellationToken cancellationToken = default)
    {
        if (!File.Exists(_cachePath))
        {
            return [];
        }

        await using var stream = File.OpenRead(_cachePath);
        return await JsonSerializer.DeserializeAsync<List<PointOfInterest>>(stream, JsonOptions, cancellationToken) ?? [];
    }

    public async Task<IReadOnlyList<PointOfInterest>> LoadBundledFallbackAsync(CancellationToken cancellationToken = default)
    {
        await using var stream = await FileSystem.Current.OpenAppPackageFileAsync("poi-fallback.json");
        return await JsonSerializer.DeserializeAsync<List<PointOfInterest>>(stream, JsonOptions, cancellationToken) ?? [];
    }
}
