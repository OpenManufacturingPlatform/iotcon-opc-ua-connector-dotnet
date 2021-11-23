namespace OMP.Connector.Domain.API.Reads.ReadValue
{
    public record ReadValuesResponse : ResponseBase
    {
        public List<NodeInReadValueResponse> NodeValues { get; set; } = new List<NodeInReadValueResponse>();
    }
}