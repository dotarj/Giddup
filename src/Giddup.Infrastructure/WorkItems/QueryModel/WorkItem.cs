// Copyright (c) Arjen Post. See LICENSE in the project root for license information.

using System.ComponentModel.DataAnnotations;
using Giddup.Infrastructure.PullRequests.QueryModel.Models;

namespace Giddup.Infrastructure.WorkItems.QueryModel;

public class WorkItem
{
    public Guid Id { get; set; }

    [StringLength(256)]
    public string Title { get; set; } = string.Empty;

    public List<PullRequest> PullRequests { get; set; } = new();
}
