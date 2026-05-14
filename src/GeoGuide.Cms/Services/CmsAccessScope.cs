namespace GeoGuide.Cms.Services;

public sealed record CmsAccessScope(bool IsSystemAdmin, Guid? TenantId, string[] Permissions);
