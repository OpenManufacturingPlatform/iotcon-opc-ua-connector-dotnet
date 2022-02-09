namespace OMP.Connector.Domain.API
{
    public abstract record RequestBase
    {
        public virtual string Id { get; set; } = Guid.NewGuid().ToString();
        public virtual string ConnectorId { get; set; } = string.Empty;
        public virtual string RequestType { get; set; } = string.Empty;
        public virtual string EndpointUrl { get; set; } = string.Empty;
    }
}
