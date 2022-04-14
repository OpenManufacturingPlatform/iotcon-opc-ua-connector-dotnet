// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

using System;
using System.Collections.Generic;
using OMP.Connector.Domain.Schema.Request.Control.WriteValues.PrimitiveTypes.Base;

namespace OMP.Connector.Domain.Schema.Request.Control.WriteValues.PrimitiveTypes
{
    public class WriteRequestDateTimeArray : WriteRequestPrimitiveArray<DateTime>
    {
        public WriteRequestDateTimeArray(IEnumerable<DateTime> items) : base(items) { }
    }
}