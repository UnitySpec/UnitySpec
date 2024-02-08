using System.ComponentModel;
using System.Runtime.Serialization;

namespace UnityFlow.Configuration.JsonConfig
{
    public class UnitTestProviderElement
    {
        //[JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
        [DefaultValue("NUnit")]
        [DataMember(Name = "name")]
        public string Name { get; set; }
    }
}