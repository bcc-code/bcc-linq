using System.Linq.Expressions;
using RuleFilterParser;
using RuleToLinqParser.Tests.Helpers;

namespace RuleToLinqParser.Tests;

public class OperandToExpressionResolverTests
{
    [Theory]
    [InlineData("strProp", "test")]
    [InlineData("strProp", "123")]
    [InlineData("strProp", "123.00")]
    [InlineData("numberIntergerProp", 123)]
    [InlineData("numberIntergerProp", 123.00)]
    [InlineData("numberIntergerProp", "123")]
    [InlineData("numberIntergerProp", "123.00")]
    [InlineData("numberDoubleProp", 123)]
    [InlineData("numberDoubleProp", 123.00)]
    [InlineData("numberDoubleProp", "123")]
    [InlineData("numberDoubleProp", "123.00")]
    [InlineData("booleanProp", true)]
    [InlineData("booleanProp", "true")]
    [InlineData("booleanProp", 1)]
    [InlineData("booleanProp", "1")]
    [InlineData("anyDate", "2017-3-1")]
    public void should_convert_values_correctly(string propertyName, object compareValue)
    {
        var parameter = Expression.Parameter(typeof(TestClass), "x");
        var prop = Expression.Property(parameter, propertyName);

        var exp = OperandToExpressionResolver.GetExpressionForRule(prop, "_eq", compareValue);
        var binaryExpression = (BinaryExpression)exp;
        Assert.Equal(binaryExpression.Left.Type, binaryExpression.Right.Type);
    }

    [Fact]
    public void should_throw_exception_when_operand_is_wrong()
    {
        var parameter = Expression.Parameter(typeof(TestClass), "x");
        var prop = Expression.Property(parameter, "strProp");

        Assert.Throws<ArgumentOutOfRangeException>(() => OperandToExpressionResolver.GetExpressionForRule(
            prop, "_wrong", ""));
    }

    [Fact]
    public void should_return_equal_expression()
    {
        var parameter = Expression.Parameter(typeof(TestClass), "x");
        var prop = Expression.Property(parameter, "strProp");

        var exp = OperandToExpressionResolver.GetExpressionForRule(
            prop, "_eq", "test");

        Assert.Equal(ExpressionType.Equal, exp.NodeType);
    }

    [Fact]
    public void should_return_not_equal_expression()
    {
        var parameter = Expression.Parameter(typeof(TestClass), "x");
        var prop = Expression.Property(parameter, "strProp");

        var exp = OperandToExpressionResolver.GetExpressionForRule(
            prop, "_neq", "test");

        Assert.Equal(ExpressionType.NotEqual, exp.NodeType);
    }

    [Fact]
    public void should_return_contains_expression()
    {
        var parameter = Expression.Parameter(typeof(TestClass), "x");
        var prop = Expression.Property(parameter, "strProp");

        var exp = OperandToExpressionResolver.GetExpressionForRule(
            prop, "_contains", "test");
        var methodCallExpression = (MethodCallExpression)exp;

        Assert.Equal("Contains", methodCallExpression.Method.Name);
    }

    [Fact]
    public void should_return_not_contains_expression()
    {
        var parameter = Expression.Parameter(typeof(TestClass), "x");
        var prop = Expression.Property(parameter, "strProp");

        var exp = OperandToExpressionResolver.GetExpressionForRule(
            prop, "_ncontains", "test");
        var binaryExpression = (UnaryExpression)exp;

        Assert.Equal(ExpressionType.Not, binaryExpression.NodeType);
        Assert.Equal("Contains", ((MethodCallExpression)binaryExpression.Operand).Method.Name);
    }

    [Fact]
    public void should_return_startswith_expression()
    {
        var parameter = Expression.Parameter(typeof(TestClass), "x");
        var prop = Expression.Property(parameter, "strProp");

        var exp = OperandToExpressionResolver.GetExpressionForRule(
            prop, "_starts_with", "test");
        var methodCallExpression = (MethodCallExpression)exp;

        Assert.Equal("StartsWith", methodCallExpression.Method.Name);
    }

    [Fact]
    public void should_return_not_startswith_expression()
    {
        var parameter = Expression.Parameter(typeof(TestClass), "x");
        var prop = Expression.Property(parameter, "strProp");

        var exp = OperandToExpressionResolver.GetExpressionForRule(
            prop, "_nstarts_with", "test");
        var binaryExpression = (UnaryExpression)exp;

        Assert.Equal(ExpressionType.Not, binaryExpression.NodeType);
        Assert.Equal("StartsWith", ((MethodCallExpression)binaryExpression.Operand).Method.Name);
    }

    [Fact]
    public void should_return_endswith_expression()
    {
        var parameter = Expression.Parameter(typeof(TestClass), "x");
        var prop = Expression.Property(parameter, "strProp");

        var exp = OperandToExpressionResolver.GetExpressionForRule(
            prop, "_ends_with", "test");
        var methodCallExpression = (MethodCallExpression)exp;

        Assert.Equal("EndsWith", methodCallExpression.Method.Name);
    }

    [Fact]
    public void should_return_not_endswith_expression()
    {
        var parameter = Expression.Parameter(typeof(TestClass), "x");
        var prop = Expression.Property(parameter, "strProp");

        var exp = OperandToExpressionResolver.GetExpressionForRule(
            prop, "_nends_with", "test");
        var binaryExpression = (UnaryExpression)exp;

        Assert.Equal(ExpressionType.Not, binaryExpression.NodeType);
        Assert.Equal("EndsWith", ((MethodCallExpression)binaryExpression.Operand).Method.Name);
    }

    [Fact]
    public void should_return_greater_than_expression()
    {
        var parameter = Expression.Parameter(typeof(TestClass), "x");
        var prop = Expression.Property(parameter, "numberIntergerProp");

        var exp = OperandToExpressionResolver.GetExpressionForRule(
            prop, "_gt", 1);

        Assert.Equal(ExpressionType.GreaterThan, exp.NodeType);
    }

    [Fact]
    public void should_return_greater_than_or_equal_expression()
    {
        var parameter = Expression.Parameter(typeof(TestClass), "x");
        var prop = Expression.Property(parameter, "numberIntergerProp");

        var exp = OperandToExpressionResolver.GetExpressionForRule(
            prop, "_gte", 1);

        Assert.Equal(ExpressionType.GreaterThanOrEqual, exp.NodeType);
    }

    [Fact]
    public void should_return_less_than_expression()
    {
        var parameter = Expression.Parameter(typeof(TestClass), "x");
        var prop = Expression.Property(parameter, "numberIntergerProp");

        var exp = OperandToExpressionResolver.GetExpressionForRule(
            prop, "_lt", 1);

        Assert.Equal(ExpressionType.LessThan, exp.NodeType);
    }

    [Fact]
    public void should_return_less_than_or_equal_expression()
    {
        var parameter = Expression.Parameter(typeof(TestClass), "x");
        var prop = Expression.Property(parameter, "numberIntergerProp");

        var exp = OperandToExpressionResolver.GetExpressionForRule(
            prop, "_lte", 1);

        Assert.Equal(ExpressionType.LessThanOrEqual, exp.NodeType);
    }

    [Fact]
    public void should_return_equal_null_expression()
    {
        var parameter = Expression.Parameter(typeof(TestClass), "x");
        var prop = Expression.Property(parameter, "strProp");

        var exp = OperandToExpressionResolver.GetExpressionForRule(
            prop, "_null", "i am not null");
        var binaryExpression = (BinaryExpression)exp;

        Assert.Equal(ExpressionType.Equal, exp.NodeType);
        Assert.Null(((ConstantExpression)binaryExpression.Right).Value);
    }

    [Fact]
    public void should_return_equal_not_null_expression()
    {
        var parameter = Expression.Parameter(typeof(TestClass), "x");
        var prop = Expression.Property(parameter, "strProp");

        var exp = OperandToExpressionResolver.GetExpressionForRule(
            prop, "_nnull", "i am not null");
        var binaryExpression = (BinaryExpression)exp;

        Assert.Equal(ExpressionType.NotEqual, exp.NodeType);
        Assert.Null(((ConstantExpression)binaryExpression.Right).Value);
    }

    [Fact]
    public void should_return_regex_expression()
    {
        var parameter = Expression.Parameter(typeof(TestClass), "x");
        var prop = Expression.Property(parameter, "strProp");

        var exp = OperandToExpressionResolver.GetExpressionForRule(
            prop, "_regex", "i am not null");
        var methodCallExpression = (MethodCallExpression)exp;

        Assert.Equal("IsMatch", methodCallExpression.Method.Name);
    }

    [Fact]
    public void should_return_array_contains_expression()
    {
        var parameter = Expression.Parameter(typeof(TestClass), "x");
        var prop = Expression.Property(parameter, "stringArrayProp");

        var exp = OperandToExpressionResolver.GetExpressionForRule(
            prop, "_in", new[] { "a", "b" });
        var methodCallExpression = (MethodCallExpression)exp;

        Assert.Equal("Contains", methodCallExpression.Method.Name);
    }

    [Fact]
    public void should_return_array_not_contains_expression()
    {
        var parameter = Expression.Parameter(typeof(TestClass), "x");
        var prop = Expression.Property(parameter, "stringArrayProp");

        var exp = OperandToExpressionResolver.GetExpressionForRule(
            prop, "_nin", new[] { "a", "b" });
        var binaryExpression = (UnaryExpression)exp;

        Assert.Equal(ExpressionType.Not, binaryExpression.NodeType);
        Assert.Equal("Contains", ((MethodCallExpression)binaryExpression.Operand).Method.Name);
    }

    [Fact]
    public void should_return_between_expression()
    {
        var parameter = Expression.Parameter(typeof(TestClass), "x");
        var prop = Expression.Property(parameter, "numberIntergerProp");

        var exp = OperandToExpressionResolver.GetExpressionForRule(
            prop, "_between", (1, 2));
        var binaryExpression = (BinaryExpression)exp;

        Assert.Equal(ExpressionType.AndAlso, exp.NodeType);
        Assert.Equal(ExpressionType.GreaterThanOrEqual, binaryExpression.Left.NodeType);
        Assert.Equal(ExpressionType.LessThanOrEqual, binaryExpression.Right.NodeType);
    }

    [Fact]
    public void should_return_between_expression_for_date_time_tuple()
    {
        var parameter = Expression.Parameter(typeof(TestClass), "x");
        var prop = Expression.Property(parameter, "numberIntergerProp");

        var exp = OperandToExpressionResolver.GetExpressionForRule(
            prop, "_between", new ValueTuple<DateTime, DateTime>(DateTime.UtcNow, DateTime.UtcNow));
        var binaryExpression = (BinaryExpression)exp;

        Assert.Equal(ExpressionType.AndAlso, exp.NodeType);
        Assert.Equal(ExpressionType.GreaterThanOrEqual, binaryExpression.Left.NodeType);
        Assert.Equal(ExpressionType.LessThanOrEqual, binaryExpression.Right.NodeType);
    }

    [Fact]
    public void should_return_not_between_expression()
    {
        var parameter = Expression.Parameter(typeof(TestClass), "x");
        var prop = Expression.Property(parameter, "numberIntergerProp");

        var exp = OperandToExpressionResolver.GetExpressionForRule(
            prop, "_nbetween", (1, 2));
        var binaryExpression = (BinaryExpression)exp;

        Assert.Equal(ExpressionType.AndAlso, exp.NodeType);
        Assert.Equal(ExpressionType.LessThan, binaryExpression.Left.NodeType);
        Assert.Equal(ExpressionType.GreaterThan, binaryExpression.Right.NodeType);
    }

    [Fact]
    public void should_return_array_empty_expression()
    {
        var parameter = Expression.Parameter(typeof(TestClass), "x");
        var prop = Expression.Property(parameter, "stringArrayProp");

        var exp = OperandToExpressionResolver.GetExpressionForRule
            (prop, "_empty", new[] { "a", "b" });
        var binaryExpression = (BinaryExpression)exp;

        Assert.Equal(ExpressionType.Equal, exp.NodeType);
        Assert.Equal(ExpressionType.ArrayLength, binaryExpression.Left.NodeType);
        Assert.Equal(0, ((ConstantExpression)binaryExpression.Right).Value);
    }

    [Fact]
    public void should_return_array_not_empty_expression()
    {
        var parameter = Expression.Parameter(typeof(TestClass), "x");
        var prop = Expression.Property(parameter, "stringArrayProp");

        var exp = OperandToExpressionResolver.GetExpressionForRule(prop, "_nempty", new[] { "a", "b" });
        var binaryExpression = (BinaryExpression)exp;

        Assert.Equal(ExpressionType.GreaterThan, exp.NodeType);
        Assert.Equal(ExpressionType.ArrayLength, binaryExpression.Left.NodeType);
        Assert.Equal(0, ((ConstantExpression)binaryExpression.Right).Value);
    }

    [Fact]
    public void should_return_submitted_expression()
    {
        var parameter = Expression.Parameter(typeof(TestClass), "x");
        var prop = Expression.Property(parameter, "strProp");

        var exp = OperandToExpressionResolver.GetExpressionForRule(
            prop, "_submitted", "i am not null");
        var binaryExpression = (BinaryExpression)exp;

        Assert.Equal(ExpressionType.NotEqual, exp.NodeType);
        Assert.Null(((ConstantExpression)binaryExpression.Right).Value);
    }
}