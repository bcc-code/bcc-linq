namespace BccCode.Linq.Client;

/// <summary>
/// Supports queryable Include/ThenInclude chaining operators.
/// </summary>
/// <typeparam name="TEntity">The entity type.</typeparam>
/// <typeparam name="TProperty">The property type.</typeparam>
/// <remarks>
/// This is a clone of
/// <a href="https://learn.microsoft.com/en-us/dotnet/api/microsoft.entityframeworkcore.query.iincludablequeryable-2?view=efcore-7.0">
/// Microsoft.EntityFrameworkCore.Query.IIncludableQueryable</a>
/// </remarks>
public interface IIncludableQueryable<out TEntity, out TProperty> : IQueryable<TEntity>
{
}
