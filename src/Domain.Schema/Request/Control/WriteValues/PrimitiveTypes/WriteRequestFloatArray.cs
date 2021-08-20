using System.Collections.Generic;
using OMP.Connector.Domain.Schema.Request.Control.WriteValues.PrimitiveTypes.Base;

namespace OMP.Connector.Domain.Schema.Request.Control.WriteValues.PrimitiveTypes
{
    public class WriteRequestFloatArray : WriteRequestPrimitiveArray<float>
    {
        public WriteRequestFloatArray(IEnumerable<float> items) : base(items) { }
    }
}