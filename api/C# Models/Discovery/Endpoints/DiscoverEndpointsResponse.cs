namespace OMP.Connector.Domain.API.Discovery.Endpoints
{
    public record DiscoverEndpointsResponse : ResponseBase
    {
        public List<Endpoint> Endpoints { get; set; } = new List<Endpoint>();
    }

}