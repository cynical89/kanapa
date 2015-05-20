using Newtonsoft.Json;

namespace Kanapa.Primitives
{
  public class CouchView<T>
  {
    [JsonProperty("total_rows")]
    public long TotalCount { get; set; }
    [JsonProperty("offset")]
    public long Offset { get; set; }
    [JsonProperty("rows")]
    public CouchViewItem<T>[] Rows { get; set; }
  }
}