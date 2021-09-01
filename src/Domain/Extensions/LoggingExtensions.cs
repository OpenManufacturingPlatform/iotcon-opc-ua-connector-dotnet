using System;
using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OMP.Connector.Domain.Enums;

namespace OMP.Connector.Domain.Extensions
{
    public static class LoggingExtensions
    {
        public static void Trace(this ILogger logger, string message)
        {
            logger.LogTrace(MessageToJson(message));
        }

        public static void Debug(this ILogger logger, string message)
        {
            logger.LogDebug(MessageToJson(message));
        }

        public static void Information(this ILogger logger, string message)
        {
            logger.LogInformation(MessageToJson(message));
        }

        public static void Warning(this ILogger logger, string message)
        {
            logger.LogWarning(MessageToJson(message));
        }

        public static void Warning(this ILogger logger, Exception e, string message)
        {
            logger.LogWarning(MessageToJson(message, e));
        }

        public static void Error(this ILogger logger, string message)
        {
            logger.LogError(MessageToJson(message));
        }

        public static void Error(this ILogger logger, Exception e, string message)
        {
            logger.LogError(MessageToJson(message, e));
        }

        public static void Error(this ILogger logger, Exception e)
        {
            var error = e.Demystify();
            logger.Error(error, error.GetMessage());
        }

        private static string MessageToJson(string message)
        {
            var values = JObject.FromObject(new
            {
                messageContent = $"{message}"
            });

            var json = values.ToString(Formatting.None);
            return json;
        }

        private static string MessageToJson(string message, Exception exception)
        {
            var error = $"{message}{Constants.ErrorMessageSeparator}{exception.GetMessage()}";
            return MessageToJson(error);
        }

        public static void LogEvent(this ILogger logger, EventTypes eventType, string requestId)
        {
            var msg = eventType switch
            {
                EventTypes.ReceivedRequestFromBroker => "Received downstream message from broker",
                EventTypes.QueuedMessageInternally => "Message queued on internal memory queue",
                EventTypes.DequeuedMessageInternally => "Message dequeued from internal memory queue",
                EventTypes.SentRequestToOpcUa => "OpcUa command request execution started",
                EventTypes.ReceivedResponseFromOpcUa => "OpcUa command request execution completed",
                EventTypes.SentResponseToBroker => "Sent upstream message to broker",
                EventTypes.SentTelemetryToBroker => "Sent upstream telemetry message to broker",
                EventTypes.SentConfigToBroker => "Sent config message to broker",
                _ => throw new NotImplementedException()
            };
            var eventTypeId = (int)eventType;
            logger.LogTrace(new EventId(eventTypeId), $"++{eventTypeId}+{requestId}+{msg}");
        }
    }
}