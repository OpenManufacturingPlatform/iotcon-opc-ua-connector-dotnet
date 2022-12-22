namespace OMP.Connector.Domain.API.Browsing
{
    public record BrowsedNode: BaseNode
    {        
        public List<BrowsedNode> ChildNodes { get; set; } = new List<BrowsedNode>();
    }
}