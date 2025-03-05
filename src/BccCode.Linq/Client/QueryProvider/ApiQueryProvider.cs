using System.Collections;
using System.Diagnostics;
using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using BccCode.Platform;
using Newtonsoft.Json;

namespace BccCode.Linq.Client;

internal class ApiQueryProvider : ExpressionVisitor, IQueryProvider, IAsyncQueryProvider
{
    private enum VisitLinqLambdaMode
    {
        Undefined,

        // ReSharper disable once InvalidXmlDocComment
        /// <summary>
        /// Expression is inside a <see cref="System.Linq.Queryable.Select" /> function.
        /// </summary>
        Select,

        // ReSharper disable once InvalidXmlDocComment
        /// <summary>
        /// Expression is inside a <see cref="System.Linq.Queryable.Where" /> function.
        /// </summary>
        Where,

        // ReSharper disable InvalidXmlDocComment
        /// <summary>
        /// Expression is inside a <see cref="System.Linq.Queryable.OrderBy" />
        /// or <see cref="System.Linq.Queryable.ThenBy"/> function.
        /// </summary>
        // ReSharper restore InvalidXmlDocComment
        OrderBy,

        // ReSharper disable InvalidXmlDocComment
        /// <summary>
        /// Expression is inside a <see cref="System.Linq.Queryable.OrderByDescending" />
        /// or <see cref="System.Linq.Queryable.ThenByDescending"/> function.
        /// </summary>
        // ReSharper restore InvalidXmlDocComment
        OrderByDescending,

        // ReSharper disable InvalidXmlDocComment
        /// <summary>
        /// Expression is inside a <see cref="QueryableExtensions.Include" />
        /// function.
        /// </summary>
        // ReSharper restore InvalidXmlDocComment
        Include,

        // ReSharper disable InvalidXmlDocComment
        /// <summary>
        /// Expression is inside a <see cref="QueryableExtensions.ThenInclude" />
        /// function.
        /// </summary>
        // ReSharper restore InvalidXmlDocComment
        ThenInclude,

        // ReSharper disable InvalidXmlDocComment
        /// <summary>
        /// Expression is inside a <see cref="QueryableExtensions.Convert" />
        /// function.
        /// </summary>
        // ReSharper restore InvalidXmlDocComment
        Convert
    }

    /// <summary>
    /// The type which is expected.
    /// </summary>
    private enum QueryableTypeMode
    {
        /// <summary>
        /// The queryable should return a <see cref="IEnumerable{T}"/>.
        /// </summary>
        Enumerable,

        /// <summary>
        /// The queryable should return a <see cref="IAsyncEnumerable{T}"/>.
        /// </summary>
        AsyncEnumerable
    }

    /// <summary>
    /// A provider class cannot handle multiple executions at the same time.
    ///
    /// This lock object is used to make sure this does not happen.
    /// </summary>
    private readonly object _queryBuilderLock = new();
    private readonly IHttpApiQueryableClient _apiClient;

    /// <summary>
    /// The URL path to the endpoint passed to the API client.
    /// </summary>
    private readonly string _path;
    private readonly Action<IQueryableParameters>? _parametersCallback;

    /// <summary>
    /// An internal mode used during expression visiting to know what kind of action should be taken.
    /// </summary>
    private VisitLinqLambdaMode _visitMode = VisitLinqLambdaMode.Undefined;
    private readonly Dictionary<ParameterExpression, IApiCaller> _activeParameters = new();
    private StringBuilder? _where;
    private StringBuilder? _selectField;
    private readonly Dictionary<IApiCaller, List<string>> _selectFields = new();
    private readonly Dictionary<IApiCaller, List<string>> _includeChains = new();
    private List<string>? _includeChainList;
    private StringBuilder? _includeChain;
    private bool _inverseOperator;
    private int _memberDepth;
    private int _indent;
    private QueryableTypeMode _expectedTypeMode;

    /// <summary>
    /// Initializes a query provider which performs queries against a REST API client implementing
    /// the <a href="https://docs.directus.io/reference/query.html"/> Directus REST API style.
    /// </summary>
    /// <param name="apiClient">
    /// The API client.
    /// </param>
    /// <param name="path">
    /// The URL path passed to the API client on request.
    /// </param>
    /// <exception cref="ArgumentNullException"></exception>
    public ApiQueryProvider(IHttpApiQueryableClient apiClient, string path = "", Action<IQueryableParameters>? parametersCallback = default(Action<IQueryableParameters>))
    {
        _apiClient = apiClient ?? throw new ArgumentNullException(nameof(apiClient));
        _path = path;
        _parametersCallback = parametersCallback;
    }

    #region IQueryProvider

    public IQueryable CreateQuery(Expression expression)
    {
        Type? elementType = TypeHelper.GetElementType(expression.Type);
        try
        {
            return (IQueryable)Activator.CreateInstance(typeof(ApiQueryable<>).MakeGenericType(elementType), this, expression);
        }
        catch (System.Reflection.TargetInvocationException tie)
        {
            throw tie.InnerException ?? tie;
        }
    }

    public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
    {
        return new ApiQueryable<TElement>(this, expression);
    }

    private Expression TranslateExpression(Expression expression, QueryableTypeMode expectedTypeMode)
    {
        lock (_queryBuilderLock)
        {
            _expectedTypeMode = expectedTypeMode;
            _visitMode = VisitLinqLambdaMode.Undefined;
            Debug.Assert(_mapFromToApiCallers.Count == 0);
            Debug.Assert(_includeChain == null);
            Expression? translatedExpression = this.Visit(Evaluator.PartialEval(
                expression,
                fnCanBeEvaluated: e => e.NodeType != ExpressionType.Parameter &&
                                       !(e is ConstantExpression constantExpression &&
                                         constantExpression.Type.IsGenericType &&
                                         constantExpression.Type.GetGenericTypeDefinition() == typeof(ApiQueryable<>))
                ));
            Debug.Assert(_activeParameters.Count == 0);
            Debug.Assert(translatedExpression != null);

            // finalize IApiRequest ...
            foreach (var apiCaller in _mapFromToApiCallers.Values)
            {
                var fields = new StringBuilder(apiCaller.QueryParameters.Fields);

                if (_selectFields.TryGetValue(apiCaller, out var selectFieldList))
                {
                    foreach (var selectField in selectFieldList
                                 .Distinct()  // remove the duplicates
                             )
                    {
                        if (fields.Length > 0)
                            fields.Append(",");
                        fields.Append(selectField);
                    }
                }

                if (fields.Length == 0)
                    // the linq 'Select' option has not been used --> set * to get all fields
                    fields.Append("*");

                if (_includeChains.TryGetValue(apiCaller, out var includeChainList))
                {
                    foreach (var includeChain in includeChainList)
                    {
                        fields.Append(",");
                        fields.Append(includeChain);
                    }
                }

                if (fields.Length > 0)
                    apiCaller.QueryParameters.Fields = fields.ToString();
            }

            // Clear internal variables
            _mapFromToApiCallers.Clear();
            _selectFields.Clear();
            _includeChains.Clear();
            // ReSharper disable once RedundantCheckBeforeAssignment
            if (_includeChain != null)
                _includeChain = null;

            return translatedExpression;
        }
    }

    public object Execute(Expression expression)
    {
        var translatedExpression = TranslateExpression(expression, QueryableTypeMode.Enumerable);
        var lambdaExpression = Expression.Lambda<Func<object>>(translatedExpression);
        Func<object> lambdaFunc = lambdaExpression.Compile();
        return lambdaFunc.Invoke();
    }

    public TResult? Execute<TResult>(Expression expression)
    {
        var translatedExpression = TranslateExpression(expression, QueryableTypeMode.Enumerable);
        var lambdaExpression = Expression.Lambda<Func<TResult>>(translatedExpression);
        Func<TResult?> lambdaFunc = lambdaExpression.Compile();
        var result = lambdaFunc.Invoke();
        return result == null ? default : (TResult)result;
    }

    #endregion

    #region IAsyncQueryProvider

    public TResult ExecuteAsync<TResult>(Expression expression, CancellationToken cancellationToken = default)
    {
        var translatedExpression = TranslateExpression(expression, QueryableTypeMode.AsyncEnumerable);
        var lambdaExpression = Expression.Lambda<Func<TResult>>(translatedExpression);
        Func<TResult> lambdaFunc = lambdaExpression.Compile();
        return lambdaFunc.Invoke();
    }

    #endregion

    /// <summary>
    /// A map holding the API caller classes (eg. <see cref="ApiPagedEnumerable"/>) mapped to the constant Expression
    /// to the <see cref="ApiQueryable{T}"/> (which is replaced with an instance of <see cref="ApiPagedEnumerable"/>.
    ///
    /// This makes it theoretically possible to use a type twice in a query e.g. when performing a cross join.
    ///
    /// In other words, you have a <see cref="IApiCaller"/> instance per 'FROM'/'JOIN' of your linq expression. 
    /// </summary>
    private readonly Dictionary<Expression, IApiCaller> _mapFromToApiCallers = new();

    /// <summary>
    /// Tries to get the <see cref="IApiCaller"/> from an expression by traveling though a possible chain of
    /// Linq <see cref="Queryable"/> method calls.
    ///
    /// Note: In case we add custom Queryable extensions, here is the place this needs to be adjusted. Potentially a
    /// check for a extension method with first parameter holding a <see cref="IQueryable{T}"/> could do this better.  
    /// </summary>
    /// <param name="node"></param>
    /// <param name="apiCaller"></param>
    /// <returns></returns>
    private bool TryGetApiCaller(Expression node, out IApiCaller apiCaller)
    {
        if (_mapFromToApiCallers.TryGetValue(node, out apiCaller))
        {
            return true;
        }

        while (node is MethodCallExpression mc)
        {
            if (mc.Method.DeclaringType == typeof(Queryable) ||
                mc.Method.DeclaringType == typeof(QueryableExtensions))
            {
                node = mc.Arguments[0];
            }
            else
            {
                // The Linq method call chain uses an unsupported extension method base class, e.g. an extension method from the entity framework.
                // We use here pessimistic behavior and throw this as an error since we don't know if in such a case something should happen
                // or the extension method call should be ignored.
                throw new InvalidOperationException(
                    "The Linq method call chain uses an unsupported extension method base class.");
            }
        }

        Debug.Assert(node.NodeType == ExpressionType.Constant);
        if (_mapFromToApiCallers.TryGetValue(node, out apiCaller))
        {
            return true;
        }

        return false;
    }

    #region Visitors


    /// <summary>
    /// Visits a where clause which is to be processed throw the API.
    ///
    /// It does not return any expression since the whole predicate is assumed to be interpretable by the API.
    /// The expression tree parser will throw an exception when the where clause cannot be interpreted by the API.
    /// </summary>
    /// <param name="node">
    /// The lambda predicate expression of the where clause
    /// </param>
    /// <param name="apiCaller">
    /// The api caller object to which the where clause shall be added.
    /// </param>
    private void VisitWhereClause(LambdaExpression node, IApiCaller apiCaller)
    {
        _visitMode = VisitLinqLambdaMode.Where;
        _activeParameters.Add(node.Parameters[0], apiCaller);
        _where = new StringBuilder();

        Visit(node.Body);

        _visitMode = VisitLinqLambdaMode.Undefined;
        _activeParameters.Remove(node.Parameters[0]);
        apiCaller.QueryParameters.Filter = string.IsNullOrEmpty(apiCaller.QueryParameters.Filter)
            ? _where.ToString()
            : $"{{ \"_and\" : [ {apiCaller.QueryParameters.Filter} , {_where} ] }}";
        _where = null;
    }

    protected override Expression VisitConstant(ConstantExpression node)
    {
        if (_visitMode == VisitLinqLambdaMode.Where)
        {
            Debug.Assert(_where != null);
            TransformConstant(_where, node);

            // Constant is passed to _where, therefore we return an empty expression.
            return Expression.Empty();
        }

        if (node.Type.IsGenericType)
        {
            if (node.Type.GetGenericTypeDefinition() == typeof(ApiQueryable<>))
            {
                return TransformApiQueryableExpression(node);
            }
        }

        return base.VisitConstant(node);
    }

    protected override Expression VisitMember(MemberExpression node)
    {
        List<MemberExpression> nestedMemberExpressions = new();
        Expression findNode = node;
        int depth = 0;
        while (findNode is not ParameterExpression &&
               findNode is MemberExpression m)
        {
            depth++;
            findNode = m.Expression;
            nestedMemberExpressions.Add(m);
        }
        nestedMemberExpressions.RemoveAt(0);
        nestedMemberExpressions.Reverse();

        if (depth > _indent)
        {
            _indent = depth;
        }
        else if (depth < _indent)
        {
            _indent = depth;
        }

        if (depth > _memberDepth)
        {
            _memberDepth = depth;
        }

        if (findNode is ParameterExpression parameterNode)
        {
            if (_activeParameters.TryGetValue(parameterNode, out var apiCaller))
            {
                string ApiMemberToJsonString(MemberInfo member)
                {
                    var dataMemberAttribute = member.GetCustomAttribute<DataMemberAttribute>();
                    if (dataMemberAttribute != null)
                    {
                        return dataMemberAttribute.Name;
                    }

#if NET6_0_OR_GREATER
                    var jsonPropertyNameAttribute = member.GetCustomAttribute<System.Text.Json.Serialization.JsonPropertyNameAttribute>();
                    if (jsonPropertyNameAttribute != null)
                    {
                        return jsonPropertyNameAttribute.Name;
                    }
#endif

                    var jsonPropertyAttribute = member.GetCustomAttribute<JsonPropertyAttribute>();
                    if (jsonPropertyAttribute != null)
                    {
                        return jsonPropertyAttribute.PropertyName ?? member.Name.ToCamelCase();
                    }

                    return member.Name.ToCamelCase();
                }

                string ApiMemberName() => ApiMemberToJsonString(node.Member);

                string ApiMemberPath(string splitter = ".")
                {
                    var memberPath = new StringBuilder();

                    foreach (var nestedMemberExpression in nestedMemberExpressions)
                    {
                        memberPath.Append(
                            ApiMemberToJsonString(nestedMemberExpression.Member)
                        );
                        memberPath.Append(splitter);
                    }

                    memberPath.Append(ApiMemberName());

                    return memberPath.ToString();
                }

                switch (_visitMode)
                {
                    case VisitLinqLambdaMode.Select:
                        {
                            bool initializedStringBuilder;
                            if (_selectField == null)
                            {
                                initializedStringBuilder = true;
                                _selectField = new StringBuilder();
                            }
                            else
                            {
                                initializedStringBuilder = false;
                            }

                            base.VisitMember(node);

                            _selectField.Append(ApiMemberName());

                            if (initializedStringBuilder)
                            {
                                if (node.Type != typeof(string) && node.Type.IsClass || node.Type.IsInterface)
                                {
                                    // it is not a scalar type, add `.*` at the end of the field selection
                                    _selectField.Append(".*");
                                }

                                if (!_selectFields.TryGetValue(apiCaller, out List<string> selectFieldsList))
                                {
                                    selectFieldsList = new List<string>();
                                    _selectFields.Add(apiCaller, selectFieldsList);
                                }

                                selectFieldsList.Add(_selectField.ToString());
                                _selectField = null;
                            }
                            else
                            {
                                _selectField.Append(".");
                            }

                            return node;
                        }
                    case VisitLinqLambdaMode.Where:
                        {
                            Debug.Assert(_where != null);
                            _where.Append(ApiMemberPath(splitter: "\": {\""));
                            return Expression.Empty();
                        }
                    case VisitLinqLambdaMode.OrderBy:
                        {
                            var memberPath = ApiMemberPath();
                            apiCaller.QueryParameters.Sort = string.IsNullOrEmpty(apiCaller.QueryParameters.Sort)
                                ? memberPath
                                : $"{apiCaller.QueryParameters.Sort},{memberPath}";
                            return Expression.Empty();
                        }
                    case VisitLinqLambdaMode.OrderByDescending:
                        {
                            var memberPath = ApiMemberPath();
                            apiCaller.QueryParameters.Sort = string.IsNullOrEmpty(apiCaller.QueryParameters.Sort)
                                ? $"-{memberPath}"
                                : $"{apiCaller.QueryParameters.Sort},-{memberPath}";
                            return Expression.Empty();
                        }
                    case VisitLinqLambdaMode.Include:
                        {
                            Debug.Assert(_includeChain != null);
                            _includeChain.Append(ApiMemberPath());
                            return Expression.Empty();
                        }
                    case VisitLinqLambdaMode.ThenInclude:
                        {
                            Debug.Assert(_includeChain != null);
                            Debug.Assert(_includeChain.Length > 0); // a 'ThenInclude' chain must have already the root member from 'Include'
                            _includeChain.Append(".");
                            _includeChain.Append(ApiMemberPath());
                            return Expression.Empty();
                        }
                    case VisitLinqLambdaMode.Convert:
                        {
                            //We assume conversion is implicitly handled on the server side
                            Debug.Assert(_where != null);
                            _where.Append(ApiMemberPath(splitter: "\": {\""));
                            return Expression.Empty();
                        }
                    default:
                        throw new NotSupportedException($"Member {node.Member.Name} used in an unsupported manner");
                }
            }
            else
            {
                Debug.WriteLine("MemberExpression to ParameterExpression which is unknown");
            }
        }
        else
        {
            Debug.WriteLine("Unhandled MemberExpression");
        }

        return base.VisitMember(node);
    }

    protected override Expression VisitBinary(BinaryExpression node)
    {
        if (_visitMode == VisitLinqLambdaMode.Where)
        {
            Debug.Assert(_where != null);

            string op;
            bool logicalOperator = false;
            bool isNullCheck = false;
            switch (node.NodeType)
            {
                case ExpressionType.Equal:
                    {
                        if (node.Right is ConstantExpression constantExpression &&
                            constantExpression.Value == null)
                        {
                            isNullCheck = true;
                            op = _inverseOperator ? "_nnull" : "_null";
                            break;
                        }

                        op = _inverseOperator ? "_neq" : "_eq";
                        break;
                    }
                case ExpressionType.AndAlso:
                    {
                        op = _inverseOperator ? "_or" : "_and";
                        logicalOperator = true;
                        break;
                    }
                case ExpressionType.OrElse:
                    {
                        op = _inverseOperator ? "_and" : "_or";
                        logicalOperator = true;
                        break;
                    }
                case ExpressionType.LessThan:
                    {
                        op = _inverseOperator ? "_gte" : "_lt";
                        break;
                    }
                case ExpressionType.GreaterThan:
                    {
                        op = _inverseOperator ? "_lte" : "_gt";
                        break;
                    }
                case ExpressionType.LessThanOrEqual:
                    {
                        op = _inverseOperator ? "_gt" : "_lte";
                        break;
                    }
                case ExpressionType.GreaterThanOrEqual:
                    {
                        op = _inverseOperator ? "_lt" : "_gte";
                        break;
                    }
                case ExpressionType.NotEqual:
                    {
                        if (node.Right is ConstantExpression constantExpression &&
                            constantExpression.Value == null)
                        {
                            isNullCheck = true;
                            op = _inverseOperator ? "_null" : "_nnull";
                            break;
                        }

                        op = _inverseOperator ? "_eq" : "_neq";
                        break;
                    }
                default:
                    throw new NotSupportedException($"Operator '{node.NodeType}' is not supported.");
            }

            if (logicalOperator)
            {
                _where.Append($"{{\"{op}\": [");
                var left = Visit(node.Left);
                Debug.Assert(left != null);
                Debug.Assert(left.NodeType == ExpressionType.Default);
                _where.Append(", ");
                var right = Visit(node.Right);
                Debug.Assert(right != null);
                Debug.Assert(right.NodeType == ExpressionType.Default);
                _where.Append("]}");
            }
            else
            {
                _where.Append("{\"");
                var left = Visit(node.Left);
                Debug.Assert(left != null);
                Debug.Assert(left.NodeType == ExpressionType.Default);
                int depth = _memberDepth;
                _where.Append("\": {\"");
                _where.Append(op);
                _where.Append("\": ");
                if (isNullCheck)
                {
                    _where.Append("true");
                }
                else
                {
                    var right = Visit(node.Right);
                    Debug.Assert(right != null);
                    Debug.Assert(right.NodeType == ExpressionType.Default);
                }
                if (depth > 1)
                {
                    // close nested members
                    for (int n = 1; n < depth; n++)
                        _where.Append("}");
                }
                _where.Append("}}");
            }

            return Expression.Empty();
        }
        else
        {
            return base.VisitBinary(node);
        }
    }

    protected override Expression VisitUnary(UnaryExpression node)
    {
        if (_visitMode == VisitLinqLambdaMode.Where)
        {
            switch (node.NodeType)
            {
                case ExpressionType.Not:
                    {
                        _inverseOperator = true;
                        var operandExpression = Visit(node.Operand);
                        _inverseOperator = false;

                        Debug.Assert(operandExpression != null);
                        Debug.Assert(operandExpression.NodeType == ExpressionType.Default);

                        // We drop here the Not expression since we will invert it later in code...
                        return operandExpression;
                    }
                case ExpressionType.Convert:
                    {
                        _visitMode = VisitLinqLambdaMode.Convert;
                        var expression = Visit(node.Operand);
                        _visitMode = VisitLinqLambdaMode.Where;
                        return expression;
                    }
                default:
                    throw new NotSupportedException($"Unsupported unary expression type in Where clause: {node.NodeType}");
            }
        }

        return base.VisitUnary(node);
    }

    private Expression VisitWhereMethodCallToString(MethodCallExpression node)
    {
        if (node.Arguments.Count != 0)
        {
            throw new NotSupportedException(
                "object.ToString method call is currently only supported in a Where clause without arguments");
        }

        Debug.Assert(node.Object != null);
        var objectType = node.Object.Type;
        if (objectType.IsGenericType && objectType.GetGenericTypeDefinition() == typeof(Nullable<>))
        {
            objectType = Nullable.GetUnderlyingType(objectType);
            Debug.Assert(objectType != null);
        }

        if (TypeHelper.IsNumberType(objectType))
        {
            throw new NotSupportedException(
                "object.ToString on number types (int, long, short, ...) is not supported in a Where clause");
        }
        else if (objectType == typeof(bool))
        {
            throw new NotSupportedException(
                $"ToString on type {objectType.FullName} is not supported in a Where clause");
        }

        if (objectType == typeof(string) || objectType == typeof(Guid) || objectType == typeof(DateTime)
#if NET6_0_OR_GREATER
                                         || objectType == typeof(DateOnly)
#endif
           )
        {
            // Officially supported types with ToString()
        }
        else
        {
            Debug.WriteLine($"{nameof(ApiQueryProvider)}: Using object.ToString() call on type {objectType.FullName} is not officially tested in a Where clause. You might end up getting unexpected results.");
        }

        // We skipp interpreting ToString() because it will already be passed as string in JSON

        var obj = Visit(node.Object);
        Debug.Assert(obj != null);
        Debug.Assert(obj.NodeType == ExpressionType.Default);
        return Expression.Empty();
    }

    protected override Expression VisitMethodCall(MethodCallExpression node)
    {
        if (_visitMode == VisitLinqLambdaMode.Where)
        {
            if (node.Method.DeclaringType == typeof(string))
            {
                switch (node.Method.Name)
                {
                    case nameof(string.StartsWith):
                        {
                            Debug.Assert(_where != null);
                            _where.Append("{\"");
                            var obj = Visit(node.Object);
                            Debug.Assert(obj != null);
                            Debug.Assert(obj.NodeType == ExpressionType.Default);
                            _where.Append("\": {\"");
                            // ReSharper disable once StringLiteralTypo
                            _where.Append(_inverseOperator ? "_nstarts_with" : "_starts_with");
                            _where.Append("\": ");
                            if (node.Arguments[0] is ConstantExpression c)
                            {
                                var constant = VisitConstant(c);
                                Debug.Assert(constant != null);
                                Debug.Assert(constant.NodeType == ExpressionType.Default);
                            }
                            else
                            {
                                throw new Exception("StartsWith is only supported for constant expressions");
                            }
                            _where.Append("}}");
                            return Expression.Empty();
                        }
                    case nameof(string.EndsWith):
                        {
                            Debug.Assert(_where != null);
                            _where.Append("{\"");
                            var obj = Visit(node.Object);
                            Debug.Assert(obj != null);
                            Debug.Assert(obj.NodeType == ExpressionType.Default);
                            _where.Append("\": {\"");
                            // ReSharper disable once StringLiteralTypo
                            _where.Append(_inverseOperator ? "_nends_with" : "_ends_with");
                            _where.Append("\": ");
                            if (node.Arguments[0] is ConstantExpression c)
                            {
                                var constant = VisitConstant(c);
                                Debug.Assert(constant != null);
                                Debug.Assert(constant.NodeType == ExpressionType.Default);
                            }
                            else
                            {
                                throw new Exception("EndsWith is only supported for constant expressions");
                            }
                            _where.Append("}}");
                            return Expression.Empty();
                        }
                    case nameof(string.Contains):
                        {
                            // CASE 1: .Where(p => p.Name.Contains("x"))
                            //         .Where(p => p.Tags.Contains("Alias"))
                            if (node.Arguments.Count != 1)
                            {
                                throw new NotSupportedException(
                                    $"Unsupported {node.Method.DeclaringType?.FullName}.{node.Method.Name} signature. Contains method can only be used without the StringComparison parameter");

                            }
                            Debug.Assert(_where != null);
                            _where.Append("{\"");
                            var obj = Visit(node.Object);
                            Debug.Assert(obj != null);
                            Debug.Assert(obj.NodeType == ExpressionType.Default);
                            _where.Append("\": {\"");
                            // ReSharper disable once StringLiteralTypo
                            _where.Append(_inverseOperator ? "_ncontains" : "_contains");
                            _where.Append("\": ");
                            var arg0 = Visit(node.Arguments[0]);
                            Debug.Assert(arg0 != null);
                            Debug.Assert(arg0.NodeType == ExpressionType.Default);
                            _where.Append("}}");
                            return Expression.Empty();
                        }
                    case nameof(string.IsNullOrEmpty):
                        {
                            if (node.Arguments[0] is MemberExpression)
                            {
                                Debug.Assert(_where != null);
                                _where.Append("{\"");
                                var arg0 = Visit(node.Arguments[0]);
                                Debug.Assert(arg0 != null);
                                Debug.Assert(arg0.NodeType == ExpressionType.Default);
                                _where.Append("\": {\"");
                                // ReSharper disable once StringLiteralTypo
                                _where.Append(_inverseOperator ? "_nempty" : "_empty");
                                _where.Append("\": ");
                                _where.Append("null"); // TODO / NOTE: I do not know what value to pass when using a '_empty/_nempty' operator so I just pass 'null' and I hope it gets ignored
                                _where.Append("}}");
                                return Expression.Empty();
                            }

                            throw new NotSupportedException(
                                "string.IsNullOrEmpty is currently only supported in a Where clause with a member expression");
                        }
                    case nameof(string.ToString):
                        {
                            if (node.Arguments.Count != 0)
                            {
                                throw new NotSupportedException(
                                    "string.ToString method call is currently only supported in a Where clause without arguments");
                            }

                            // Useless call of ToString():
                            // Optimize query by removing the ToString() call from the expression tree.
                            var obj = Visit(node.Object);
                            Debug.Assert(obj != null);
                            Debug.Assert(obj.NodeType == ExpressionType.Default);
                            return Expression.Empty();
                        }
                    default:
                        throw new NotSupportedException($"Not supported method call: {node.Method.DeclaringType?.FullName}.{node.Method.Name}");
                }
            }
            else if (node.Method.DeclaringType == typeof(Enumerable)) // Array
            {
                switch (node.Method.Name)
                {
                    case nameof(Enumerable.Contains):
                        {
                            // .Where(p => titles.Contains(p.Title))
                            if (node.Arguments.Count != 2)
                            {
                                throw new NotSupportedException(
                                    $"Unsupported {node.Method.DeclaringType?.FullName}.{node.Method.Name} signature. The method can only be used without comparer.");
                            }

                            if (node.Arguments[0] is ConstantExpression c)
                            {
                                Debug.Assert(_where != null);
                                _where.Append("{\"");
                                var arg1 = Visit(node.Arguments[1]);
                                Debug.Assert(arg1 != null);
                                Debug.Assert(arg1.NodeType == ExpressionType.Default);
                                _where.Append("\": {\"");
                                _where.Append(_inverseOperator ? "_nin" : "_in");
                                _where.Append("\": [");

                                // NOTE: here we execute a Expression (and enumeration) during visiting (!)
                                var values = ((IEnumerable)c.Value)?.Cast<object>().Where(o => o != null).ToArray();

                                if (values != null)
                                {
                                    bool first = true;
                                    foreach (var value in values)
                                    {
                                        if (first)
                                        {
                                            first = false;
                                        }
                                        else
                                        {
                                            _where.Append(",");
                                        }
                                        TransformConstant(_where, Expression.Constant(value));
                                    }
                                }

                                _where.Append("]}}");

                                return Expression.Empty();
                            }

                            throw new NotSupportedException("Not supported source for Enumerable.");
                        }
                    default:
                        throw new NotSupportedException($"Not supported method call: {node.Method.DeclaringType?.FullName}.{node.Method.Name}");
                }
            }
            else if (node.Method.DeclaringType != typeof(string) && node.Method.DeclaringType?.GetInterface(nameof(IEnumerable)) != null) // List<> etc
            {
                switch (node.Method.Name)
                {
                    case nameof(Enumerable.Contains):
                        {
                            // .Where(p => titles.Contains(p.Title))

                            if (node.Object is ConstantExpression c)
                            {
                                Debug.Assert(_where != null);
                                _where.Append("{\"");
                                var arg1 = Visit(node.Arguments[0]);
                                Debug.Assert(arg1 != null);
                                Debug.Assert(arg1.NodeType == ExpressionType.Default);
                                _where.Append("\": {\"");
                                _where.Append(_inverseOperator ? "_nin" : "_in");
                                _where.Append("\": [");

                                // NOTE: here we execute a Expression (and enumeration) during visiting (!)
                                var values = ((IEnumerable)c.Value)?.Cast<object>().Where(o => o != null).ToArray();

                                if (values != null)
                                {
                                    bool first = true;
                                    foreach (var value in values)
                                    {
                                        if (first)
                                        {
                                            first = false;
                                        }
                                        else
                                        {
                                            _where.Append(",");
                                        }
                                        TransformConstant(_where, Expression.Constant(value));
                                    }
                                }

                                _where.Append("]}}");

                                return Expression.Empty();
                            }

                            throw new NotSupportedException("Not supported for IEnumerable source.");
                        }
                    default:
                        throw new NotSupportedException($"Not supported method call: {node.Method.DeclaringType?.FullName}.{node.Method.Name}");
                }
            }
            else if (node.Method.DeclaringType == typeof(object))
            {
                switch (node.Method.Name)
                {
                    case nameof(object.Equals):
                        // When using object.Equals(,) in a Where query, we need to handle its logic here
                        // separately.
                        Debug.Assert(_where != null);
                        _where.Append("{\"");
                        var arg0 = Visit(node.Arguments[0]);
                        Debug.Assert(arg0 != null);
                        Debug.Assert(arg0.NodeType == ExpressionType.Default);
                        int depth = _memberDepth;
                        _where.Append("\": {\"");
                        _where.Append(_inverseOperator ? "_neq" : "_eq");
                        _where.Append("\": ");
                        var arg1 = Visit(node.Arguments[1]);
                        Debug.Assert(arg1 != null);
                        Debug.Assert(arg1.NodeType == ExpressionType.Default);
                        if (depth > 1)
                        {
                            // close nested members
                            for (int n = 1; n < depth; n++)
                                _where.Append("}");
                        }
                        _where.Append("}}");

                        return Expression.Empty();
                    case nameof(object.ToString):
                        return VisitWhereMethodCallToString(node);
                    default:
                        throw new NotSupportedException($"Not supported method call: {node.Method.DeclaringType?.FullName}.{node.Method.Name}");
                }
            }
            else if (node.Method.Name == "ToString")
            {
                return VisitWhereMethodCallToString(node);
            }
            else
            {
                throw new NotSupportedException($"Not supported method call: {node.Method.DeclaringType?.FullName}.{node.Method.Name}");
            }
        }

        if (node.Method.DeclaringType == typeof(Queryable))
        {
            switch (node.Method.Name)
            {
                case nameof(Queryable.Any):
                    {
                        //Arg 0: source
                        var source = Visit(node.Arguments[0]);
                        Debug.Assert(source != null);
                        if (!TryGetApiCaller(node.Arguments[0], out var apiCaller))
                        {
                            // ... the source was not a ApiQueryable and is not
                            // part of the API. So we just do nothing and pass it
                        }
                        else
                        {
                            apiCaller.QueryParameters.Limit = 1;

                            if (node.Arguments.Count == 1)
                            {
                                // We remove here the Take method call from the expression tree,
                                // because the Take is done by the API.
                                return Expression.Call(
                                    instance: null,
                                    method: System.Linq.Internal.Enumerable.AnyMethodInfo.MakeGenericMethod(
                                        node.Method.GetGenericArguments()[0]),
                                    arguments: new[] { source });
                            }
                            else if (node.Arguments.Count == 2 &&
                                     node.Arguments[1] is UnaryExpression u &&
                                     u.Operand is LambdaExpression l)
                            {
                                Debug.Assert(l.Parameters.Count == 1);

                                VisitWhereClause(l, apiCaller);

                                // We remove here the predicate from the expression tree, because
                                // the where logic is processed by the API.
                                // NOTE: In case some where expressions cannot be passed over to the API,
                                //       this would be the place where you can still decide to keep a
                                //       modified Where expression in the tree for the parts which the API
                                //       cannot handle.
                                return Expression.Call(
                                    instance: null,
                                    method: System.Linq.Internal.Enumerable.AnyMethodInfo.MakeGenericMethod(
                                        node.Method.GetGenericArguments()[0]),
                                    arguments: new[] { source });
                            }
                            else
                            {
                                throw new NotSupportedException(
                                    $"Unsupported {node.Method.DeclaringType?.FullName}.{node.Method.Name} signature. The method can only be used without parameters.");
                            }
                        }
                    }
                    break;
                case nameof(Queryable.ElementAt):
                    {
                        var source = Visit(node.Arguments[0]);
                        Debug.Assert(source != null);
                        if (!TryGetApiCaller(node.Arguments[0], out var apiCaller))
                        {
                            // ... the source was not a ApiQueryable and is not
                            // part of the API. So we just do nothing and pass it
                        }
                        else if (node.Arguments[1] is ConstantExpression c)
                        {
                            if (c.Type != typeof(int))
                                throw new NotSupportedException(
                                    "The parameter for Queryable.ElementAt/ElementAtOrDefault must be a constant integer expression");
                            apiCaller.QueryParameters.Offset = (int)c.Value;
                            apiCaller.QueryParameters.Limit = 1;

                            // We remove here the Take method call from the expression tree,
                            // because the Take is done by the API.
                            return Expression.Call(
                                instance: null,
                                method: System.Linq.Internal.Enumerable.FirstMethodInfo.MakeGenericMethod(
                                    node.Method.GetGenericArguments()[0]),
                                arguments: new[] { source });
                        }
                        else
                        {
                            throw new Exception("Format for ElementAt/ElementAtOrDefault expression not supported.");
                        }
                    }
                    break;
                case nameof(Queryable.ElementAtOrDefault):
                    {
                        var source = Visit(node.Arguments[0]);
                        Debug.Assert(source != null);
                        if (!TryGetApiCaller(node.Arguments[0], out var apiCaller))
                        {
                            // ... the source was not a ApiQueryable and is not
                            // part of the API. So we just do nothing and pass it
                        }
                        else if (node.Arguments[1] is ConstantExpression c)
                        {
                            if (c.Type != typeof(int))
                                throw new NotSupportedException(
                                    "The parameter for Queryable.ElementAt/ElementAtOrDefault must be a constant integer expression");
                            apiCaller.QueryParameters.Offset = (int)c.Value > 0 ? (int)c.Value : null;
                            apiCaller.QueryParameters.Limit = 1;

                            // We remove here the Take method call from the expression tree,
                            // because the Take is done by the API.
                            return Expression.Call(
                                instance: null,
                                method: System.Linq.Internal.Enumerable.FirstOrDefaultMethodInfo.MakeGenericMethod(
                                    node.Method.GetGenericArguments()[0]),
                                arguments: new[] { source });
                        }
                        else
                        {
                            throw new Exception("Format for ElementAt/ElementAtOrDefault expression not supported.");
                        }
                    }
                    break;
                case nameof(Queryable.First):
                    {
                        var source = Visit(node.Arguments[0]);
                        Debug.Assert(source != null);
                        if (!TryGetApiCaller(node.Arguments[0], out var apiCaller))
                        {
                            // ... the source was not a ApiQueryable and is not
                            // part of the API. So we just do nothing and pass it
                        }
                        else if (node.Arguments.Count == 1)
                        {
                            apiCaller.QueryParameters.Limit = 1;

                            // We remove here the Take method call from the expression tree,
                            // because the Take is done by the API.
                            return Expression.Call(
                                instance: null,
                                method: System.Linq.Internal.Enumerable.FirstMethodInfo.MakeGenericMethod(
                                    node.Method.GetGenericArguments()[0]),
                                arguments: new[] { source });
                        }
                        else
                        {
                            throw new NotSupportedException($"Unsupported {node.Method.DeclaringType?.FullName}.{node.Method.Name} signature. The method can only be used without parameters.");
                        }
                    }
                    break;
                case nameof(Queryable.FirstOrDefault):
                    {
                        var source = Visit(node.Arguments[0]);
                        Debug.Assert(source != null);
                        if (!TryGetApiCaller(node.Arguments[0], out var apiCaller))
                        {
                            // ... the source was not a ApiQueryable and is not
                            // part of the API. So we just do nothing and pass it
                        }
                        else if (node.Arguments.Count == 1)
                        {
                            apiCaller.QueryParameters.Limit = 1;

                            // We remove here the Take method call from the expression tree,
                            // because the Take is done by the API.
                            return Expression.Call(
                                instance: null,
                                method: System.Linq.Internal.Enumerable.FirstOrDefaultMethodInfo.MakeGenericMethod(
                                    node.Method.GetGenericArguments()[0]),
                                arguments: new[] { source });
                        }
                        else
                        {
                            throw new NotSupportedException($"Unsupported {node.Method.DeclaringType?.FullName}.{node.Method.Name} signature. The method can only be used without parameters.");
                        }
                    }
                    break;
                case nameof(Queryable.Where):
                    {
                        //Arg 0: source
                        var source = Visit(node.Arguments[0]);
                        Debug.Assert(source != null);
                        if (!TryGetApiCaller(node.Arguments[0], out var apiCaller))
                        {
                            // ... the source was not a ApiQueryable and is not
                            // part of the API. So we just do nothing and pass it
                        }
                        //Arg 1: predicate
                        else if (node.Arguments[1] is UnaryExpression u && u.Operand is LambdaExpression l)
                        {
                            Debug.Assert(l.Parameters.Count == 1);

                            VisitWhereClause(l, apiCaller);

                            // We remove here the Where method call from the expression tree, because
                            // the where logic is processed by the API.
                            // NOTE: In case some where expressions cannot be passed over to the API,
                            //       this would be the place where you can still decide to keep a
                            //       modified Where expression in the tree for the parts which the API
                            //       cannot handle.
                            return source;
                        }
                        throw new Exception("Syntax of Where expression not supported.");
                    }
                case nameof(Queryable.Select):
                    {
                        //Arg 0: source
                        var source = Visit(node.Arguments[0]);
                        Debug.Assert(source != null);
                        if (!TryGetApiCaller(node.Arguments[0], out var apiCaller))
                        {
                            // ... the source was not a ApiQueryable and is not
                            // part of the API. So we just do nothing and pass it
                        }
                        //Arg 1: selector
                        else if (node.Arguments[1] is UnaryExpression u)
                        {
                            Delegate? selector;
                            if (u.Operand is LambdaExpression l)
                            {
                                Debug.Assert(l.Parameters.Count == 1);

                                // Lambda: row => row
                                if (l.Body is ParameterExpression p && p == l.Parameters[0])
                                {
                                    if (!_selectFields.TryGetValue(apiCaller, out var selectFieldsList))
                                        selectFieldsList = new List<string>();

                                    selectFieldsList.Add("*");
                                }
                                else
                                {
                                    _visitMode = VisitLinqLambdaMode.Select;
                                    _activeParameters.Add(l.Parameters[0], apiCaller);

                                    Visit(l.Body);

                                    _visitMode = VisitLinqLambdaMode.Undefined;
                                    _activeParameters.Remove(l.Parameters[0]);
                                }

                                // We compile here the select expression for further usage ...
                                selector = l.Compile();
                            }
                            else
                            {
                                throw new Exception("Expected a Lambda expression in a Select query.");
                            }

                            // Note: Above we used the ExpressionVisitor logic to travel through the Select tree and find out
                            //       what properties we need to retrieve from the API. Here we now select an alternative
                            //       Select method which is not based on IQueryable<T> which performs a client-side execution
                            //       of the compiled selector:
                            //         when the return is IEnumerable<T>, we use System.Linq.Enumerable.Select
                            //         when the return is IAsyncEnumerable<T>, we use BccCode.Linq.Async.AsyncEnumerable.Select

                            MethodInfo? genericMethod;
                            switch (_expectedTypeMode)
                            {
                                case QueryableTypeMode.Enumerable:
                                    genericMethod = System.Linq.Internal.Enumerable.SelectMethodInfo;
                                    break;
                                case QueryableTypeMode.AsyncEnumerable:
                                    genericMethod = AsyncEnumerable.SelectMethodInfo;
                                    break;
                                default:
                                    throw new ArgumentOutOfRangeException();
                            }

                            Debug.Assert(genericMethod != null);
                            var method = genericMethod.MakeGenericMethod(
                                u.Type.GenericTypeArguments[0].GenericTypeArguments[0],
                                u.Type.GenericTypeArguments[0].GenericTypeArguments[1]
                            );

                            // Replace the exception with the new source.
                            return Expression.Call(
                                instance: null,
                                method: method,
                                arguments: new[]
                                {
                                    source,
                                    Expression.Constant(selector) // materialized u Expression                                    
                                });
                        }
                        throw new Exception("Syntax of Select expression not supported.");
                    }
                case nameof(Queryable.Single):
                    {
                        var source = Visit(node.Arguments[0]);
                        Debug.Assert(source != null);
                        if (!TryGetApiCaller(node.Arguments[0], out var apiCaller))
                        {
                            // ... the source was not a ApiQueryable and is not
                            // part of the API. So we just do nothing and pass it
                        }
                        else if (node.Arguments.Count == 1)
                        {
                            apiCaller.QueryParameters.Limit = 2;

                            // We remove here the Take method call from the expression tree,
                            // because the Take is done by the API.
                            return Expression.Call(
                                instance: null,
                                method: System.Linq.Internal.Enumerable.SingleMethodInfo.MakeGenericMethod(
                                    node.Method.GetGenericArguments()[0]),
                                arguments: new[] { source });
                        }
                        else
                        {
                            throw new NotSupportedException($"Unsupported {node.Method.DeclaringType?.FullName}.{node.Method.Name} signature. The method can only be used without parameters.");
                        }
                    }
                    break;
                case nameof(Queryable.SingleOrDefault):
                    {
                        var source = Visit(node.Arguments[0]);
                        Debug.Assert(source != null);
                        if (!TryGetApiCaller(node.Arguments[0], out var apiCaller))
                        {
                            // ... the source was not a ApiQueryable and is not
                            // part of the API. So we just do nothing and pass it
                        }
                        else if (node.Arguments.Count == 1)
                        {
                            apiCaller.QueryParameters.Limit = 2;

                            // We remove here the Take method call from the expression tree,
                            // because the Take is done by the API.
                            return Expression.Call(
                                instance: null,
                                method: System.Linq.Internal.Enumerable.SingleOrDefaultMethodInfo.MakeGenericMethod(
                                    node.Method.GetGenericArguments()[0]),
                                arguments: new[] { source });
                        }
                        else
                        {
                            throw new NotSupportedException($"Unsupported {node.Method.DeclaringType?.FullName}.{node.Method.Name} signature. The method can only be used without parameters.");
                        }
                    }
                    break;
                case nameof(Queryable.OrderBy):
                case nameof(Queryable.ThenBy):
                case nameof(Queryable.OrderByDescending):
                case nameof(Queryable.ThenByDescending):
                    {
                        if (node.Arguments.Count != 2)
                        {
                            throw new NotSupportedException($"Not supported method call: {node.Method.DeclaringType?.FullName}.{node.Method.Name}");
                        }

                        //Arg 0: source
                        var source = Visit(node.Arguments[0]);
                        Debug.Assert(source != null);
                        if (!TryGetApiCaller(node.Arguments[0], out var apiCaller))
                        {
                            // ... the source was not a ApiQueryable and is not
                            // part of the API. So we just do nothing and pass it
                        }
                        // Args[1] keySelector
                        else if (node.Arguments[1] is UnaryExpression u && u.Operand is LambdaExpression l)
                        {
                            var sourceType = node.Arguments[0].Type.GenericTypeArguments[0];

                            var declaringType = l.Parameters[0].Type;
                            if (declaringType != sourceType)
                            {
                                throw new Exception($"Ordering is only supported on root document type {sourceType.Name}");
                            }

                            if (node.Method.Name == nameof(Queryable.OrderByDescending) ||
                                node.Method.Name == nameof(Queryable.ThenByDescending))
                            {
                                _visitMode = VisitLinqLambdaMode.OrderByDescending;
                            }
                            else
                            {
                                _visitMode = VisitLinqLambdaMode.OrderBy;
                            }

                            _activeParameters.Add(l.Parameters[0], apiCaller);

                            Visit(l.Body);

                            _visitMode = VisitLinqLambdaMode.Undefined;
                            _activeParameters.Remove(l.Parameters[0]);

                            // We remove here the OrderBy method call from the expression tree,
                            // because the ordering is done by the API.
                            return source;
                        }
                    }
                    break;
                #region Aggregation functions
                case nameof(Queryable.Count):
                case nameof(Queryable.LongCount):
                case nameof(Queryable.Sum):
                case nameof(Queryable.Average):
                case nameof(Queryable.Min):
                case nameof(Queryable.Max):
                    {
                        var source = Visit(node.Arguments[0]);
                        Debug.Assert(source != null);
                        if (!TryGetApiCaller(node.Arguments[0], out var apiCaller))
                        {
                            // ... the source was not a ApiQueryable and is not
                            // part of the API. So we just do nothing and pass it
                        }
                        else if (node.Arguments.Count == 1)
                        {
                            string function = node.Method.Name switch
                            {
                                nameof(Queryable.LongCount) => "count",
                                nameof(Queryable.Count) => "count",
                                nameof(Queryable.Sum) => "sum",
                                nameof(Queryable.Average) => "avg",
                                nameof(Queryable.Min) => "min",
                                nameof(Queryable.Max) => "max",
                                _ => throw new ArgumentOutOfRangeException()
                            };
                            apiCaller.QueryParameters.Aggregate = function;

                            // We remove here the aggregation method call from the expression tree,
                            // because the aggregation is done by the API.
                            return source;
                        }
                        else
                        {
                            throw new NotSupportedException($"Unsupported {node.Method.DeclaringType?.FullName}.{node.Method.Name} signature. The method can only be used without parameters.");
                        }
                    }
                    break;
                #endregion
                case nameof(Queryable.Take):
                    {
                        var source = Visit(node.Arguments[0]);
                        Debug.Assert(source != null);
                        if (!TryGetApiCaller(node.Arguments[0], out var apiCaller))
                        {
                            // ... the source was not a ApiQueryable and is not
                            // part of the API. So we just do nothing and pass it
                        }
                        else if (node.Arguments[1] is ConstantExpression c)
                        {
                            if (c.Type != typeof(int))
                                throw new NotSupportedException(
                                    "The parameter for Queryable.Take must be a constant integer expression");
                            apiCaller.QueryParameters.Limit = (int)c.Value;

                            // We remove here the Take method call from the expression tree,
                            // because the Take is done by the API.
                            return source;
                        }
                        else
                        {
                            throw new Exception("Format for Take expression not supported.");
                        }
                    }
                    break;
                case nameof(Queryable.Skip):
                    {
                        var source = Visit(node.Arguments[0]);
                        Debug.Assert(source != null);
                        if (!TryGetApiCaller(node.Arguments[0], out var apiCaller))
                        {
                            // ... the source was not a ApiQueryable and is not
                            // part of the API. So we just do nothing and pass it
                        }
                        else if (node.Arguments[1] is ConstantExpression c)
                        {
                            if (c.Type != typeof(int))
                                throw new NotSupportedException(
                                    "The parameter for Queryable.Skip must be a constant integer expression");
                            apiCaller.QueryParameters.Offset = (int)c.Value;

                            // We remove here the Skip method call from the expression tree,
                            // because the Take is done by the API.
                            return source;
                        }
                        else
                        {
                            throw new Exception("Format for Skip expression not supported.");
                        }
                    }
                    break;
                default:
                    return base.VisitMethodCall(node);
            }
        }
        else if (node.Method.DeclaringType == typeof(QueryableExtensions))
        {
            switch (node.Method.Name)
            {
                case nameof(QueryableExtensions.Include):
                    {
                        //Arg 0: source
                        var source = Visit(node.Arguments[0]);
                        Debug.Assert(source != null);
                        if (!TryGetApiCaller(node.Arguments[0], out var apiCaller))
                        {
                            // ... the source was not a ApiQueryable and is not
                            // part of the API. So we just do nothing and pass it
                        }
                        else if (node.Arguments.Count == 2)
                        {
                            if (node.Arguments[1] is UnaryExpression u && u.Operand is LambdaExpression l)
                            {
                                _visitMode = VisitLinqLambdaMode.Include;
                                _activeParameters.Add(l.Parameters[0], apiCaller);

                                // Starts a new include chain, frees up the old include chain when multiple include chains are used

                                _includeChain = new StringBuilder();

                                Visit(l.Body);

                                var newIncludeChain = new StringBuilder(_includeChain.ToString());
                                _includeChain.Append(".*");
                                if (!_includeChains.TryGetValue(apiCaller, out _includeChainList))
                                {
                                    _includeChainList = new List<string>();
                                    _includeChains.Add(apiCaller, _includeChainList);
                                }
                                _includeChainList.Add(_includeChain.ToString());
                                _includeChain = newIncludeChain;

                                _visitMode = VisitLinqLambdaMode.Undefined;
                                _activeParameters.Remove(l.Parameters[0]);

                                // We remove here the Include method call from the expression tree,
                                // because the Include is done by the API.
                                return source;
                            }
                        }
                        else
                        {
                            throw new NotSupportedException(
                                $"Unsupported {node.Method.DeclaringType.FullName}.{node.Method.Name} signature.");
                        }
                    }
                    break;
                case nameof(QueryableExtensions.ThenInclude):
                    {
                        //Arg 0: source
                        var source = Visit(node.Arguments[0]);
                        Debug.Assert(source != null);
                        if (!TryGetApiCaller(node.Arguments[0], out var apiCaller))
                        {
                            // ... the source was not a ApiQueryable and is not
                            // part of the API. So we just do nothing and pass it
                        }
                        else if (node.Arguments.Count == 2)
                        {
                            if (node.Arguments[1] is UnaryExpression u && u.Operand is LambdaExpression l)
                            {
                                _visitMode = VisitLinqLambdaMode.ThenInclude;
                                _activeParameters.Add(l.Parameters[0], apiCaller);
                                Debug.Assert(_includeChainList != null);
                                Debug.Assert(_includeChain != null);
                                Debug.Assert(_includeChain.Length > 0);

                                Visit(l.Body);

                                var newIncludeChain = new StringBuilder(_includeChain.ToString());
                                _includeChain.Append(".*");
                                _includeChainList.Add(_includeChain.ToString());
                                _includeChain = newIncludeChain;

                                _visitMode = VisitLinqLambdaMode.Undefined;
                                _activeParameters.Remove(l.Parameters[0]);

                                // We remove here the Include method call from the expression tree,
                                // because the Include is done by the API.
                                return source;
                            }
                        }
                        else
                        {
                            throw new NotSupportedException(
                                $"Unsupported {node.Method.DeclaringType.FullName}.{node.Method.Name} signature.");
                        }
                    }
                    break;
                case nameof(QueryableExtensions.Search):
                    {
                        //Arg 0: source
                        var source = Visit(node.Arguments[0]);
                        Debug.Assert(source != null);
                        if (!TryGetApiCaller(node.Arguments[0], out var apiCaller))
                        {
                            // ... the source was not a ApiQueryable and is not
                            // part of the API. So we just do nothing and pass it
                        }
                        else if (node.Arguments.Count == 2 && node.Arguments[1] is ConstantExpression c)
                        {
                            if (c.Type != typeof(string))
                                throw new NotSupportedException(
                                    "The parameter for QueryableExtensions.Search must be a constant string expression");
                            apiCaller.QueryParameters.Search = (string?)c.Value;

                            // We remove here the Search method call from the expression tree,
                            // because the Take is done by the API.
                            return source;
                        }
                        else
                        {
                            throw new NotSupportedException(
                                $"Unsupported {node.Method.DeclaringType.FullName}.{node.Method.Name} signature.");
                        }
                    }
                    break;
                default:
                    return base.VisitMethodCall(node);
            }
        }

        return base.VisitMethodCall(node);
    }

    #endregion

    #region Transformers

    /// <summary>
    /// Transforms a constant expression to a <see cref="ApiQueryable{T}"/> class to a
    /// constant instance of a <see cref="QueryablePagedEnumerable{T}"/> class.
    /// </summary>
    /// <param name="node"></param>
    /// <returns></returns>
    private ConstantExpression TransformApiQueryableExpression(ConstantExpression node)
    {
        // data model type
        var type = node.Type.GenericTypeArguments[0];

        if (!_mapFromToApiCallers.TryGetValue(node, out var apiCaller))
        {
            var apiPagedEnumerableType = typeof(QueryablePagedEnumerable<>).MakeGenericType(type);
            apiCaller = (IApiCaller)Activator.CreateInstance(apiPagedEnumerableType, _apiClient, _path, _parametersCallback);

            _mapFromToApiCallers.Add(node, apiCaller);
        }

        switch (_expectedTypeMode)
        {
            case QueryableTypeMode.Enumerable:
                return Expression.Constant(apiCaller.AsQueryable());
            case QueryableTypeMode.AsyncEnumerable:
                return Expression.Constant(apiCaller);
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private void TransformConstant(StringBuilder stringBuilder, ConstantExpression node)
    {
        object? value = node.Value;
        Type type = node.Type;

        if (value != null)
        {
            var valueType = value.GetType();

            if (valueType.IsValueType && type != valueType)
            {
                // A value type is converted in the constant expression. For identifying the
                // type, we force to use the actual constant type here. This is necessary
                // when e.g. a value type is passed to a object parameter of a function.
                type = valueType;
            }
        }

        if (value == null)
        {
            stringBuilder.Append("null");
            return;
        }

        if (type.IsGenericType &&
            type.GetGenericTypeDefinition() == typeof(Nullable<>))
        {
            // ReSharper disable once PossibleNullReferenceException
#pragma warning disable CS8602
            value = type.GetProperty(nameof(Nullable<int>.Value)).GetMethod.Invoke(
                value, Array.Empty<object>());
#pragma warning restore CS8602
            Debug.Assert(value != null);
            type = Nullable.GetUnderlyingType(type) ?? throw new NotSupportedException("Could not get type of Nullable<> during visiting constant expression");
        }

        if (type == typeof(string))
        {
            stringBuilder.Append("\"");
            stringBuilder.Append(node.Value);
            stringBuilder.Append("\"");
        }
        else if (type == typeof(Guid))
        {
            stringBuilder.Append("\"");
            stringBuilder.Append(value);
            stringBuilder.Append("\"");
        }
        else if (type == typeof(DateTime))
        {
            stringBuilder.Append("\"");
#pragma warning disable CS8605
            DateTime valueDateTime = ((DateTime)value);
#pragma warning restore CS8605
            if (valueDateTime != valueDateTime.Date)
            {
                stringBuilder.Append(valueDateTime.ToString("O", CultureInfo.InvariantCulture));
            }
            else
            {
                stringBuilder.Append(valueDateTime.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
            }
            stringBuilder.Append("\"");
        }
        else if (type == typeof(DateTimeOffset))
        {
            stringBuilder.Append("\"");
#pragma warning disable CS8605
            stringBuilder.Append(((DateTimeOffset)value).ToString("O", CultureInfo.InvariantCulture));
#pragma warning restore CS8605
            stringBuilder.Append("\"");
        }
#if NET6_0_OR_GREATER
        else if (type == typeof(DateOnly))
        {
            stringBuilder.Append("\"");
#pragma warning disable CS8605
            stringBuilder.Append(((DateOnly)value).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
#pragma warning restore CS8605
            stringBuilder.Append("\"");
        }
#endif
        else if (type == typeof(bool))
        {
#pragma warning disable CS8602
            stringBuilder.Append(value.ToString().ToLowerInvariant());
#pragma warning restore CS8602
        }
        else if (type.IsValueType)
        {
            var isNumberType = TypeHelper.IsNumberType(type);
            if (!isNumberType)
                stringBuilder.Append('"');

            if (value is IFormattable formattable)
            {
                // avoid having culture-specific strings in JSON object
                stringBuilder.Append(formattable.ToString(null, CultureInfo.InvariantCulture));
            }

            if (!isNumberType)
                stringBuilder.Append('"');
        }
        else
        {
            throw new NotSupportedException($"Constant expression type {node.Type.FullName} cannot be passed in a where clause.");
        }
    }

    #endregion
}
