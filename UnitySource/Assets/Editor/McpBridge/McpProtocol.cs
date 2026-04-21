using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace McpBridge
{
    [Serializable]
    public class UnityCommand
    {
        [JsonProperty("id")]    public string Id;
        [JsonProperty("type")]  public string Type;
        [JsonProperty("payload")] public JObject Payload;

        public static UnityCommand FromJson(string json)
        {
            return JsonConvert.DeserializeObject<UnityCommand>(json);
        }
    }

    [Serializable]
    public class UnityResponse
    {
        [JsonProperty("id")]    public string Id;
        [JsonProperty("ok")]    public bool Ok;
        [JsonProperty("data")]  public JToken Data;
        [JsonProperty("error")] public string Error;

        public static UnityResponse Success(string id, JToken data = null)
        {
            return new UnityResponse { Id = id, Ok = true, Data = data ?? new JObject(), Error = null };
        }

        public static UnityResponse Fail(string id, string error)
        {
            return new UnityResponse { Id = id, Ok = false, Data = null, Error = error };
        }

        public string ToJson()
        {
            return JsonConvert.SerializeObject(this) + "\n";
        }
    }
}
