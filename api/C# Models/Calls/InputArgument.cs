namespace OMP.Connector.Domain.API.Calls
{
    public record InputArgument
    {
        public string Key { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
    }

}