namespace OMP.Connector.Domain.Schema.Attributes.Examples
{
    public class GuidExamplesAttribute : ExampleAttribute
    {
        public GuidExamplesAttribute() : base(ExampleValues)
        {
        }

        private static readonly string[] ExampleValues =
        {
            "26fabcf0-84ac-4f76-82ac-61257f4a3577",
            "df72a336-fed7-41b6-91a2-ed930eac3652",
            "3641239e-0b8f-481e-850d-4b9101bf6ec1"
        };
    }
}