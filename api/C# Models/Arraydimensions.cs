namespace OMP.Connector.Domain.API
{
    public record Arraydimensions
    {
        public string DataType { get; set; } = string.Empty;
        public List<object> Values { get; set; } = new List<object>();
    }
}