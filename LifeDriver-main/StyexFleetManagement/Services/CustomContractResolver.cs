using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace StyexFleetManagement.Services
{
    public class CustomContractResolver : DefaultContractResolver
    {
        public CustomContractResolver()
        {
            PropertyMappings = new Dictionary<string, string>
            {
                ["Longitude"] = "Lon",
                ["Latitude"] = "Lat",
                ["Id"] = "TripPointId",
            };

            IgnoreProperties = new List<string> { "HasOBDData" };
        }

        private Dictionary<string, string> PropertyMappings { get; }
        private List<string> IgnoreProperties { get; }

        protected override string ResolvePropertyName(string propertyName)
        {
            string resolvedName;
            var resolved = PropertyMappings.TryGetValue(propertyName, out resolvedName);
            return resolved ? resolvedName : base.ResolvePropertyName(propertyName);
        }

        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            JsonProperty property = base.CreateProperty(member, memberSerialization);

            if (IgnoreProperties.Contains(property.PropertyName))
            {
                property.ShouldSerialize = p => false;
            }

            return property;
        }
    }
}
