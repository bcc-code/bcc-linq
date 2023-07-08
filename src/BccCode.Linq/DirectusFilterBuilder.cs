using System.Linq.Expressions;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace BccCode.Linq;

[Obsolete]
public class DirectusFilterBuilder<T>
{
    private IQueryable<T> _queryable;

    public DirectusFilterBuilder()
    {
        _queryable = new List<T>().AsQueryable();
    }

    public DirectusFilterBuilder(IQueryable<T> queryable)
    {
        _queryable = queryable;
    }

    [Obsolete("Use the constructor instead")]
    public static DirectusFilterBuilder<T> Create()
    {
        return new DirectusFilterBuilder<T>();
    }

    public DirectusFilterBuilder<T> Build() => this;

    public string Serialize()
    {
        JObject filterObject = new JObject();

        if (_queryable.Expression is MethodCallExpression methodCall)
        {
            if (methodCall.Method.Name == "Where")
            {
                var lambda = (LambdaExpression)((UnaryExpression)methodCall.Arguments[1]).Operand;

                ProcessBinaryExpression(lambda.Body as BinaryExpression, filterObject);
            }
        }

        return JsonConvert.SerializeObject(filterObject);
    }

    private static void ProcessBinaryExpression(BinaryExpression? binaryExpression, JObject filterObject)
    {
        if (binaryExpression == null) return;

        if (binaryExpression.NodeType == ExpressionType.AndAlso)
        {
            ProcessBinaryExpression(binaryExpression.Left as BinaryExpression, filterObject);
            ProcessBinaryExpression(binaryExpression.Right as BinaryExpression, filterObject);
        }
        else
        {
            var left = binaryExpression.Left as MemberExpression;
            var right = UnwrapConvert(binaryExpression.Right);

            if (left != null && right != null)
            {
                var propertyName = left.Member.Name;
                var filterValue = right;

                JObject operationObject;

                switch (binaryExpression.NodeType)
                {
                    case ExpressionType.Equal:
                        operationObject = new JObject { { "_eq", filterValue } };
                        break;
                    case ExpressionType.NotEqual:
                        operationObject = new JObject { { "_neq", filterValue } };
                        break;
                    case ExpressionType.GreaterThanOrEqual:
                        operationObject = new JObject { { "_gte", filterValue } };
                        break;
                    case ExpressionType.LessThanOrEqual:
                        operationObject = new JObject { { "_lte", filterValue } };
                        break;
                    case ExpressionType.GreaterThan:
                        operationObject = new JObject { { "_gt", filterValue } };
                        break;
                    case ExpressionType.LessThan:
                        operationObject = new JObject { { "_lt", filterValue } };
                        break;
                    default:
                        throw new NotSupportedException($"The provided expression {binaryExpression.NodeType} is not supported.");
                }

                if (filterObject[propertyName] == null)
                {
                    filterObject[propertyName] = new JObject();
                }

                foreach (var property in operationObject.Properties())
                {
#pragma warning disable CS8602
#pragma warning disable CS8600
                    ((JObject)filterObject[propertyName]).Add(property);
#pragma warning restore CS8600
#pragma warning restore CS8602
                }
            }
        }
    }
    private static string? UnwrapConvert(Expression expression)
    {
        if (expression is ConstantExpression constExpr)
        {
            return constExpr.Value.ToString();
        }
        else if (expression is UnaryExpression { Operand: MemberExpression memberExpr } &&
                 memberExpr.Expression is ConstantExpression constExpr2)
        {
            var container = constExpr2.Value;
            return ((FieldInfo)memberExpr.Member).GetValue(container).ToString();
        }
        else if (expression is NewExpression newExpr && newExpr.Type == typeof(DateTime))
        {
            var constructorArgs = newExpr.Arguments.Select(arg =>
            {
                if (arg is ConstantExpression ce) return ce.Value;
                throw new NotSupportedException("Unsupported expression type in DateTime constructor.");
            }).ToArray();

            var date = Activator.CreateInstance(typeof(DateTime), constructorArgs) is DateTime
                ? (DateTime)Activator.CreateInstance(typeof(DateTime), constructorArgs)
                : default;
            if (date != default)
            {
                return date.ToString("o");
            }
        }
        else if (expression is MemberExpression fieldExpr && fieldExpr.Type == typeof(DateTime))
        {
            if (fieldExpr.Expression is ConstantExpression constExpr3)
            {
                var container = constExpr3.Value;
                if (((FieldInfo)fieldExpr.Member).GetValue(container) is DateTime date)
                {
                    return date.ToString("o");
                }
            }
        }


        return null;
    }

    public DirectusFilterBuilder<T> Where(Expression<Func<T, bool>> predicate)
    {
        _queryable = _queryable.Where(predicate);
        return this;
    }
}
