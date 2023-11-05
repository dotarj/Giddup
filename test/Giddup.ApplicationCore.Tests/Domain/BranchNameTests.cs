// Copyright (c) Arjen Post. See LICENSE in the project root for license information.

using Giddup.ApplicationCore.Domain;
using Xunit;

namespace Giddup.ApplicationCore.Tests.Domain;

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
    public void Create_InvalidValue_ReturnsFalse(string value)
    {
        // Act
        var result = BranchName.TryCreate(value, out _);

        // Assert
        Assert.False(result);
    }
}
