// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

using Microsoft.AspNetCore.Http;
using Opc.Ua;

namespace ApplicationV2.Models.Subscriptions
{
    public record MonitoriedItemResult(SubscriptionMonitoredItem MonitoredItem, StatusCode StatusCode, string Message);

    public class CreateSubscriptionResult : List<MonitoriedItemResult>
    {
        public CreateSubscriptionResult() { }
        public CreateSubscriptionResult(List<MonitoriedItemResult> monitoriedItems)
        {
            base.AddRange(monitoriedItems);
        }
    }

    public record CreateSubscriptionResponse : CommandResult<CreateSubscriptionsCommand, CreateSubscriptionResult>
    {
        public CreateSubscriptionResponse(CreateSubscriptionsCommand command, CreateSubscriptionResult result, bool succeeded, string? errorMessage = null)
            : base(command, result, succeeded)
        {
            this.Message = errorMessage;
        }
    }

    public record SubscriptionDto
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();

        public string EndpointUrl { get; set; } = string.Empty;

        public string PublishingInterval { get; set; } = string.Empty;

        public IDictionary<string, SubscriptionMonitoredItem> MonitoredItems { get; set; } = new Dictionary<string, SubscriptionMonitoredItem>();
    }

}
