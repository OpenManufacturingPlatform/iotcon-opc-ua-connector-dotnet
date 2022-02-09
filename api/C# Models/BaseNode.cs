namespace OMP.Connector.Domain.API
{
    public record BaseNode
    {
        public virtual string NodeId { get; set; } = string.Empty;
        public virtual string NodeType { get; set; } = string.Empty;
        public virtual string NodeClass { get; set; } = string.Empty;
        public virtual string DataType { get; set; } = string.Empty;
        public virtual string Value { get; set; } = string.Empty;
        public virtual string ValueRank { get; set; } = string.Empty;
        public virtual string AccessLevel { get; set; } = string.Empty;
        public virtual string UserAccessLevel { get; set; } = string.Empty;
        public virtual long MinimumSamplingInterval { get; set; }
        public virtual bool Historizing { get; set; }
        public virtual Arraydimensions ArrayDimensions { get; set; } = new Arraydimensions();
    }
}