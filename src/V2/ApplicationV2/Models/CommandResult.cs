// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

namespace OMP.PlantConnectivity.OpcUa.Models
{
    public record CommandResult<TCommand, TResponse> : CommandResultBase
    {
        public CommandResult()        {        }

        public CommandResult(TCommand command, TResponse response) : this(command, response, true)
        { }

        public CommandResult(TCommand command, TResponse response, bool succeeded, string message = "")
        {
            Command = command;
            Response = response;
            Succeeded = succeeded;
            Message = message;
        }

        public TCommand? Command { get; set; }
        public TResponse? Response { get; set; }
    }   
}
