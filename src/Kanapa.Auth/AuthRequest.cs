using Newtonsoft.Json;

namespace Kanapa.Auth
{
  internal class AuthRequest
  {
    [JsonProperty("name")]
    public string Name { get; set; }
    [JsonProperty("password")]
    public string Password { get; set; }
  }
}