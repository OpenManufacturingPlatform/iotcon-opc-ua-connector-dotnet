using System;
using System.ComponentModel;
using Newtonsoft.Json;
using OMP.Connector.Domain.Schema;

namespace OMP.Connector.Domain.Models
{
    public class EndpointDescriptionDto
    {
        [JsonProperty("endpointUrl", Required = Required.Always)]
        [Description("endpointUrl")]
        public string EndpointUrl {get; set;}
        
        [JsonProperty("serverDetails", Required = Required.Always)]
        [Description("serverDetails")]
        public ServerDetails ServerDetails {get; set;}
        
        protected bool Equals(EndpointDescriptionDto other)
        {
            return this.EndpointUrl == other.EndpointUrl && 
                   this.ServerDetails.Name == other.ServerDetails.Name &&
                   this.ServerDetails.Route == other.ServerDetails.Route;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return this.Equals((EndpointDescriptionDto) obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(this.EndpointUrl, this.ServerDetails);
        }
    }
}