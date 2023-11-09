// Copyright (c) Arjen Post. See LICENSE in the project root for license information.

namespace Giddup.Infrastructure;

public class EventProjectionOffset
{
    public string AggregateType { get; set; } = string.Empty;

    public long Value { get; set; }
}
