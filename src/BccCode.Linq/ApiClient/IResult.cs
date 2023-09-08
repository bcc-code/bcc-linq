namespace BccCode.Linq.ApiClient;

public interface IResult<T>
{
    T Data { get; set; }
}

