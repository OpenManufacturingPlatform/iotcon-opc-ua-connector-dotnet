// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

using System.Collections.Generic;
using OMP.Connector.Domain.Schema.Request.Control.WriteValues.PrimitiveTypes.Base;

namespace OMP.Connector.Domain.Schema.Request.Control.WriteValues.PrimitiveTypes
{
    public class WriteRequestIntegerArray : WriteRequestPrimitiveArray<int>
    {
        public WriteRequestIntegerArray(IEnumerable<int> items) : base(items) { }
    }
}