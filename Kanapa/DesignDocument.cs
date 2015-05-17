using System.Collections.Generic;
using Newtonsoft.Json;

namespace Kanapa
{
  public class DesignDocument
  {
    [JsonIgnore]
    public string Name => Id.Substring(Id.IndexOf('/') + 1);
    [JsonProperty("_id")]
    public string Id { get; set; }
    [JsonProperty("language")]
    public string Language { get; set; }
    [JsonProperty("_rev", NullValueHandling = NullValueHandling.Ignore)]
    public string Revision { get; set; }
    [JsonConverter(typeof(ViewDefinitionsConverter)), JsonProperty("views")]
    public IEnumerable<ViewDefinition> Views { get; set; }
    [JsonIgnore]
    internal bool IgnoreRevisionAndId { get; set; }

    internal bool ShouldSerializeRevision => IgnoreRevisionAndId;
    internal bool ShouldSerializeId => IgnoreRevisionAndId;
  }
}