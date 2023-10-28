// Copyright (c) Arjen Post. See LICENSE in the project root for license information.

using Giddup.ApplicationCore.Domain;

namespace Giddup.Infrastructure;

public interface IBranchService
{
    Task<bool> IsExistingBranch(BranchName branch);
}
