namespace OMP.Connector.Domain.API.Calls
{
    public record NodeCallValue
    {
        public string NodeId { get; set; } = string.Empty;
        public Status Status { get; set; } = new Status();
        public List<Outputargument> OutputArguments { get; set; } = new List<Outputargument>();
    }


}