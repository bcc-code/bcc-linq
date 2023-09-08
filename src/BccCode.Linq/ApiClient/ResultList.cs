using System.Collections;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace BccCode.ApiClient;

[DataContract(Name = "ResultList")]
public class ResultList<T> : IResultList<T>, IEquatable<ResultList<T>>, IValidatableObject
{
    public ResultList()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ResultList{T}" /> class.
    /// </summary>
    /// <param name="data">data.</param>
    /// <param name="meta">meta.</param>
    public ResultList(List<T>? data = default, Metadata? meta = default)
    {
        this.Data = data;
        this.Meta = meta;
    }

    /// <summary>
    /// Gets or Sets Data
    /// </summary>
    [DataMember(Name = "data", EmitDefaultValue = false)]
    public List<T>? Data { get; set; }

    public void AddData(IEnumerable? data)
    {
        Data ??= new List<T>();
        if (data != null)
        {
            Data.AddRange(data.Cast<T>());
        }
    }

    public IEnumerable<object> GetData()
    {
        return (Data ?? new List<T>()).Cast<object>();
    }

    
    /// <summary>
    /// Gets or Sets Meta
    /// </summary>
    [DataMember(Name = "meta", EmitDefaultValue = false)]
    public Metadata? Meta { get; set; }

    /// <summary>
    /// Returns the JSON string presentation of the object
    /// </summary>
    /// <returns>JSON string presentation of the object</returns>
    public string ToJson()
    {
        return JsonConvert.SerializeObject(this, Formatting.Indented);
    }

    /// <summary>
    /// Returns true if objects are equal
    /// </summary>
    /// <param name="input">Object to be compared</param>
    /// <returns>Boolean</returns>
    public override bool Equals(object input)
    {
        if (input is ResultList<T> resultList)
        {
            return this.Equals(resultList);            
        }

        return false;
    }

    /// <summary>
    /// Returns true if WrappedWithMetaArray instances are equal
    /// </summary>
    /// <param name="input">Instance of WrappedWithMetaArray to be compared</param>
    /// <returns>Boolean</returns>
    public bool Equals(ResultList<T>? input)
    {
        if (input == null)
            return false;

        return
            (
                this.Data == input.Data ||
                this.Data != null &&
                input.Data != null &&
                this.Data.SequenceEqual(input.Data)
            ) &&
            (
                this.Meta == input.Meta ||
                (this.Meta != null &&
                this.Meta.Equals(input.Meta))
            );
    }

    /// <summary>
    /// Gets the hash code
    /// </summary>
    /// <returns>Hash code</returns>
    public override int GetHashCode()
    {
        unchecked // Overflow is fine, just wrap
        {
            int hashCode = 41;
            if (this.Data != null)
                hashCode = hashCode * 59 + this.Data.GetHashCode();
            if (this.Meta != null)
                hashCode = hashCode * 59 + this.Meta.GetHashCode();
            return hashCode;
        }
    }

    /// <summary>
    /// To validate all properties of the instance
    /// </summary>
    /// <param name="validationContext">Validation context</param>
    /// <returns>Validation Result</returns>
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        yield break;
    }

    #region IResultList<T>

    IReadOnlyList<T>? IResultList<T>.Data => Data;

    IMetadata? IResultList<T>.Meta => Meta;

    #endregion
}
