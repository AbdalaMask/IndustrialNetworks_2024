using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace NetStudio.Common.Files;

public class FileTypeJsonConverter : JsonConverter<FileType>
{
	public override FileType Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		return (FileType)Enum.Parse(typeof(FileType), reader.GetString());
	}

	public override void Write(Utf8JsonWriter writer, FileType value, JsonSerializerOptions options)
	{
		writer.WriteStringValue(value.ToString());
	}
}
