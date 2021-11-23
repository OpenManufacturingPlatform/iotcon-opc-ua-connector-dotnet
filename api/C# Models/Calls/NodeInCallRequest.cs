namespace OMP.Connector.Domain.API.Calls
{
    public record NodeInCallRequest
    {
        public string NodeId { get; set; } = string.Empty;
        public List<InputArgument> InputArguments { get; set; } = new List<InputArgument>();
    }

}