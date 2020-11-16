using Newtonsoft.Json;

namespace CanalSharp.Connections
{
    public class CanalClientRunningInfo
    {
        [JsonProperty("active")]
        public bool Active { get; set; }

        [JsonProperty("address")]
        public string Address { get; set; }

        [JsonProperty("clientId")]
        public string ClientId { get; set; }
    }
}