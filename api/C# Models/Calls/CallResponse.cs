namespace OMP.Connector.Domain.API.Calls
{
    public record CallResponse : ResponseBase
    {
        public List<NodeCallValue> NodeValues { get; set; } = new List<NodeCallValue>();
    }
}