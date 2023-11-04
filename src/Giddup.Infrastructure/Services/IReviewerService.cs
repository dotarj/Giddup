// Copyright (c) Arjen Post. See LICENSE in the project root for license information.

namespace Giddup.Infrastructure.Services;

public interface IReviewerService
{
    Task<bool> IsValidReviewer(Guid userId);
}
