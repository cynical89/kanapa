using System.Collections.Generic;
using Newtonsoft.Json;

namespace Kanapa.Primitives
{
  public class CouchDesignDocument
  {
    [JsonIgnore]
    public string Name => Id.Substring(Id.IndexOf('/') + 1);
    [JsonProperty("_id"), JsonIgnore]
    public string Id { get; set; }
    [JsonProperty("language")]
    public string Language { get; set; }
    [JsonProperty("_rev", NullValueHandling = NullValueHandling.Ignore), JsonIgnore]
    public string Revision { get; set; }
    [JsonConverter(typeof(CouchViewDefinitionsConverter)), JsonProperty("views")]
    public IEnumerable<CouchViewDefinition> Views { get; set; }
  }
}