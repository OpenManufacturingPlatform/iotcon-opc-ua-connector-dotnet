# OMP OPC UA Connector via Kafka

The **OMP OPC UA Connector** (hereafter referred to as the **Connector**) via an **Kafka** broker, provides a mechanism to interact with **OPC UA Servers** over **Kafka**, wihtout having to implement the OPC UA protocol. The **Connector** allows you to:
- *Subscribe* to nodes in order to receive telemetry messages when values change on these nodes
- Do *Read* operations on nodes
- Do *Write* operations on nodes
- Do *Call* operations on nodes
- Browse nodes
- ...and more

This document serves as a guide to *Configure*, *Build*, *Run* and *Test* a generic implementation of the connector utilizing an **Kafka** broker.

## Index

- [Prerequisites](#prerequisites)
- [How to Configure](#how-to-configure)
- [How to Run](#how-to-run)
- [How to Test](#how-to-test)



## Prerequisites
- Docker - To Build and run the Connector via Docker
- OPC UA Server
- Kafka Broker

---
## How to Configure the Connector

**Very important**: An example settings file is provided **moduleSettings.example.json** (/samples/KafkaSample/moduleSettings.example.json). Please copy the file and change the name to **moduleSettings.json** (/samples/KafkaSample/moduleSettings.json). Change the required settings to suit your environment.

##  Understanding how the *Connector's* Configuration works
To understand how the connector's configuration works, it is important to understand how/where the Connector receives command requests and sends response/telemetry messages to and from the outside world.

| Endpoint        | Description   |
| ------------- | ------------- |
| Command | This is the endpoint (Kafka Topic) where the outside world sends commands to the *Connector*  |
| Response | This is the endpoint (Kafka Topic) where the *Connector* sends the results of commands back  |
| Telemetry | This is the endpoint (Kafka Topic) where the *Connector* publishes telemetry messages for subscriptions  |


###  Configuring the Endpoints for Kafka
The configuration file has a couple of sections, but for brevity, we focus on the *Communication* section here.
Under *Communication* there are 4 sub-sections of importance
- Shared
```json
 {
  ...,
  "Communication": {    
    "Shared": {
      "Type": "kafka",
       ...
```
The *Shared* section groups Kafka settings (e.g Username, Password,..) that are shared between different endpoints (when the same **Kafka Broker** is used for all endpoints (Topics)). This reduces unnecessary repetition and bloating of the settings file.

**Note**: The Shared settings are read and applied first. Settings in subsequent sections will override those with the same name that appear in the Shared settings. For example, if the brokerAddress is set in Shared as 'Broker-Shared', and it is set again in the CommandEndpoint section (under NativeSettings) as 'Broker-Command', the final value of brokerAddress will be 'Broker-Command' and not 'Broker-Shared'.

- Command Endpoint
```json
 {
  ...,
  "CommandEndpoint": {
      "Type": "kafka",
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
This is where the topic name, QoS Level and any other settings specific to the *Command Endpoint* are set.

- Response Endpoint
```json
 {
  ...,
  "ResponseEndpoint": {
      "Type": "kafka",
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
This is where the topic name, QoS Level and any other settings specific to the *Response Endpoint* are set.

- Telemetry Endpoint
```json
{
  ...,
    "TelemetryEndpoint": {
      "Type": "kafka",
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

This is where the topic name, QoS Level and any other settings specific to the *Telemetry Endpoint* are set.

### Complete example of the *Communication* section in the settigns file:

```json
 {
  ...,
  "Communication": {
    "SchemaUrl": "https://someSchemaStore.com/schemas/",
    "Shared": {
      "Type": "kafka",
      "NativeSettings": {
        "bootstrapServers": "localhost:9092",
        "debug": "consumer,cgrp,topic,fetch",
        "saslMechanism": "PLAIN",
        "saslUsername": "test",
        "saslPassword": "test",
        "securityProtocol": "SaslSsl",
        "sslCaLocation": "cert.pem",
        "socketKeepaliveEnable": "True",
        "SocketTimeoutMs": 30000,
        "acks": -1,
        "topicMetadataRefreshIntervalMs": 300000,
        "autoOffsetReset": "earliest",
        "enableAutoCommit": "False",
        "enablePartitionEof": "False",
        "statisticsIntervalMs": 5000,
        "groupId": "connectorCG"
      }
    },
    "TelemetryEndpoint": {
      "Type": "kafka",
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
      "Type": "kafka",
      "NativeSettings": {
        "groupId": "connectorCommandCG",
        "topics": [
          {
            "topicName": "commands",
            "qosLevel": 1,
            "retain": false
          },
        ]
      }
    },
    "ResponseEndpoint": {
      "Type": "kafka",
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

###  How to configure Kafka
Internally the connector uses a 3rd party package that handles the specifics of the Kafka protocol ([Confluent.Kafka](https://github.com/confluentinc/confluent-kafka-dotnet/)).

#### NativeSettings
  ```json
  All the Kafka specific settings that are exposed by the 'Confluent.Kafka' package, can be configured in this NativeSettings section.
  Below is a list of these settings (Note: This list contains the most common settings, but it is not meant to be an exhaustive list)

 {
  ...,
  "Communication": {
    "Shared": {
      "Type": "kafka",
      "NativeSettings": {
        "bootstrapServers": "localhost:9092",
        "debug": "consumer,cgrp,topic,fetch",
        "saslMechanism": "PLAIN",
        "saslUsername": "test",
        "saslPassword": "test",
        "securityProtocol": "SaslSsl",
        "sslCaLocation": "cert.pem",
        "socketKeepaliveEnable": "True",
        "SocketTimeoutMs": 30000,
        "acks": -1,
        "topicMetadataRefreshIntervalMs": 300000,
        "autoOffsetReset": "earliest",
        "enableAutoCommit": "False",
        "enablePartitionEof": "False",
        "statisticsIntervalMs": 5000,
        "groupId": "connectorCG"
      }
    },
   ...
}
```
**Note**: Defaults will be applied for settings that are not explicitly configured.

###  How to Configure OPC UA

Internally the connector uses a 3rd party package that handles the specifics of the OPC UA protocol ([OPC UA .NET Standard](https://github.com/OPCFoundation/UA-.NETStandard)).

  ```json
  All the library specific settings that are exposed by the 'OPC UA .Net Standard' package, can be configured in this NativeSettings section.
  Below is a list of these settings (Note: This list contains the most common settings, but it is not meant to be an exhaustive list)

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
**Note**:  Defaults will be applied for settings that are not explicitly configured.

---

## How to Run

[Kafka Docker file](Dockerfile.KafkaSample)
  ```
  docker build -t kafkatest1 -f Dockerfile.KafkaSample .
  ```
  
##### Docker Run
  ```
  docker run -t kafkatest1
  ```

## How to Test
Below is a list of commands that you can send over kafka to the *Command* Topic of your choosing (same *Command* Topic that you configured in the connector prior to building).
- [Call Command](/samples/MessageModels/Commands/CallCommand.json)
- [Read Command](/samples/MessageModels/Commands/ReadCommand.json)
- [Write Command](/samples/MessageModels/Commands/WriteCommand.json)
- [Create Subscription(s)](/samples/MessageModels/Subscriptions/CreateSubscriptionRequest.json)
- [Remove Subscription(s)](/samples/MessageModels/Subscriptions/RemoveSubscriptionRequest.json)
- [Remove All Subscriptions](/samples/MessageModels/Subscriptions/RemoveAllSubscriptionsRequest.json)

**Note**: The current message models (API) is being reviewed and will change in the comming weeks *[2021-10-18]*

------------------------------------------------
## What is coming next?
Next we aim to provide an end-to-end environment via a docker compose file. Which will setup a OPC UA Server, Kafka Broker the actual *Connector* and a small sample app to send *commands* and see the results.