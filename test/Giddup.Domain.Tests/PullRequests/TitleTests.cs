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
        var result = Title.TryCreate(value, out _, out var error);

        // Assert
        Assert.False(result);
        _ = Assert.IsType<TitleEmptyOrWhitespaceError>(error);
    }
}
