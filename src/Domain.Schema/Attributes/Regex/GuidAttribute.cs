using System.ComponentModel.DataAnnotations;
using OMP.Connector.Domain.Schema.Constants;

namespace OMP.Connector.Domain.Schema.Attributes.Regex
{
    public class GuidAttribute : RegularExpressionAttribute
    {
        public GuidAttribute() : base(RegexConstants.Guid)
        {
        }
    }
}