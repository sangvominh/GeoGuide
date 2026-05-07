using SQLite;

namespace MauiApp1.Services;

[Table("sync_state")]
public sealed class LocalSyncStateRecord
{
    [PrimaryKey]
    public string Key { get; set; } = string.Empty;

    public string Value { get; set; } = string.Empty;
}
