// Copyright (c) Arjen Post. See LICENSE in the project root for license information.

using Giddup.ApplicationCore.Domain;

namespace Giddup.Infrastructure;

public class BranchService : IBranchService
{
    public Task<bool> IsExistingBranch(BranchName branch)
    {
        // This method will check whether the given branch exists.
        return Task.FromResult(true);
    }
}
