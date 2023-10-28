// Copyright (c) Arjen Post. See LICENSE in the project root for license information.

using System.Text.Json;
using System.Text.Json.Serialization;
using Giddup.ApplicationCore.Domain.PullRequests;

namespace Giddup.Infrastructure.JsonConverters;

public class TitleJsonConverter : JsonConverter<Title>
{
    public override Title Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var value = reader.GetString();

        if (value == null)
        {
            throw new JsonException("Error occurred");
        }

        if (!Title.TryCreate(value, out var title, out _))
        {
            throw new JsonException("Error occurred");
        }

        return title;
    }

    public override void Write(Utf8JsonWriter writer, Title value, JsonSerializerOptions options)
        => writer.WriteStringValue(value.ToString());
}
