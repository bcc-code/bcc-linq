using System.Collections.Immutable;
using System.Runtime.Serialization;

namespace BccCode.Linq.Client;

public class ImmutableResultList<T> : IResultList<T>
{
    public ImmutableResultList(IResultList<T> resultList)
    {
        if (resultList.Data != null)
            Data = resultList.Data.ToImmutableList();

        if (resultList.Meta != null)
            Meta = resultList.Meta.ToImmutableMetadata();
    }

    public IReadOnlyList<T>? Data { get; }

    public IMetadata Meta { get; }
}
