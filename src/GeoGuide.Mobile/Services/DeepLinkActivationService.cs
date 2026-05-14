namespace MauiApp1.Services;

public sealed class DeepLinkActivationService
{
    private Uri? _pendingUri;

    public event EventHandler<Uri>? LinkReceived;

    public void Publish(Uri uri)
    {
        _pendingUri = uri;
        LinkReceived?.Invoke(this, uri);
    }

    public Uri? ConsumePendingUri()
    {
        var current = _pendingUri;
        _pendingUri = null;
        return current;
    }
}
