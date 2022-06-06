﻿// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

using Opc.Ua;

namespace OMP.Connector.Domain.OpcUa
{
    public class AlarmMessageContent
    {
        public string SchemaUrl { get; set; }
        public string SenderId { get; set; }
        public string SenderName { get; set; }
        public string SenderRoute { get; set; }
        public string SequenceNr { get; set; }
        public string DataSourceId { get; set; }
        public string DataSourceName { get; set; }
        public string DataSourceRoute { get; set; }
        public string DataSourceUrl { get; set; }
        public string DataKey { get; set; }
        public string DataDescription { get; set; }
        public EventFieldList EventFields { get; set; }
    }
}
