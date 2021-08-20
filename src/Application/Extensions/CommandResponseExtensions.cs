using Omp.Connector.Domain.Schema.Responses.Base;

namespace OMP.Connector.Application.Extensions
{
    public static class CommandResponseExtensions
    {
        public static bool StatusIsGood(this CommandResponse commandResponse)
        {
            //TODO: Discuss - Is this really a good check? What about contains, culture and case?
            return !commandResponse.Message.StartsWith("Bad");
        }

        //TODO: Determine if and how to handle CreateSubscriptionItemResponse messages
    }
}