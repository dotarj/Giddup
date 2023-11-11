// Copyright (c) Arjen Post. See LICENSE in the project root for license information.

namespace Giddup.Presentation.Api.Controllers;

public record ListResult<T>(int TotalCount, PageInfo PageInfo, List<T> Items);

public record PageInfo(bool HasPreviousPage, bool HasNextPage);
