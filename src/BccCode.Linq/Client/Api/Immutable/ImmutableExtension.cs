
using BccCode.Platform.Apis;

namespace BccCode.Linq.Client;

public static class ImmutableExtension
{
    public static ImmutableMetadata ToImmutableMetadata(this IMetadata metadata)
    {
        if (metadata is ImmutableMetadata immutableMetadata)
            return immutableMetadata;

        return new ImmutableMetadata(metadata);
    }

    public static ImmutableResultList<T> ToImmutableResultList<T>(this IResultList<T> resultList)
    {
        if (resultList is ImmutableResultList<T> immutableResultList)
            return immutableResultList;

        return new ImmutableResultList<T>(resultList);
    }
}
