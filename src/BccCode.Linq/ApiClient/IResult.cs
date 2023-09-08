namespace BccCode.ApiClient;

public interface IResult<T>
{
    T Data { get; set; }
}

