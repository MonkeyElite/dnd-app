namespace DndApp.Media.Options;

public sealed class S3StorageOptions
{
    public string Endpoint { get; set; } = string.Empty;

    public string AccessKey { get; set; } = string.Empty;

    public string SecretKey { get; set; } = string.Empty;

    public bool UseSsl { get; set; }

    public string Bucket { get; set; } = string.Empty;

    public string? Region { get; set; }

    public string? PublicBaseUrl { get; set; }
}
