﻿// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

using System;

namespace OMP.Connector.Infrastructure.MQTT.Common.Events
{
    public delegate void ClosedConnectionEventHandler(object sender, EventArgs e);
    public delegate void OnMessageEventHandler(object sender, MqttMessageEventArgs e);

    
}
