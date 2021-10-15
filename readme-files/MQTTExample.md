# OMP OPC UA Connector via MQTT

The **OMP OPC UA Connecotr** via (reffered to as the **Connector**) **MQTT** provides a mechanism to interact with **OPC UA Server(s)** over **MQTT**, wihtout having to implement the OPC UA protocol. The **Connector** allows you to:
- *Subscribe* to telemery message on nodes
- Do *Read* opperations on nodes
- Do *Write* opperations on nodes
- Do *Call* opperations on nodes
- Browse nodes
- ...and more

In this document we intend to provide a guide to *Configure*, *Build*, *Run* and *Test* an generic implementation vir **MQTT**

## Index

- [Prerequisites](#prerequisites)
- [How to Configure](#how-to-configure)
- [How to Run](#how-to-run)
- [How to Test](#how-to-test)



## Prerequisites
- Docker - To Build and run the Connector via Docker
- OPC UA Server
- MQTT Broker

---
## How to Configure the Connector

**Verry important**: An example settings file is provided **moduleSettings.example.json** (/samples/MqttSample/moduleSettings.example.json). Please copy the file and change the name to **moduleSettings.json** (/samples/MqttSample/moduleSettings.json). Change any and all settings to suite your environment here.

##  Understanding how the *Connector's* Configuration works
To understand how the connector's configuration works it is important to understant how/where the Connector gets commands and send response/telemetry to-and-from the outside world.

| Endpoint        | Description   |
| ------------- | ------------- |
| Command | This is the endpoint (MQTT Topic) where the outside world can send commands to the *Connector*  |
| Response | This is the endpoint (MQTT Topic) where the *Connector* will send the Result of commands back  |
| Telemetry | This is the endpoint (MQTT Topic) where the *Connector* will publish telemetry message  |


###  Configuring the Endpoints for MQTT
Then the configuration file has a couple of sections but for MQTT we will focus on the *Communication* section. 
Under *Communication* there are 4 sub sections of importance
- Shared
```json
 {
  ...,
  "Communication": {    
    "Shared": {
      "Type": "mqtt",
       ...
```
In an attempt to not bloat the settings file the *Shared* section is there to share basic MQTT settings (e.g Username, Password,..) between different endpoints, if the same **MQTT Broker** is used accross the endpoints (Topics).

**Note**: Shared settings are read and applied first (1st) subsequent section will override Shared settings. Meaning if you set the brokerAddress in Shared to 'Broker-Shared'. And in the CommandEndpoint section (Under NativeSettings) you set the brokerAddress to 'Broker-Command' then, when the *Connector* instatiates the CommandEndpoint it will set the brokerAddress to 'Broker-Command' and not 'Broker-Shared'.

- Command Endpoint
```json
 {
  ...,
  "CommandEndpoint": {
      "Type": "mqtt",
      "NativeSettings": {
        "topics": [
          {
            "topicName": "commands",
            "qosLevel": 1,
            "retain": false
          }
        ]
      }
        ...
```
This is where we can specify what Topic, QoS Level and any other setting specific to the *Command Endpoint*

- Response Endpoint
```json
 {
  ...,
  "ResponseEndpoint": {
      "Type": "mqtt",
      "NativeSettings": {
        "topics": [
          {
            "topicName": "responses",
            "qosLevel": 1,
            "retain": false
          }
        ]
      }
        ...
```
This is where we can specify what Topic, QoS Level and any other setting specific to the *Response Endpoint*

- Telemetry Endpoint
```json
{
  ...,
    "TelemetryEndpoint": {
      "Type": "mqtt",
      "NativeSettings": {
        "topics": [
          {
            "topicName": "telemetry",
            "qosLevel": 1,
            "retain": false
          }
        ]
      }
        ...
```

This is where we can specify what Topic, QoS Level and any other setting specific to the *Telemetry Endpoint*

### Complete example of the *Communication* section in the settigns file:

```json
 {
  ...,
  "Communication": {
    "SchemaUrl": "https://someSchemaStore.com/schemas/",
    "Shared": {
      "Type": "mqtt",
      "NativeSettings": {
        "brokerAddress": "localhost",
        "brokerPort": "1883",
        "username": "test",
        "password": "test",
        "clientId": null,
        "secure": false,
        "caCertData": null,
        "clientCaCertData": null,
        "ignoreCertificateValidation": false,
        "cleanSession": true,
        "willFlag": false,
        "willQosLevel": 0,
        "willTopic": null,
        "willMessage": null,
        "willRetain": false,
        "keepAlivePeriod": 60,
        "autoReconnectTimeInSeconds": 10,
        "sslProtocols": "None",
        "protocolVersion": "Version_3_1"
      }
    },
    "TelemetryEndpoint": {
      "Type": "mqtt",
      "NativeSettings": {
        "topics": [
          {
            "topicName": "telemetry",
            "qosLevel": 1,
            "retain": false
          }
        ]
      }
    },
    "CommandEndpoint": {
      "Type": "mqtt",
      "NativeSettings": {
        "topics": [
          {
            "topicName": "commands",
            "qosLevel": 1,
            "retain": false
          }
        ]
      }
    },
    "ResponseEndpoint": {
      "Type": "mqtt",
      "NativeSettings": {
        "topics": [
          {
            "topicName": "responses",
            "qosLevel": 1,
            "retain": false
          }
        ]
      }
    }
  },
  "Persistance": {
    "Type": "inMemory",
    "NativeSettings": {
    }
  }
}
```
---

###  How to configure MQTT
Internally we use a 3rd party package to handle the specifics of the MQTT ([M2MqttDotnetCore](https://github.com/mohaqeq/paho.mqtt.m2mqtt)) protocol.

#### NativeSettings
  ```json
  In NativeSettings is where you can set all the MQTT specific settings that the M2MqttDotnetCore exposes
  Below is a list of such settings (Note: These are commen settings but it is not meant to be an exaustive list)

 {
  ...,
  "Communication": {
    "Shared": {
      "Type": "mqtt",
      "NativeSettings": {
        "brokerAddress": "localhost",
        "brokerPort": "1883",
        "username": "test",
        "password": "test",
        "clientId": null,
        "secure": false,
        "caCertData": null,
        "clientCaCertData": null,
        "ignoreCertificateValidation": false,
        "cleanSession": true,
        "willFlag": false,
        "willQosLevel": 0,
        "willTopic": null,
        "willMessage": null,
        "willRetain": false,
        "keepAlivePeriod": 60,
        "autoReconnectTimeInSeconds": 10,
        "sslProtocols": "None",
        "protocolVersion": "Version_3_1"
      }
    },
   ...
}
```
**Note**: Defaults will be applied for each setting not explicitly provided

###  How to Configure OPC UA

Internally we use a 3rd party package to handle the specifics of the OPC UA ([OPC UA .NET Standard](https://github.com/OPCFoundation/UA-.NETStandard)) protocol.

  ```json
  In NativeSettings is where you can set all the OPC UA specific settings that the 'OPC UA .NET Standard' exposes
  Below is a list of such settings (Note: These are commen settings but it is not meant to be an exaustive list)

 {
  ...,
  "OpcUa": {
    "DefaultServerBrowseDepth": 3,
    "NodeBrowseDepth": 1,
    "EnableRegisteredNodes": false,
    "SubscriptionBatchSize": 100,/* We will batch to try and not overload the OPC UA Server */
    "ReadBatchSize": 100,/* When sending commands for many nodes in a single request we will batch these command to try and not overload the OPC UA Server */
    "RegisterNodeBatchSize": 100,/* We will batch to try and not overload the OPC UA Server */
    "AwaitSessionLockTimeoutSecs": 3,
    "ReconnectIntervalSecs": 10,/* We try and re-establish connection incase of network problem - How many seconds should we wait before retrying to connect*/
    "NativeSettings": {
    }
  },
   ...
}
```
**Note**: Defaults will be applied for each setting not explicitly provided

---

## How to Run


##### Docker Build
  ```
  docker build -t mqqttest1 -f Dockerfile.MqttSample .
  ```
  
##### Docker Run
  ```
  docker run -t mqqttest1
  ```

## How to Test
-Message Models

