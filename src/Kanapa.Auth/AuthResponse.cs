using Newtonsoft.Json;

namespace Kanapa.Auth
{
  internal class AuthResponse
  {
    [JsonProperty("ok")]
    public bool Ok { get; set; }

    [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
    public string Name { get; set; }

    [JsonProperty("roles", NullValueHandling = NullValueHandling.Ignore)]
    public string[] Roles { get; set; }
  }
}