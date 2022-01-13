using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using AutoMapper;
using Microsoft.Extensions.Logging;
using OMP.Connector.Domain.Models.Command.Requests.Basic.NodeCommands;
using Opc.Ua;
using WriteRequest = OMP.Connector.Domain.Schema.Request.Control.WriteRequest;

namespace OMP.Connector.Infrastructure.AutoMapper.Converters
{
    public class DataValueConverter : IValueConverter<WriteRequest, DataValue>,
                                      IValueConverter<WriteRequestWrapper, DataValue>
    {
        private readonly ILogger _logger;

        public DataValueConverter(ILogger<DataValueConverter> logger)
        {
            _logger = logger;
        }

        public DataValue Convert(WriteRequest sourceMember, ResolutionContext _)
            => this.ConvertToDataValue(sourceMember.NodeId, sourceMember.Value);


        public DataValue Convert(WriteRequestWrapper sourceMember, ResolutionContext _)
            => this.ConvertToDataValue(sourceMember.NodeId, sourceMember.Value);

        private DataValue ConvertToDataValue(string nodeId, object value)
        {
            var dataValue = new DataValue();

            try
            {
                var parsedNodeId = NodeId.Parse(nodeId);
                var type = DataTypes.GetSystemType(parsedNodeId, EncodeableFactory.GlobalFactory);

                var converter = TypeDescriptor.GetConverter(type ?? typeof(object));

                // ReSharper disable once AssignNullToNotNullAttribute
                var convertedValue = converter.IsValid(value)
                    ? converter.ConvertFrom(null, CultureInfo.InvariantCulture, value)
                    : value;

                dataValue.Value = convertedValue;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Demystify(), "Could not convert WriteRequest to DataValue");
                dataValue.StatusCode = StatusCodes.Bad;
            }

            return dataValue;
        }
    }
}