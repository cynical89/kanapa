using Newtonsoft.Json;

namespace Kanapa
{
  internal class Response
  {
    [JsonProperty("ok")]
    public bool Ok { get; set; } 
    [JsonProperty("id")]
    public string EntityId { get; set; }
    [JsonProperty("rev")]
    public string ETag { get; set; }
    [JsonProperty("error")]
    public string Error { get; set; }
    [JsonProperty("reason")]
    public string Reason { get; set; }
  }
}