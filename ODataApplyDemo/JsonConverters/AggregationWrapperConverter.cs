using System.Text.Json;
using System.Text.Json.Serialization;

namespace ODataApplyDemo.JsonConverters;

public class AggregationWrapperConverter : JsonConverter<object>
{
    public override object Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        throw new NotImplementedException();
    }

    public override void Write(Utf8JsonWriter writer, object obj, JsonSerializerOptions options)
    {
        var groupByContainer = obj.GetType().GetProperty("GroupByContainer")?.GetValue(obj, null);
        var container = obj.GetType().GetProperty("Container")?.GetValue(obj, null);

        if (groupByContainer is null && container is null) return;
        
        writer.WriteStartObject();

        if (groupByContainer is not null)
        {
            WriteContainer(writer, groupByContainer);
        }

        if (container != null)
        {
            WriteContainer(writer, container);
        }

        writer.WriteEndObject();
    }

    private static void WriteContainer(Utf8JsonWriter writer, object container)
    {
        var name = container.GetType().GetProperty("Name")?.GetValue(container, null)?.ToString();
        var value = container.GetType().GetProperty("Value")?.GetValue(container, null);

        if (name is not null && value is not null)
        {
            var dictionary = new Dictionary<string, object>
            {
                { name, value }
            };
            var jsonDictionary = JsonSerializer.Serialize(dictionary);

            using var jsonDoc = JsonDocument.Parse(jsonDictionary);
            foreach (var element in jsonDoc.RootElement.EnumerateObject())
            {
                writer.WritePropertyName(element.Name);
                switch (element.Value.ValueKind)
                {
                    case JsonValueKind.Number:
                        writer.WriteNumberValue(element.Value.GetDecimal());
                        break;
                    case JsonValueKind.String:
                        writer.WriteStringValue(element.Value.GetString());
                        break;
                    case JsonValueKind.True:
                    case JsonValueKind.False:
                        writer.WriteBooleanValue(element.Value.GetBoolean());
                        break;
                    case JsonValueKind.Null:
                        writer.WriteNullValue();
                        break;
                    case JsonValueKind.Object:
                    case JsonValueKind.Array:
                        throw new JsonException($"Cannot write complex nested types: {element.Value.ValueKind}");
                    default:
                        throw new JsonException($"Unhandled ValueKind: {element.Value.ValueKind}");
                }
            }
        }
    }
}