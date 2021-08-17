using System.Collections.Generic;
using Omp.Connector.Domain.Schema.Request.Control.WriteValues.PrimitiveTypes.Base;

namespace Omp.Connector.Domain.Schema.Request.Control.WriteValues.PrimitiveTypes
{
    public class WriteRequestIntegerArray : WriteRequestPrimitiveArray<int>
    {
        public WriteRequestIntegerArray(IEnumerable<int> items) : base(items) { }
    }
}