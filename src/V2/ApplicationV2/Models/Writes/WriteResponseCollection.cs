// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

using Opc.Ua;

namespace ApplicationV2.Models.Writes
{
    public class WriteResponseCollection : List<CommandResult<WriteCommand, StatusCode>> { }
}
