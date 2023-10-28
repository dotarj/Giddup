// Copyright (c) Arjen Post. See LICENSE in the project root for license information.

namespace Giddup.Infrastructure.Services;

public class ReviewerService : IReviewerService
{
    public Task<bool> IsValidReviewer(Guid userId)
    {
        // This method will check whether the given user ID is a valid reviewer.
        return Task.FromResult(true);
    }
}
