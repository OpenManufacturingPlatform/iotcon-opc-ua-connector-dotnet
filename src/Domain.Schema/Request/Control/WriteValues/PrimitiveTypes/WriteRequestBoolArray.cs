using System.Collections.Generic;
using Omp.Connector.Domain.Schema.Request.Control.WriteValues.PrimitiveTypes.Base;

namespace Omp.Connector.Domain.Schema.Request.Control.WriteValues.PrimitiveTypes
{
    public class WriteRequestBoolArray : WriteRequestPrimitiveArray<bool>
    {
        public WriteRequestBoolArray(IEnumerable<bool> items) :base(items){}
    }
}