// Copyright (c) Arjen Post. See LICENSE in the project root for license information.

using System.Text.Json.Serialization;
using Giddup.ApplicationCore.Domain.PullRequests;
using Giddup.Infrastructure.System.QueryModel;
using HotChocolate;

namespace Giddup.Infrastructure.PullRequests.QueryModel.Models;

public class OptionalReviewer
{
    [GraphQLIgnore]
    [JsonIgnore]
    public Guid PullRequestId { get; set; }

    public User User { get; set; } = null!;

    [GraphQLIgnore]
    [JsonIgnore]
    public Guid UserId { get; set; }

    public ReviewerFeedback Feedback { get; set; }
}
