using System.ComponentModel.DataAnnotations;
using OMP.Connector.Domain.Schema.Constants;

namespace OMP.Connector.Domain.Schema.Attributes.Regex
{
    public class MacAddressAttribute : RegularExpressionAttribute
    {
        public MacAddressAttribute() : base(RegexConstants.MacAddress)
        {
        }
    }
}