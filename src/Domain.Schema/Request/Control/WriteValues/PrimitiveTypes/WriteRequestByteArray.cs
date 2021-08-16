using System.Collections.Generic;
using Omp.Connector.Domain.Schema.Request.Control.WriteValues.PrimitiveTypes.Base;

namespace Omp.Connector.Domain.Schema.Request.Control.WriteValues.PrimitiveTypes
{
    public class WriteRequestByteArray : WriteRequestPrimitiveArray<byte>
    {
        public WriteRequestByteArray(IEnumerable<byte> items) : base(items) { }
    }
}