// Copyright (c) Arjen Post. See LICENSE in the project root for license information.

using System.ComponentModel.DataAnnotations;

namespace Giddup.Infrastructure.System.QueryModel;

public class User
{
    public Guid Id { get; set; }

    [StringLength(64)]
    public string FirstName { get; set; } = string.Empty;

    [StringLength(64)]
    public string LastName { get; set; } = string.Empty;
}
