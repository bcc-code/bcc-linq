namespace RuleFilterParser;

public interface IResultList<out T> : IMeta
{
    IReadOnlyList<T> Data { get; }
}
