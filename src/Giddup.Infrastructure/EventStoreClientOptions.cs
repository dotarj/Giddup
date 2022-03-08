// Copyright (c) Arjen Post. See LICENSE in the project root for license information.

using System.ComponentModel.DataAnnotations;

namespace Giddup.Infrastructure;

public class EventStoreClientOptions
{
    [Required]
    public string ConnectionString { get; set; } = string.Empty;
}
