// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.Serialization;

namespace OMP.Connector.Domain.Exceptions
{
    public class ValidationException : Exception
    {
        private ValidationResult[] ValidationResults { get; }

        public ValidationException(IEnumerable<ValidationResult> results)
        {
            this.ValidationResults = results.ToArray();
        }

        public ValidationException(string message) : base(message)
        {
            this.ValidationResults = new[] { new ValidationResult(message) };
        }

        public ValidationException(string message, IEnumerable<ValidationResult> results) : base(message, null)
        {
            this.ValidationResults = results.ToArray();
        }

        // This constructor is needed for serialization.
        protected ValidationException(SerializationInfo info, StreamingContext context) : base(info, context) { }

        public override string ToString()
        {
            return string.Join(Environment.NewLine, this.ValidationResults.ToList());
        }
    }
}
