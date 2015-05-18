using Newtonsoft.Json;

namespace Kanapa
{
  public class CouchViewItem<T>
  {
    [JsonProperty("id")]
    public string Id { get; set; }

    [JsonProperty("key")]
    public string Key { get; set; }

    [JsonProperty("value")]
    public T Item { get; set; }
  }
}