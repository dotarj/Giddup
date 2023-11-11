// Copyright (c) Arjen Post. See LICENSE in the project root for license information.

using Giddup.Infrastructure.System.QueryModel;
using HotChocolate.Data.Filters;

namespace Giddup.Presentation.Api.Queries.FilterInputTypes;

public class UserFilterInputType : FilterInputType<User>
{
    protected override void Configure(IFilterInputTypeDescriptor<User> descriptor)
    {
        _ = descriptor.Field(user => user.Id);
    }
}
