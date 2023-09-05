using System.Collections.Immutable;
using System.Runtime.Serialization;

namespace BccCode.ApiClient.Immutable;

[DataContract(Name = "ResultList")]
public class ImmutableResultList<T> : IResultList<T>
{
    public ImmutableResultList(IResultList<T> resultList)
    {
        if (resultList.Data != null)
            Data = resultList.Data.ToImmutableList();

        if (resultList.Meta != null)
            Meta = resultList.Meta.ToImmutableMetadata();
    }

    [DataMember(Name = "data", EmitDefaultValue = false)]
    public IReadOnlyList<T>? Data { get; }
    
    [DataMember(Name = "meta", EmitDefaultValue = false)]
    public IMetadata Meta { get; }
}
