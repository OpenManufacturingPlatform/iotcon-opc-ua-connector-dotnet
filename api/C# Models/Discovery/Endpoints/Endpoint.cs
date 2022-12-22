namespace OMP.Connector.Domain.API.Discovery.Endpoints
{
    public record Endpoint
    {
        public string EndpointUrl { get; set; } = string.Empty;
        public string SecurityMode { get; set; } = string.Empty;
        public int SecurityLevel { get; set; }
    }

}