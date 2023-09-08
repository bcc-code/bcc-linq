namespace BccCode.Linq.ApiClient;

public interface IResultList<out T>
{
    IReadOnlyList<T>? Data { get; }

    IMetadata? Meta { get; }
}
