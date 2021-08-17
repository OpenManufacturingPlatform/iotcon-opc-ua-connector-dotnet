using System;
using System.Collections.Generic;
using Omp.Connector.Domain.Schema.Interfaces;

namespace Omp.Connector.Domain.Schema.Request.Control.WriteValues.PrimitiveTypes.Base
{
    public class WriteRequestPrimitiveArray<TType> : List<TType>, IWriteRequestValue, IPrimitiveArray
        where TType: IComparable, IEquatable<TType>
    {
        public WriteRequestPrimitiveArray(IEnumerable<TType> items) 
            => this.AddRange(items);

        Array IPrimitiveArray.ToArray() 
            => this.ToArray();
    }
}