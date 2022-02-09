namespace OMP.Connector.Domain.API
{
    public record WriteValuesResponse: ResponseBase
    {
        public List<NodeWriteValueResult> Nodes { get; set; } = new List<NodeWriteValueResult>();
    }

}