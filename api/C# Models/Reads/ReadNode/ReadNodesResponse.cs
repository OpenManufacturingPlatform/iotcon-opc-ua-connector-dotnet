namespace OMP.Connector.Domain.API.Reads.ReadNode
{
    public record ReadNodesResponse : ResponseBase
    {
        public List<NodeInReadNodeResponse> Nodes { get; set; } = new List<NodeInReadNodeResponse>();
    }
}