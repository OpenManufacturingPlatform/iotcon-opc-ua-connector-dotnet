namespace OmpHandsOnUi
{
	public class ResponseMessage
    {
        public string Namespace { get; set; }
        public Payload Payload { get; set; }
        public string Id { get; set; }
        public string Schema { get; set; }
        public Metadata Metadata { get; set; }
    }
}