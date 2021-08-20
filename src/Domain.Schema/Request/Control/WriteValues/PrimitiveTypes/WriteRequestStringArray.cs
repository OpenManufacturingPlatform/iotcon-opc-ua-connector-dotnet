using System.Collections.Generic;
using OMP.Connector.Domain.Schema.Request.Control.WriteValues.PrimitiveTypes.Base;

namespace OMP.Connector.Domain.Schema.Request.Control.WriteValues.PrimitiveTypes
{
    public class WriteRequestStringArray : WriteRequestPrimitiveArray<string>
    {
        public WriteRequestStringArray(IEnumerable<string> items) : base(items) { }
    }
}