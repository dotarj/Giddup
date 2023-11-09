// Copyright (c) Arjen Post. See LICENSE in the project root for license information.

using Giddup.ApplicationCore.Domain.PullRequests;

namespace Giddup.Infrastructure.PullRequests.QueryModel.Models;

public class OptionalReviewer
{
    public Guid Id { get; set; }

    public ReviewerFeedback Feedback { get; set; }
}
