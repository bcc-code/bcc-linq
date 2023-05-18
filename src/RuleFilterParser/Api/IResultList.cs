namespace RuleFilterParser;

public interface IResultList<T> : IMeta
{
    IReadOnlyList<T> Data { get; }
}