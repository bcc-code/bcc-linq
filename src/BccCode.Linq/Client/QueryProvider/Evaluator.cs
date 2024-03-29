﻿using System.Linq.Expressions;

namespace BccCode.Linq.Client;

/// <summary>
///
/// Reference: <a href="https://msdn.microsoft.com/en-us/library/bb546158.aspx">Walkthrough: Creating an IQueryable LINQ Provider</a>.
/// </summary>
internal class Evaluator
{
    /// <summary>
    /// Performs evaluation and replacement of independent sub-trees
    /// </summary>
    /// <param name="expression">The root of the expression tree.</param>
    /// <param name="fnCanBeEvaluated">A function that decides whether a given expression node can be part of the local function.</param>
    /// <returns>A new tree with sub-trees evaluated and replaced.</returns>
    public static Expression PartialEval(Expression expression, Func<Expression, bool> fnCanBeEvaluated)
    {
        return new SubtreeEvaluator(new Nominator(fnCanBeEvaluated).Nominate(expression)).Eval(expression);
    }

    /// <summary>
    /// Performs evaluation and replacement of independent sub-trees 
    /// </summary>
    /// <param name="expression">The root of the expression tree.</param>
    /// <returns>A new tree with sub-trees evaluated and replaced.</returns> 
    public static Expression PartialEval(Expression expression)
    {
        return PartialEval(expression, CanBeEvaluatedLocally);
    }

    private static bool CanBeEvaluatedLocally(Expression expression)
    {
        return expression.NodeType != ExpressionType.Parameter;
    }

    /// <summary>
    /// Evaluates and replaces sub-trees when first candidate is reached (top-down) 
    /// </summary>
    private class SubtreeEvaluator : ExpressionVisitor
    {
        private readonly HashSet<Expression> _candidates;

        internal SubtreeEvaluator(HashSet<Expression> candidates)
        {
            this._candidates = candidates;
        }

        internal Expression Eval(Expression exp)
        {
            return this.Visit(exp);
        }

        public override Expression Visit(Expression exp)
        {
            if (exp == null)
                return null;

            if (this._candidates.Contains(exp))
            {
                return this.Evaluate(exp);
            }

            return base.Visit(exp);
        }

        private Expression Evaluate(Expression e)
        {
            if (e.NodeType == ExpressionType.Constant)
                return e;

            LambdaExpression lambda = Expression.Lambda(e);
            Delegate fn = lambda.Compile();
            return Expression.Constant(fn.DynamicInvoke(null), e.Type);
        }
    }

    /// <summary>
    /// Performs bottom-up analysis to determine which nodes can possibly
    /// be part of an evaluated sub-tree.
    /// </summary>
    private class Nominator : ExpressionVisitor
    {
        private readonly Func<Expression, bool> _fnCanBeEvaluated;
        private readonly HashSet<Expression> _candidates = new();
        private bool _cannotBeEvaluated;

        internal Nominator(Func<Expression, bool> fnCanBeEvaluated)
        {
            this._fnCanBeEvaluated = fnCanBeEvaluated;
        }

        internal HashSet<Expression> Nominate(Expression expression)
        {
            _candidates.Clear();
            this.Visit(expression);
            return this._candidates;
        }

        public override Expression Visit(Expression expression)
        {
            if (expression == null)
                return null;

            bool saveCannotBeEvaluated = this._cannotBeEvaluated;
            this._cannotBeEvaluated = false;
            base.Visit(expression);
            if (!this._cannotBeEvaluated)
            {
                if (this._fnCanBeEvaluated(expression))
                {
                    this._candidates.Add(expression);
                }
                else
                {
                    this._cannotBeEvaluated = true;
                }
            }

            this._cannotBeEvaluated |= saveCannotBeEvaluated;

            return expression;
        }
    }
}
