// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

using System.Collections;
using NJsonSchema.Annotations;

namespace OMP.Connector.Domain.Schema.Attributes.Examples
{
    public abstract class ExampleAttribute : JsonSchemaExtensionDataAttribute
    {
        private const string ExamplesKey = "examples";

        protected ExampleAttribute(IEnumerable value) : base(ExamplesKey, value)
        {
        }
    }
}