using System.ComponentModel.DataAnnotations;
using OMP.Connector.Domain.Schema.Constants;

namespace OMP.Connector.Domain.Schema.Attributes.Regex
{
    public class EmailAttribute : RegularExpressionAttribute
    {
        public EmailAttribute() : base(RegexConstants.Email)
        {
        }
    }
}