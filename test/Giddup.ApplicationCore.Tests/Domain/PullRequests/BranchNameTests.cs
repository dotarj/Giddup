// Copyright (c) Arjen Post. See LICENSE in the project root for license information.

using Giddup.ApplicationCore.Domain.PullRequests;
using Xunit;

namespace Giddup.ApplicationCore.Tests.Domain.PullRequests;

public class BranchNameTests
{
    [Theory]
    [InlineData("foo")]
    [InlineData("refs/heads/.foo")]
    [InlineData("refs/heads/.foo/bar")]
    [InlineData("refs/heads/foo/.bar")]
    [InlineData("refs/heads/foo/bar.lock")]
    [InlineData("refs/heads/foo.lock/bar")]
    [InlineData("refs/heads/foo.lock")]
    [InlineData("refs/heads/foo..bar")]
    [InlineData("refs/heads/foo ")]
    [InlineData("refs/heads/foo~")]
    [InlineData("refs/heads/foo^")]
    [InlineData("refs/heads/foo:")]
    [InlineData("refs/heads/foo?")]
    [InlineData("refs/heads/foo*")]
    [InlineData("refs/heads/foo[")]
    [InlineData("refs/heads/foo\\")]
    [InlineData("refs/heads//foo")]
    [InlineData("refs/heads/foo/")]
    [InlineData("refs/heads/foo//bar")]
    [InlineData("refs/heads/foo.")]
    [InlineData("refs/heads/foo@{")]
    [InlineData("@")]
    public void Create_InvalidValue_ReturnsInvalidBranchNameError(string value)
    {
        // Act
        var result = BranchName.TryCreate(value, out _, out var error);

        // Assert
        Assert.False(result);
        _ = Assert.IsType<InvalidBranchNameError>(error);
    }
}
