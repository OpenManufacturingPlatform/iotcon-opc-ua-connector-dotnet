// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

namespace ApplicationV2.Models.Reads
{
    public class ReadResponseCollection : List<CommandResult<ReadCommand, ReadResponse>> { }
}
