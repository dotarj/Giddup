// Copyright (c) Arjen Post. See LICENSE in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Text;
using HotChocolate.Language;

namespace Giddup.Presentation.Api.Controllers;

public class GraphQLQueryBuilder<T>
{
    private string _queryType;
    private string _fields;
    private string _paging;
    private string _order;
    private StringBuilder _where = new();

    private GraphQLQueryBuilder(string queryType, string fields, int skip, int take, string order)
    {
        _queryType = queryType;
        _fields = fields;
        _paging = $"skip: {skip}, take: {take},";
        _order = order;
    }

    public static GraphQLQueryBuilder<T> Create(string queryType, string fields, int skip, int take, string order)
        => new(queryType, fields, skip, take, order);

    public GraphQLQueryBuilder<T> WhereEquals<TValue>(Expression<Func<T, TValue>> expression, TValue value)
    {
        if (value != null)
        {
            // where += $"createdBy: {{ id: {{ eq: \"{createdBy.Value}\" }} }} ";
            var propertyNames = GetPropertyPath(expression)
                .Reverse()
                .Select(StringExtensions.ToLowerFirstChar)
                .ToList();

            foreach (var propertyName in propertyNames)
            {
                _ = _where.Append(propertyName);
                _ = _where.Append(": { ");
            }

            _ = _where.Append("eq: \"");
            _ = _where.Append(value);
            _ = _where.Append("\" ");

            foreach (var propertyName in propertyNames)
            {
                _ = _where.Append("} ");
            }
        }

        return this;
    }

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
