namespace Bot.Converters
{
	internal sealed class EnumConverter : JsonConverter<object>
	{
		public override bool CanConvert(Type typeToConvert)
		{
			return typeToConvert.IsEnum;
		}

		public override object? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			return Enum.TryParse(typeToConvert, reader.GetString(), out object? result) ? result : null;
		}

		public override void Write(Utf8JsonWriter writer, object value, JsonSerializerOptions options)
		{
			writer.WriteStringValue(value.ToString());
		}
	}
}
