using System;
using System.Collections.Generic;
using AutoMapper;
using OMP.Connector.Domain.Models.OpcUa.Attributes;
using OMP.Connector.Domain.OpcUa;
using Omp.Connector.Domain.Schema;
using Omp.Connector.Domain.Schema.Enums;
using Omp.Connector.Domain.Schema.Factories;
using Omp.Connector.Domain.Schema.MetaData.Message;
using Omp.Connector.Domain.Schema.SensorTelemetry;

namespace OMP.Connector.Application.OpcUa
{
    public static class TelemetryUtilities
    {
        public static SensorTelemetryMessage CreateMessage(
            TelemetryMessageContent telemetryMessageContent,
            SensorTelemetrySource dataSource,
            IMapper mapper)
        {
            var dataValue = mapper.Map<OpcDataValue>(telemetryMessageContent.DataValue);
            var dataType = TelemetryConversion.GetDataTypeName( telemetryMessageContent.DataDataType, telemetryMessageContent.ElementType, dataValue);
            var convertedValue = TelemetryConversion.ConvertToMeasurement(dataValue.Value, telemetryMessageContent.ElementType, mapper);

            var msg = ModelFactory.CreateInstance<SensorTelemetryMessage>(telemetryMessageContent.SchemaUrl);
            msg.Id = Guid.NewGuid().ToString();
            
            msg.MetaData = new MessageMetaData()
            {
                CorrelationIds = new List<string>(){},
                TimeStamp = DateTime.UtcNow,
                SenderIdentifier = new Participant()
                {
                    Id = telemetryMessageContent.SenderId,
                    Name = telemetryMessageContent.SenderName,
                    Type = ParticipantType.Gateway,
                    Route = telemetryMessageContent.SenderRoute
                },
                DestinationIdentifiers = new List<Participant>()
            };

            msg.Payload = new SensorTelemetryPayload
            {
                DataSource = dataSource,
                Data = new SensorMeasurement
                {
                    DataType = dataType,
                    Key = telemetryMessageContent.DataKey,
                    LastChangeTimestamp = dataValue.SourceTimestamp,
                    MeasurementTimestamp = dataValue.ServerTimestamp,
                    Value = convertedValue,
                    Status = dataValue.StatusCode
                }
            };

            return msg;
        }
    }
}