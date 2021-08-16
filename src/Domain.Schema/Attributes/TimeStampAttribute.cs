using NJsonSchema.Annotations;
using Omp.Connector.Domain.Schema.Constants;

namespace Omp.Connector.Domain.Schema.Attributes
{
    public class TimeStampAttribute : JsonSchemaExtensionDataAttribute
    {
        public TimeStampAttribute() : base("pattern", RegexConstants.Timestamp)
        {
        }
    }
}