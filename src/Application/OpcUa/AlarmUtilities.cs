using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using OMP.Connector.Domain.Models.OpcUa.Attributes;
using OMP.Connector.Domain.OpcUa;
using OMP.Connector.Domain.Schema;
using OMP.Connector.Domain.Schema.Alarms;
using OMP.Connector.Domain.Schema.Enums;
using OMP.Connector.Domain.Schema.Factories;
using OMP.Connector.Domain.Schema.MetaData.Message;
using Opc.Ua;

namespace OMP.Connector.Application.OpcUa
{
    public static class AlarmUtilities
    {
        public static AlarmMessage CreateMessage(
            AlarmMessageContent alarmMessageContent,
            AlarmSource dataSource,
            MonitoringFilter monitoringFilter,
            IMapper mapper)
        {
            var eventFilter = (EventFilter)monitoringFilter;
            var eventFields = mapper.Map<OpcEventFieldList>(alarmMessageContent.EventFields);
            var dictionary = eventFilter.SelectClauses.Zip(eventFields.EventFields, (k, v) => new { k, v }).ToDictionary(x => x.k.ToString(), x => x.v);
            //var convertedValue = AlarmConversion.ConvertToAlarm(dictionary, mapper);

            var msg = ModelFactory.CreateInstance<AlarmMessage>(alarmMessageContent.SchemaUrl);
            msg.Id = Guid.NewGuid().ToString();

            msg.MetaData = new MessageMetaData()
            {
                CorrelationIds = new List<string>() { },
                TimeStamp = DateTime.UtcNow,
                SenderIdentifier = new Participant()
                {
                    Id = alarmMessageContent.SenderId,
                    Name = alarmMessageContent.SenderName,
                    Type = ParticipantType.Gateway,
                    Route = alarmMessageContent.SenderRoute
                },
                DestinationIdentifiers = new List<Participant>()
            };

            msg.Payload = new AlarmPayload
            {
                DataSource = dataSource,
                Data = new AlarmEventData
                {
                    EventProperties = dictionary
                }
            };

            return msg;
        }
    }
}