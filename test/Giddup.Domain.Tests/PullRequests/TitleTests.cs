// Copyright (c) Arjen Post. See LICENSE in the project root for license information.

using Giddup.Domain.PullRequests;
using Xunit;

namespace Giddup.Domain.Tests.PullRequests;

public class TitleTests
{
    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void Create_InvalidValue_ReturnsTitleEmptyOrWhitespaceError(string value)
    {
        // Act
        var result = Title.Create(value);

        // Assert
        Assert.True(result.TryPickT0(out var error, out _));
        _ = Assert.IsType<TitleEmptyOrWhitespaceError>(error);
    }
}
