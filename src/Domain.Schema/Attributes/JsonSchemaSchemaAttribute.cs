using NJsonSchema.Annotations;

namespace Omp.Connector.Domain.Schema.Attributes
{
    public class JsonSchemaSchemaAttribute : JsonSchemaExtensionDataAttribute
    {
        public JsonSchemaSchemaAttribute() : base("format", "uri-reference")
        {
        }        
    }
}