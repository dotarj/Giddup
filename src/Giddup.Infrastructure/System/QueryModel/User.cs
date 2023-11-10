// Copyright (c) Arjen Post. See LICENSE in the project root for license information.

namespace Giddup.Infrastructure.System.QueryModel;

public class User
{
    public Guid Id { get; set; }

    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;
}
