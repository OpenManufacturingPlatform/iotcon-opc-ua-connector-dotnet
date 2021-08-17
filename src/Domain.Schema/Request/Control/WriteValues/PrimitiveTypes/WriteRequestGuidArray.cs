using System;
using System.Collections.Generic;
using Omp.Connector.Domain.Schema.Request.Control.WriteValues.PrimitiveTypes.Base;

namespace Omp.Connector.Domain.Schema.Request.Control.WriteValues.PrimitiveTypes
{
    public class WriteRequestGuidArray : WriteRequestPrimitiveArray<Guid>
    {
        public WriteRequestGuidArray(IEnumerable<Guid> items) : base(items) { }
    }
}