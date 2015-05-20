using Newtonsoft.Json;

namespace Kanapa.Primitives
{
  public class CouchMapReduce
  {
    [JsonProperty("map", NullValueHandling = NullValueHandling.Ignore)]
    public string Map { get; set; }

    [JsonProperty("reduce", NullValueHandling = NullValueHandling.Ignore)]
    public string Reduce { get; set; }
  }
}