// Copyright (c) Arjen Post. See LICENSE in the project root for license information.

using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Giddup.ApplicationCore.Domain.PullRequests;
using Giddup.Infrastructure.System.QueryModel;
using Giddup.Infrastructure.WorkItems.QueryModel;
using HotChocolate;

namespace Giddup.Infrastructure.PullRequests.QueryModel.Models;

public class PullRequest
{
    public Guid Id { get; set; }

    public DateTime CreatedAt { get; set; }

    public User CreatedBy { get; set; } = null!;

    [GraphQLIgnore]
    [JsonIgnore]
    public Guid CreatedById { get; set; }

    [StringLength(256)]
    public string SourceBranch { get; set; } = string.Empty;

    [StringLength(256)]
    public string TargetBranch { get; set; } = string.Empty;

    [StringLength(256)]
    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public PullRequestStatus Status { get; set; }

    public AutoCompleteMode AutoCompleteMode { get; set; }

    public CheckForLinkedWorkItemsMode CheckForLinkedWorkItemsMode { get; set; }

    public List<OptionalReviewer> OptionalReviewers { get; set; } = new();

    public List<RequiredReviewer> RequiredReviewers { get; set; } = new();

    public List<WorkItem> WorkItems { get; set; } = new();
}
