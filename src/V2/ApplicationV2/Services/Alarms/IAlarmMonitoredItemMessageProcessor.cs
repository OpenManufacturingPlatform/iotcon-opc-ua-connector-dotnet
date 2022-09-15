// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

using Opc.Ua.Client;

namespace OMP.PlantConnectivity.OpcUA.Services.Alarms
{
    public interface IAlarmMonitoredItemMessageProcessor
    {
        string Identifier { get; }
        void ProcessMessage(MonitoredItem monitoredItem, MonitoredItemNotificationEventArgs eventArguments);
    }
}
