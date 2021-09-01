using System.ComponentModel.DataAnnotations;
using OMP.Connector.Domain.Schema.Constants;

namespace OMP.Connector.Domain.Schema.Attributes.Regex
{
    public class PercentageAttribute : RegularExpressionAttribute
    {
        public PercentageAttribute() : base(RegexConstants.Percentage)
        {
        }
    }
}