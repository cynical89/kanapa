using Newtonsoft.Json;

namespace Kanapa
{
  public class View<T>
  {
    [JsonProperty("total_rows")]
    public long TotalCount { get; set; }
    [JsonProperty("offset")]
    public long Offset { get; set; }
    [JsonProperty("rows")]
    public ViewItem<T>[] Rows { get; set; }
  }
}