// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

using Opc.Ua;

namespace ApplicationV2.Models.Writes
{
    public record WriteCommand
    {
        public bool DoRegisteredWrite { get; set; } = false;
        public WriteValue? Value { get; set; }
    }


}
