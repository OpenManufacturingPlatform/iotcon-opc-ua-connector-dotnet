namespace OMP.Connector.Domain.API.Browsing
{
    public record BrowseNodesResponse: ResponseBase
    {        
        public BrowsedNode DiscoveredNode { get; set; } = new BrowsedNode();
    }
}