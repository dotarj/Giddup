// Copyright (c) Arjen Post. See LICENSE in the project root for license information.

namespace Giddup.Infrastructure;

public interface IReviewerService
{
    Task<bool> IsValidReviewer(Guid userId);
}
