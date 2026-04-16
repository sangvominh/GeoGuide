using SQLite;

namespace MauiApp1.Services;

public sealed class LocalDatabaseService
{
    private readonly SemaphoreSlim _initLock = new(1, 1);
    private SQLiteAsyncConnection? _connection;

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        if (_connection is not null)
        {
            return;
        }

        await _initLock.WaitAsync(cancellationToken);
        try
        {
            if (_connection is not null)
            {
                return;
            }

            var dbPath = Path.Combine(FileSystem.Current.AppDataDirectory, "geoguide-local.db3");
            var flags = SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.Create | SQLiteOpenFlags.SharedCache;

            _connection = new SQLiteAsyncConnection(dbPath, flags);
            await _connection.CreateTableAsync<LocalPoiRecord>();
            await _connection.CreateTableAsync<LocalSyncStateRecord>();
            await _connection.CreateTableAsync<LocalOfflineLogRecord>();
        }
        finally
        {
            _initLock.Release();
        }
    }

    public async Task ReplacePoisAsync(IReadOnlyList<LocalPoiRecord> pois, CancellationToken cancellationToken = default)
    {
        await InitializeAsync(cancellationToken);
        var connection = _connection!;

        await connection.RunInTransactionAsync(transaction =>
        {
            transaction.DeleteAll<LocalPoiRecord>();
            transaction.InsertAll(pois);
        });
    }

    public async Task<IReadOnlyList<LocalPoiRecord>> GetPoisAsync(CancellationToken cancellationToken = default)
    {
        await InitializeAsync(cancellationToken);
        return await _connection!.Table<LocalPoiRecord>().ToListAsync();
    }

    public async Task<string?> GetSyncStateValueAsync(string key, CancellationToken cancellationToken = default)
    {
        await InitializeAsync(cancellationToken);

        var record = await _connection!
            .Table<LocalSyncStateRecord>()
            .FirstOrDefaultAsync(row => row.Key == key);

        return record?.Value;
    }

    public async Task SetSyncStateValueAsync(string key, string value, CancellationToken cancellationToken = default)
    {
        await InitializeAsync(cancellationToken);

        var record = new LocalSyncStateRecord
        {
            Key = key,
            Value = value
        };

        await _connection!.InsertOrReplaceAsync(record);
    }

    public async Task InsertOfflineLogAsync(LocalOfflineLogRecord record, CancellationToken cancellationToken = default)
    {
        await InitializeAsync(cancellationToken);
        await _connection!.InsertAsync(record);
    }
}
