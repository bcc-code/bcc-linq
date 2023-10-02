/*
 * BCC Core API
 *
 * This is the Core API
 *
 */


using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using System.Text;
using Newtonsoft.Json;

namespace BccCode.Linq;

public abstract class Result
{
    public abstract object GetData();
}

/// <summary>
/// WrappedPerson
/// </summary>
[DataContract(Name = "Result")]
public partial class Result<T> : Result, IResult<T>, IEquatable<Result<T>>, IValidatableObject
{
    public Result()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="WrappedPerson" /> class.
    /// </summary>
    /// <param name="data">data.</param>
    public Result(T data = default)
    {
        Data = data;
    }

    public override object GetData()
    {
        return Data;
    }

    /// <summary>
    /// Gets or Sets Data
    /// </summary>
    [DataMember(Name = "data", EmitDefaultValue = false)]
    public T Data { get; set; }

    /// <summary>
    /// Returns the string presentation of the object
    /// </summary>
    /// <returns>String presentation of the object</returns>
    public override string ToString()
    {
        var sb = new StringBuilder();
        sb.Append("class Result {\n");
        sb.Append("  Data: ").Append(Data).Append("\n");
        sb.Append("}\n");
        return sb.ToString();
    }

    /// <summary>
    /// Returns the JSON string presentation of the object
    /// </summary>
    /// <returns>JSON string presentation of the object</returns>
    public virtual string ToJson()
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
        return input != null && input.GetType() == typeof(T) && Equals((T)input);
    }

    /// <summary>
    /// Returns true if Wrapped instances are equal
    /// </summary>
    /// <param name="input">Instance of Wrapped to be compared</param>
    /// <returns>Boolean</returns>
    public bool Equals(Result<T> input)
    {
        if (input == null)
            return false;

        return

                Data == null && input.Data == null ||
                Data != null &&
                Data.Equals(input.Data)
            ;
    }

    /// <summary>
    /// Gets the hash code
    /// </summary>
    /// <returns>Hash code</returns>
    public override int GetHashCode()
    {
        unchecked // Overflow is fine, just wrap
        {
            var hashCode = 41;
            if (Data != null)
                hashCode = hashCode * 59 + Data.GetHashCode();
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
}

