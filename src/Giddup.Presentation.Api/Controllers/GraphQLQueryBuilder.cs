// Copyright (c) Arjen Post. See LICENSE in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Text;
using HotChocolate.Language;

namespace Giddup.Presentation.Api.Controllers;

public class GraphQLQueryBuilder<T>
{
    private readonly string _queryType;
    private readonly string _fields;

    private string _paging = string.Empty;
    private string _order = string.Empty;
    private StringBuilder? _where;

    private GraphQLQueryBuilder(string queryType, string fields)
    {
        _queryType = queryType;
        _fields = fields;
    }

    public static GraphQLQueryBuilder<T> Create(string queryType, string fields)
        => new(queryType, fields);

    public GraphQLQueryBuilder<T> WithOffsetPaging(int skip, int take)
    {
        _paging = $"skip: {skip}, take: {take},";

        return this;
    }

    public GraphQLQueryBuilder<T> WithSorting(string order)
    {
        _order = order;

        return this;
    }

    public GraphQLQueryBuilder<T> WithWhereEquals<TValue>(Expression<Func<T, TValue>> expression, TValue value)
        => Where(expression, value, "eq");

    public bool TryCreateQuery([NotNullWhen(true)] out DocumentNode? query)
    {
        try
        {
            query = Utf8GraphQLParser.Parse($"query {{ {_queryType}({_paging} order: {{ {_order} }}, where: {{ {_where} }}) {{ {_fields} }} }}");
        }
        catch (SyntaxException)
        {
            query = null;

            return false;
        }

        return true;
    }

    private GraphQLQueryBuilder<T> Where<TValue>(Expression<Func<T, TValue>> expression, TValue value, string operation)
    {
        if (value != null)
        {
            _where ??= new StringBuilder();

            var propertyNames = GetPropertyPath(expression)
                .Reverse()
                .Select(StringExtensions.ToLowerFirstChar)
                .ToList();

            foreach (var propertyName in propertyNames)
            {
                _ = _where.Append(propertyName);
                _ = _where.Append(": { ");
            }

            _ = _where.Append(operation);
            _ = _where.Append(": \"");
            _ = _where.Append(value);
            _ = _where.Append("\" ");

            foreach (var propertyName in propertyNames)
            {
                _ = _where.Append("} ");
            }
        }

        return this;
    }

    private IEnumerable<string> GetPropertyPath<TValue>(Expression<Func<T, TValue>> expression)
    {
        if (expression.Body is MemberExpression memberExpression)
        {
            yield return memberExpression.Member.Name;

            yield break;
        }

        if (expression.Body is not UnaryExpression unaryExpression)
        {
            throw new InvalidOperationException();
        }

        if (unaryExpression.Operand is not MemberExpression nestedMemberExpression)
        {
            throw new InvalidOperationException();
        }

        yield return nestedMemberExpression.Member.Name;

        while (true)
        {
            if (nestedMemberExpression.Expression is ParameterExpression)
            {
                break;
            }

            if (nestedMemberExpression.Expression is not MemberExpression parentMemberExpression)
            {
                throw new InvalidOperationException();
            }

            yield return parentMemberExpression.Member.Name;

            nestedMemberExpression = parentMemberExpression;
        }
    }
}
