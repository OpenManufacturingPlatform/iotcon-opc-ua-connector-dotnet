namespace OMP.Connector.Domain.API.Calls
{
    public record Outputargument
    {
        public string Key { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
        public string DataType { get; set; } = string.Empty;
    }


}