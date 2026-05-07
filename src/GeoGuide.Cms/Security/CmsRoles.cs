namespace GeoGuide.Cms.Security;

public static class CmsRoles
{
    public const string SystemAdmin = "SystemAdmin";
    public const string PoiTenant = "PoiTenant";

    public static readonly string[] All = [SystemAdmin, PoiTenant];
}
