using System.Text.Json;
using System.Text.Json.Serialization;

namespace Bedrock.Infrastructure.Helpers
{
    public class DateTimeUtcConverter : JsonConverter<DateTime>
    {
        public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            // 读取时，我们假设 JSON 里的字符串是 UTC（带Z），所以直接指定为 Utc
            return DateTime.SpecifyKind(reader.GetDateTime(), DateTimeKind.Utc);
        }

        public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
        {
            // 写入时，我们假设 value 的数值就是 UTC，只是 Kind 可能不对
            // 所以我们不进行时区换算，只修正 Kind 并格式化
            var utcValue = DateTime.SpecifyKind(value, DateTimeKind.Utc);
            writer.WriteStringValue(utcValue.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"));
        }
    }
}
