using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Kanapa.Tests.Misc;
using Xunit;

namespace Kanapa.Tests
{
  public class ViewDefinitionsConverterTests
  {
    [Fact]
    public void Converter_CanConvert_ReturnsTrueForCorrectData() =>
      Assert.True(GetConverter().CanConvert(typeof(List<ViewDefinition>)));

    [Fact]
    public void Converter_CanConvert_ReturnsFalseForIncorrectData() =>
      Assert.False(GetConverter().CanConvert(typeof(int)));

    [Theory]
    [InlineData(@"{
        ""all"": {
          ""map"": ""function(doc) { if (doc.Type == 'customer')  emit(null, doc) }""
        },
       ""by_lastname"": {
         ""map"": ""function(doc) { if (doc.Type == 'customer')  emit(doc.LastName, doc) }""
       },
       ""total_purchases"": {
         ""map"": ""function(doc) { if (doc.Type == 'purchase')  emit(doc.Customer, doc.Amount) }"",
         ""reduce"": ""function(keys, values) { return sum(values) }""
       }
     }", 3)]
    public void Converter_ReadJson_Correct(string json, int viewCount)
    {
      var converter = GetConverter();
      IEnumerable<ViewDefinition> @object;
      using (var ms = new MemoryStream())
      {
        var bytes = Encoding.UTF8.GetBytes(json);
        ms.Write(bytes, 0, bytes.Length);
        ms.Seek(0, SeekOrigin.Begin);
        using (var reader = new StreamReader(ms))
        {
          using (var jsonTextReader = new JsonTextReader(reader))
          {
            @object = (IEnumerable<ViewDefinition>)converter.ReadJson(jsonTextReader, null, null, null);
          }
        }
      }

      Assert.True(@object.Count() == viewCount);
    }

    [Theory]
    [MemberData("TestCasesForWrite")]
    public void Converter_WriteJson_Correct(TestCaseWithViews @case)
    {
      var converter = GetConverter();
      string json;
      using (var ms = new MemoryStream())
      {
        using (var writer = new StreamWriter(ms))
        {
          using (var jsonWriter = new JsonTextWriter(writer))
          {
            converter.WriteJson(jsonWriter, @case.Items, null);
            jsonWriter.Flush();
            json = Encoding.UTF8.GetString(ms.ToArray());
          }
        }
      }

      Assert.Equal(@case.AdditionalData, json);

    }

    public static IEnumerable<object[]> TestCasesForWrite => new[]
    {
      (new object[]
      {
        new TestCaseWithViews
        {
          AdditionalData = "{\"view0\":{\"map\":\"map0\",\"reduce\":\"reduce0\"},\"view1\":{\"map\":\"map1\"},\"view2\":{\"reduce\":\"reduce2\"},\"view3\":{}}",
          Items = new[]
          {
            new ViewDefinition
            {
              Name = "view0",
              Mapping = new MapReduce
              {
                Map = "map0",
                Reduce = "reduce0"
              }
            },
            new ViewDefinition
            {
              Name = "view1",
              Mapping = new MapReduce
              {
                Map = "map1"
              }
            },
            new ViewDefinition
            {
              Name = "view2",
              Mapping = new MapReduce
              {
                Reduce = "reduce2"
              }
            },
            new ViewDefinition
            {
              Name = "view3"
            }

          }
        }
      })
    };

    private static ViewDefinitionsConverter GetConverter() => new ViewDefinitionsConverter();
  }
}