namespace OMP.Connector.Domain.API.Reads.ReadValue
{
    public record NodeInReadValueResponse
    {
        public string NodeId { get; set; } = string.Empty;
        public Status Status { get; set; } = new Status();
        public string Balue { get; set; } = string.Empty;
        public string DataType { get; set; } = string.Empty;
    }
}