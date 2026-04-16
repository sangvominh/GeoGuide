using System.Security.Cryptography;
using MauiApp1.Models;

namespace MauiApp1.Services;

public sealed class MediaPrefetchService
{
    private readonly HttpClient _httpClient;
    private readonly PoiApiOptions _poiApiOptions;

    public MediaPrefetchService(HttpClient httpClient, PoiApiOptions poiApiOptions)
    {
        _httpClient = httpClient;
        _poiApiOptions = poiApiOptions;
    }

    public async Task PrefetchNearbyAsync(IReadOnlyList<PointOfInterest> pois, int maxItems, CancellationToken cancellationToken = default)
    {
        var ordered = pois
            .Where(static poi => !string.IsNullOrWhiteSpace(poi.AudioUrl))
            .OrderBy(static poi => poi.DistanceMeters)
            .Take(maxItems)
            .ToList();

        foreach (var poi in ordered)
        {
            await EnsureAudioCachedAsync(poi.AudioUrl, cancellationToken);
        }
    }

    public async Task<Uri?> ResolvePlaybackUriAsync(string audioUrl, CancellationToken cancellationToken = default)
    {
        var sourceUri = ResolveSourceUri(audioUrl);
        if (sourceUri == null)
        {
            return null;
        }

        var cachedPath = GetCachedPath(sourceUri);
        if (File.Exists(cachedPath))
        {
            return new Uri(cachedPath);
        }

        return sourceUri;
    }

    public Task<int> CountCachedMediaAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var root = Path.Combine(FileSystem.Current.AppDataDirectory, "media-cache");
        if (!Directory.Exists(root))
        {
            return Task.FromResult(0);
        }

        var count = Directory.EnumerateFiles(root, "*", SearchOption.TopDirectoryOnly).Count();
        return Task.FromResult(count);
    }

    private async Task EnsureAudioCachedAsync(string audioUrl, CancellationToken cancellationToken)
    {
        var sourceUri = ResolveSourceUri(audioUrl);
        if (sourceUri == null)
        {
            return;
        }

        var cachedPath = GetCachedPath(sourceUri);
        if (File.Exists(cachedPath))
        {
            return;
        }

        Directory.CreateDirectory(Path.GetDirectoryName(cachedPath)!);
        using var response = await _httpClient.GetAsync(sourceUri, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            return;
        }

        await using var remote = await response.Content.ReadAsStreamAsync(cancellationToken);
        await using var local = File.Create(cachedPath);
        await remote.CopyToAsync(local, cancellationToken);
    }

    private Uri? ResolveSourceUri(string audioUrl)
    {
        if (string.IsNullOrWhiteSpace(audioUrl))
        {
            return null;
        }

        if (Uri.TryCreate(audioUrl, UriKind.Absolute, out var absolute))
        {
            return absolute;
        }

        return Uri.TryCreate(new Uri(_poiApiOptions.BaseUrl, UriKind.Absolute), audioUrl.TrimStart('/'), out var relative)
            ? relative
            : null;
    }

    private static string GetCachedPath(Uri sourceUri)
    {
        var root = Path.Combine(FileSystem.Current.AppDataDirectory, "media-cache");
        var extension = Path.GetExtension(sourceUri.AbsolutePath);
        if (string.IsNullOrWhiteSpace(extension))
        {
            extension = ".bin";
        }

        var keyBytes = SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(sourceUri.AbsoluteUri));
        var key = Convert.ToHexString(keyBytes).ToLowerInvariant();
        return Path.Combine(root, $"{key}{extension}");
    }
}
