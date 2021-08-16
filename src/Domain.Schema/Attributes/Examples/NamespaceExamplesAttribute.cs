namespace Omp.Connector.Domain.Schema.Attributes.Examples
{
    public class NamespaceExamplesAttribute : ExampleAttribute
    {
        public NamespaceExamplesAttribute() : base(ExampleValues)
        {
        }

        private static readonly string[] ExampleValues =
        {
            "iot.businessobject.account.user",
            "iot.businessobject.account.usergroup"
        };
    }
}