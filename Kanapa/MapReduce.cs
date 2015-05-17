using Newtonsoft.Json;

namespace Kanapa
{
  public class MapReduce
  {
    [JsonProperty("map", NullValueHandling = NullValueHandling.Ignore)]
    public string Map { get; set; }

    [JsonProperty("reduce", NullValueHandling = NullValueHandling.Ignore)]
    public string Reduce { get; set; }
  }
}