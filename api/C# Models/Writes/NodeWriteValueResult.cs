namespace OMP.Connector.Domain.API
{
    public class NodeWriteValueResult
    {
        public string NodeId { get; set; } = string.Empty;
        public Status Status { get; set; } = new Status();
    }

}