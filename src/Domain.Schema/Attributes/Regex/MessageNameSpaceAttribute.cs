// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

using System.ComponentModel.DataAnnotations;
using OMP.Connector.Domain.Schema.Constants;

namespace OMP.Connector.Domain.Schema.Attributes.Regex
{
    public class MessageNameSpaceAttribute : RegularExpressionAttribute
    {
        public MessageNameSpaceAttribute() : base(RegexConstants.MessageNamespace)
        {
        }
    }
}