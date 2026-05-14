using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;

namespace GeoGuide.Cms.Controllers.Api.V1;

[ApiController]
[Route("api/v1/maps")]
public class MapsV1Controller(IWebHostEnvironment webHostEnvironment) : ControllerBase
{
    private const string MapsRootRelativePath = "static/maps";
    private static readonly IReadOnlyDictionary<string, string> ContentTypes = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
    {
        [".json"] = "application/json",
        [".pmtiles"] = "application/octet-stream",
        [".pbf"] = "application/x-protobuf",
        [".png"] = "image/png",
        [".jpg"] = "image/jpeg",
        [".jpeg"] = "image/jpeg",
        [".webp"] = "image/webp",
        [".glyphs"] = "application/octet-stream"
    };

    [HttpGet("offline-manifest")]
    public IActionResult OfflineManifest()
    {
        var mapsRoot = GetMapsRoot();
        var packsRoot = Path.Combine(mapsRoot, "packs");
        var stylesRoot = Path.Combine(mapsRoot, "styles");
        var fontsRoot = Path.Combine(mapsRoot, "fonts");

        var packs = Directory.Exists(packsRoot)
            ? Directory.GetDirectories(packsRoot)
                .OrderBy(Path.GetFileName)
                .Select(versionDirectory => new OfflineMapPackVersionDto(
                    Path.GetFileName(versionDirectory),
                    Directory.GetFiles(versionDirectory)
                        .Where(IsServableAsset)
                        .OrderBy(Path.GetFileName)
                        .Select(file => new OfflineMapPackFileDto(
                            Path.GetFileName(file),
                            $"/api/v1/maps/packs/{Uri.EscapeDataString(Path.GetFileName(versionDirectory))}/{Uri.EscapeDataString(Path.GetFileName(file))}",
                            new FileInfo(file).Length))
                        .ToArray()))
                .ToArray()
            : [];

        var styles = Directory.Exists(stylesRoot)
            ? Directory.GetFiles(stylesRoot, "*", SearchOption.AllDirectories)
                .Where(IsServableAsset)
                .Select(file => ToSlashRelativePath(stylesRoot, file))
                .OrderBy(path => path)
                .Select(path => new OfflineMapAssetDto(path, $"/api/v1/maps/styles/{Uri.EscapeDataString(path).Replace("%2F", "/", StringComparison.Ordinal)}"))
                .ToArray()
            : [];

        var fontFamilies = Directory.Exists(fontsRoot)
            ? Directory.GetDirectories(fontsRoot)
                .Select(directory => Path.GetFileName(directory))
                .Where(name => !string.IsNullOrWhiteSpace(name))
                .Select(name => name!)
                .Order()
                .ToArray()
            : [];

        return Ok(new OfflineMapManifestDto(
            DateTimeOffset.UtcNow,
            "/api/v1/maps/packs/{version}/{file}",
            "/api/v1/maps/styles/{path}",
            "/api/v1/maps/fonts/{fontstack}/{range}.pbf",
            packs,
            styles,
            fontFamilies,
            packs.Length == 0
                ? MissingAssetsMessage("map packs", "wwwroot/static/maps/packs/{version}/{file}.pmtiles")
                : null));
    }

    [HttpGet("packs/{version}/{file}")]
    public IActionResult GetPack(string version, string file)
    {
        var packsRoot = Path.Combine(GetMapsRoot(), "packs");
        if (!TryResolveFile(packsRoot, [version, file], out var resolvedPath))
        {
            return MissingFile("map pack", "wwwroot/static/maps/packs/{version}/{file}.pmtiles");
        }

        return ServePhysicalFile(resolvedPath, enableRangeProcessing: true);
    }

    [HttpGet("styles/{**path}")]
    public IActionResult GetStyle(string path)
    {
        var stylesRoot = Path.Combine(GetMapsRoot(), "styles");
        if (!TryResolveFile(stylesRoot, [path], out var resolvedPath))
        {
            return MissingFile("map style", "wwwroot/static/maps/styles/{path}");
        }

        return ServePhysicalFile(resolvedPath, enableRangeProcessing: false);
    }

    [HttpGet("fonts/{fontstack}/{range}.pbf")]
    public IActionResult GetFont(string fontstack, string range)
    {
        var fontsRoot = Path.Combine(GetMapsRoot(), "fonts");
        if (!TryResolveFile(fontsRoot, [fontstack, $"{range}.pbf"], out var resolvedPath))
        {
            return MissingFile("font glyph range", "wwwroot/static/maps/fonts/{fontstack}/{range}.pbf");
        }

        return ServePhysicalFile(resolvedPath, enableRangeProcessing: false);
    }

    private IActionResult ServePhysicalFile(string path, bool enableRangeProcessing)
    {
        var contentType = ContentTypes.GetValueOrDefault(Path.GetExtension(path), "application/octet-stream");
        Response.Headers[HeaderNames.AcceptRanges] = "bytes";

        return PhysicalFile(path, contentType, enableRangeProcessing);
    }

    private NotFoundObjectResult MissingFile(string assetKind, string expectedPath)
    {
        return NotFound(new
        {
            error = $"{assetKind} asset was not found.",
            expectedPath,
            message = MissingAssetsMessage(assetKind, expectedPath)
        });
    }

    private static string MissingAssetsMessage(string assetKind, string expectedPath)
    {
        return $"Place {assetKind} assets under src/GeoGuide.Cms/{expectedPath}. Real PMTiles data is intentionally not committed.";
    }

    private string GetMapsRoot()
    {
        var webRoot = webHostEnvironment.WebRootPath ?? Path.Combine(webHostEnvironment.ContentRootPath, "wwwroot");
        return Path.Combine(webRoot, MapsRootRelativePath);
    }

    private static bool TryResolveFile(string rootPath, IReadOnlyList<string> segments, out string resolvedPath)
    {
        resolvedPath = string.Empty;

        if (segments.Any(segment => string.IsNullOrWhiteSpace(segment) || IsUnsafePathSegment(segment)))
        {
            return false;
        }

        var fullRoot = Path.GetFullPath(rootPath);
        var combinedPath = segments.Aggregate(fullRoot, Path.Combine);
        var fullPath = Path.GetFullPath(combinedPath);
        var rootWithSeparator = fullRoot.EndsWith(Path.DirectorySeparatorChar)
            ? fullRoot
            : fullRoot + Path.DirectorySeparatorChar;

        if (!fullPath.StartsWith(rootWithSeparator, StringComparison.OrdinalIgnoreCase) || !System.IO.File.Exists(fullPath))
        {
            return false;
        }

        resolvedPath = fullPath;
        return true;
    }

    private static bool IsUnsafePathSegment(string segment)
    {
        return segment.Contains("..", StringComparison.Ordinal)
            || Path.IsPathRooted(segment);
    }

    private static bool IsServableAsset(string path)
    {
        var fileName = Path.GetFileName(path);
        return !fileName.StartsWith(".", StringComparison.Ordinal) && fileName != "README.md";
    }

    private static string ToSlashRelativePath(string rootPath, string fullPath)
    {
        return Path.GetRelativePath(rootPath, fullPath).Replace(Path.DirectorySeparatorChar, '/');
    }

    private sealed record OfflineMapManifestDto(
        DateTimeOffset GeneratedAt,
        string PacksRoute,
        string StylesRoute,
        string FontsRoute,
        IReadOnlyCollection<OfflineMapPackVersionDto> Packs,
        IReadOnlyCollection<OfflineMapAssetDto> Styles,
        IReadOnlyCollection<string> FontStacks,
        string? Note);

    private sealed record OfflineMapPackVersionDto(
        string Version,
        IReadOnlyCollection<OfflineMapPackFileDto> Files);

    private sealed record OfflineMapPackFileDto(
        string File,
        string Url,
        long SizeBytes);

    private sealed record OfflineMapAssetDto(
        string Path,
        string Url);
}
