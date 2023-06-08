using System.Linq.Expressions;

namespace BccCode.Linq;

/// <summary>
/// Adds extension methods for <see cref="IQueryable{T}"/> to specify advanced behavior not covered
/// by default C# Linq. 
/// </summary>
public static class QueryableExtensions
{
    [Obsolete("Not implemented yet", true)]
    public static IQueryable<TEntity> Include<TEntity, TProperty>(
        this IQueryable<TEntity> source,
        Expression<Func<TEntity, TProperty>> navigationPropertyPath)
    {
        throw new NotImplementedException();
    }
}
