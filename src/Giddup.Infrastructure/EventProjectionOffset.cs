// Copyright (c) Arjen Post. See LICENSE in the project root for license information.

using System.ComponentModel.DataAnnotations;

namespace Giddup.Infrastructure;

public class EventProjectionOffset
{
    [Key]
    public string AggregateType { get; set; } = string.Empty;

    public long Value { get; set; }
}
