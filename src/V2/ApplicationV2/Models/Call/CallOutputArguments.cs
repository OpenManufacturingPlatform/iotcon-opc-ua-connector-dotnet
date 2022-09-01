// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

using Opc.Ua;

namespace ApplicationV2.Models.Call
{
    public record CallCommandCollectionResponse : CommandResult<CallCommandCollection, CallResponse>
    {
        public CallCommandCollectionResponse(CallCommandCollection command, CallResponse response, bool succeeded)
            : base(command, response, succeeded) { }

        public CallCommandCollectionResponse(CallCommandCollection command, Exception exception)
            : base(command, default, false)
        {
            Message = exception.Message;            
            
        }

        public static CallCommandCollectionResponse Success(CallCommandCollection command, CallResponse response)
            => new CallCommandCollectionResponse(command, response, true);

        public static CallCommandCollectionResponse Failed(CallCommandCollection command, Exception exception)
            => new CallCommandCollectionResponse(command, exception);
    }
}
