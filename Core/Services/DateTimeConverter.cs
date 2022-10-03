using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Xml;

namespace Gov.KansasDCF.Cse.App;
public class DateTimeConverter: JsonConverter<DateTime>
{
  public override DateTime Read(
    ref Utf8JsonReader reader,
    Type typeToConvert,
    JsonSerializerOptions options) =>
    XmlConvert.ToDateTime(
      reader.GetString(),
      XmlDateTimeSerializationMode.Unspecified);

  public override void Write(
    Utf8JsonWriter writer,
    DateTime value,
    JsonSerializerOptions options)
  {
    if(value.TimeOfDay.TotalSeconds != 0)
    {
      writer.WriteStringValue(value.ToString("yyyy-MM-ddTHH:mm:ss.ffffff"));
    }
    else
    {
      writer.WriteStringValue(value.ToString("yyyy-MM-dd"));
    }
  }
}
