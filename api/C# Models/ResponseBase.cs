namespace OMP.Connector.Domain.API
{
    public abstract record ResponseBase
    {
        public virtual string RequestId { get; set; } = string.Empty;
        public virtual string ConnectorId { get; set; } = string.Empty;
        public virtual Timestamps Timestamps { get; set; } = new Timestamps();
        public virtual string RequestType { get; set; } = string.Empty;
        public virtual Status Status { get; set; } = new Status();
    }
}