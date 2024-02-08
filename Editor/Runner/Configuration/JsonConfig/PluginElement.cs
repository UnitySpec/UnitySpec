//using Newtonsoft.Json;

using UnityFlow.Plugins;
using System.Runtime.Serialization;

namespace UnityFlow.Configuration.JsonConfig
{
    public class PluginElement
    {
        //[JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
        [DataMember(Name="name")]
        public string Name { get; set; }

        //[JsonProperty("path", NullValueHandling = NullValueHandling.Ignore)]
        [DataMember(Name = "path")]
        public string Path { get; set; }

        //[JsonProperty("parameters", NullValueHandling = NullValueHandling.Ignore)]
        [DataMember(Name = "parameters")]
        public string Parameters { get; set; }

        //[JsonProperty("type", NullValueHandling = NullValueHandling.Ignore)]
        [DataMember(Name = "type")]
        public PluginType Type { get; set; }
    }
}