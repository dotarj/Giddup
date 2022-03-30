// Copyright (c) Arjen Post. See LICENSE in the project root for license information.

using System.Text.Json;
using System.Text.Json.Serialization;
using Giddup.Domain.PullRequests;

namespace Giddup.Infrastructure.JsonConverters;

public class BranchNameJsonConverter : JsonConverter<BranchName>
{
    public override BranchName Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var value = reader.GetString();

        if (value == null)
        {
            throw new JsonException("Error occurred");
        }

        if (!BranchName.TryCreate(value, out var branchName, out _))
        {
            throw new JsonException("Error occurred");
        }

        return branchName;
    }

    public override void Write(Utf8JsonWriter writer, BranchName value, JsonSerializerOptions options)
        => writer.WriteStringValue(value.ToString());
}
