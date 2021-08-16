namespace Omp.Connector.Domain.Schema.Attributes.Examples
{
    public class SchemaExamplesAttribute : ExampleAttribute
    {
        public SchemaExamplesAttribute() : base(ExampleValues)
        {
        }

        private static readonly string[] ExampleValues =
        {
            "https://example.com/schemas/business_object_schemas/2020-03-31/iot.businessobject.account.user.schema.json",
            "https://example.com/schemas/business_object_schemas/2020-03-31/iot.businessobject.account.usergroup.schema.json"
        };
    }
}