using System;
using uPLibrary.Networking.M2Mqtt.Messages;

namespace OMP.Connector.Infrastructure.MQTT.Common.Events
{
    public delegate void ClosedConnectionEventHandler(object sender, EventArgs e);
    public delegate void OnMessageEventHandler(object sender, MqttMessageEventArgs e);
    public delegate void OnMessagePublishedEventHandler(object sender, MqttMsgPublishedEventArgs e);


}
