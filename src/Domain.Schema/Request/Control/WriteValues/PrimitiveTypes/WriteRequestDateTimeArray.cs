using System;
using System.Collections.Generic;
using Omp.Connector.Domain.Schema.Request.Control.WriteValues.PrimitiveTypes.Base;

namespace Omp.Connector.Domain.Schema.Request.Control.WriteValues.PrimitiveTypes
{
    public class WriteRequestDateTimeArray : WriteRequestPrimitiveArray<DateTime>
    {
        public WriteRequestDateTimeArray(IEnumerable<DateTime> items) : base(items) { }
    }
}