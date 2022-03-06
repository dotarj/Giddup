// Copyright (c) Arjen Post. See LICENSE in the project root for license information.

using Giddup.Domain.PullRequests;
using Xunit;

namespace Giddup.Domain.Tests.PullRequests;

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
        var result = BranchName.Create(value);

        // Assert
        Assert.True(result.TryPickT0(out var error, out _));
        _ = Assert.IsType<InvalidBranchNameError>(error);
    }
}
