using Microsoft.AspNetCore.SignalR;

namespace OmpHandsOnUi
{
	public class HandsOnHub : Hub
    {
        public const string RequestMessageEventName = "OnTelemetryMessage";
        public const string ResponseMessageEventName = "OnResponseMessage";
        public const string TelemetryReceivedEventName = "OnTelemetryMessage";
    }    
}
