// Copyright (c) Arjen Post. See LICENSE in the project root for license information.

using System.ComponentModel.DataAnnotations;

namespace Giddup.Infrastructure;

public class Event
{
    public long Offset { get; set; }

    public Guid AggregateId { get; set; }

    [StringLength(128)]
    public string AggregateType { get; set; } = string.Empty;

    public long AggregateVersion { get; set; }

    [StringLength(128)]
    public string Type { get; set; } = string.Empty;

    public string Data { get; set; } = string.Empty;
}
