namespace OMP.Connector.Domain.API
{
    public record NodeWriteValue
    {
        public string NodeId { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
    }

}