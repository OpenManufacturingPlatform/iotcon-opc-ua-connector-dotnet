// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

using Opc.Ua;

namespace ApplicationV2.Models.Reads
{
    public record ReadResponse(DataValue DataValue, ServiceResult ServiceResult);


}
