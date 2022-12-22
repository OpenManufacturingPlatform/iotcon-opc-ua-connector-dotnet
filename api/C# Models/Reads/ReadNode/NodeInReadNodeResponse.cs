namespace OMP.Connector.Domain.API.Reads.ReadNode
{
    public record NodeInReadNodeResponse : BaseNode
    {
        public Status Status { get; set; } = new Status();
    }
}