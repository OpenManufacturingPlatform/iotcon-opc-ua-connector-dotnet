using System.Collections.Generic;
using Omp.Connector.Domain.Schema.Request.Control.WriteValues.PrimitiveTypes.Base;

namespace Omp.Connector.Domain.Schema.Request.Control.WriteValues.PrimitiveTypes
{
    public class WriteRequestDoubleArray : WriteRequestPrimitiveArray<double>
    {
        public WriteRequestDoubleArray(IEnumerable<double> items) : base(items) { }
    }
}