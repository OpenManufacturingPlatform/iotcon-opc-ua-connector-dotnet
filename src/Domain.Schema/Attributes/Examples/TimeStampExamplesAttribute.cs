namespace Omp.Connector.Domain.Schema.Attributes.Examples
{
    public class TimeStampExamplesAttribute : ExampleAttribute
    {
        public TimeStampExamplesAttribute() : base(ExampleValues)
        {
        }

        private static readonly string[] ExampleValues =
        {
            "2015-08-29T11:22:09.815234Z",
            "2015-08-29T11:22:09.815234+02:00",
            "2015-08-29T11:22:09.815Z",
            "2015-08-29T11:22:09.815+02:00",
            "2015-08-29T11:12:09Z",
            "2015-08-29T11:12:09+02:00"
        };
    }
}