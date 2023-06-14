namespace BccCode.Linq.ApiClient;

public interface IResultList<out T> : IMeta
{
    IReadOnlyList<T> Data { get; }
}
