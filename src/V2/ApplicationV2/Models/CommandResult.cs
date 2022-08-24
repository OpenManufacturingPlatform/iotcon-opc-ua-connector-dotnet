// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

namespace ApplicationV2.Models
{
    public record CommandResult<TCommand, TResponse> : CommandResultBase
    {
        public CommandResult()        {        }

        public CommandResult(TCommand command, TResponse response)
        {
            Command = command;
            Response = response;
        }
        public TCommand? Command { get; set; }
        public TResponse? Response { get; set; }
    }

    
}
