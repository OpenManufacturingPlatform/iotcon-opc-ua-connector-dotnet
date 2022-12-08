// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

using Opc.Ua.Client;

namespace OMP.PlantConnectivity.OpcUa.Services.Subscriptions
{
    public interface IMonitoredItemMessageProcessor
    {
        void ProcessMessage(MonitoredItem monitoredItem, MonitoredItemNotificationEventArgs eventArguments);
    }
}
