using System.Collections.Generic;
using Omp.Connector.Domain.Schema.Request.Control.WriteValues.PrimitiveTypes.Base;

namespace Omp.Connector.Domain.Schema.Request.Control.WriteValues.PrimitiveTypes
{
    public class WriteRequestStringArray : WriteRequestPrimitiveArray<string>
    {
        public WriteRequestStringArray(IEnumerable<string> items) : base(items) { }
    }
}