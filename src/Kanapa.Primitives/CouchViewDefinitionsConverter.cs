using System;
using System.Linq;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

#if DNXCORE50
using System.Reflection;
#endif

namespace Kanapa.Primitives
{
  public sealed class CouchViewDefinitionsConverter : JsonConverter
  {
    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
      var @object = (IEnumerable<CouchViewDefinition>) value;
      writer.WriteStartObject();
      foreach (var item in @object)
      {
        writer.WritePropertyName(item.Name);
        writer.WriteStartObject();
        if (item.Mapping != null)
        {
          if (string.IsNullOrEmpty(item.Mapping.Map) == false)
          {
            writer.WritePropertyName("map");
            writer.WriteValue(item.Mapping.Map);
          }
          if (string.IsNullOrEmpty(item.Mapping.Reduce) == false)
          {
            writer.WritePropertyName("reduce");
            writer.WriteValue(item.Mapping.Reduce);
          }
        }
        writer.WriteEndObject();
      }
      writer.WriteEndObject();
    }

    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
      var obj = JObject.Load(reader);
      return (from item in obj.Properties()
              let name = item.Name
              let @object = obj[item.Name]
              select new CouchViewDefinition
              {
                Name = name,
                Mapping = new CouchMapReduce
                {
                  Map = @object["map"]?.Value<string>(),
                  Reduce = @object["reduce"]?.Value<string>()
                }
              }
        );
    }

    public override bool CanConvert(Type objectType)
    {
#if DNXCORE50
      var typeInfo = objectType.GetTypeInfo();
      var baseType = typeof(IEnumerable<CouchViewDefinition>).GetTypeInfo();
      return baseType.IsAssignableFrom(typeInfo);
#else
      return typeof(IEnumerable<CouchViewDefinition>).IsAssignableFrom(objectType);
#endif
    }
  }
}