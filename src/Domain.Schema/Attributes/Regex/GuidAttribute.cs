using System.ComponentModel.DataAnnotations;
using Omp.Connector.Domain.Schema.Constants;

namespace Omp.Connector.Domain.Schema.Attributes.Regex
{
    public class GuidAttribute : RegularExpressionAttribute
    {
        public GuidAttribute() : base(RegexConstants.Guid)
        {
        }
    }
}