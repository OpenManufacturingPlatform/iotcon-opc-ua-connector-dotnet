// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

using Opc.Ua.Client;

namespace OMP.PlantConnectivity.OpcUA.Services.Subscriptions
{
    public interface IMonitoredItemMessageProcessor
    {
        string Identifier { get; }
        void ProcessMessage(MonitoredItem monitoredItem, MonitoredItemNotificationEventArgs eventArguments);
    }
}
