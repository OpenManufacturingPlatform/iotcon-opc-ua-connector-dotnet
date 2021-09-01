using NJsonSchema.Annotations;

namespace OMP.Connector.Domain.Schema.Attributes
{
    public class JsonSchemaSchemaAttribute : JsonSchemaExtensionDataAttribute
    {
        public JsonSchemaSchemaAttribute() : base("format", "uri-reference")
        {
        }        
    }
}