// Copyright (c) Arjen Post. See LICENSE in the project root for license information.

using System.Text.Json;
using System.Text.Json.Serialization;
using Giddup.Domain.PullRequests;

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

        var result = Title.Create(value);

        if (result.TryPickT0(out _, out var title))
        {
            throw new JsonException("Error occurred");
        }

        return title;
    }

    public override void Write(Utf8JsonWriter writer, Title value, JsonSerializerOptions options)
        => writer.WriteStringValue(value.ToString());
}
