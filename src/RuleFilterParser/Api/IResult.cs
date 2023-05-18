namespace RuleFilterParser;

public interface IResult
{
    object? Data { get; }
}

public interface IResult<out T> : IMeta
{
    T? Data { get; }
}