# OMP Hand on session for OPC UA Connector via MQTT

## How to Run

[Docker compose file](docker-compose.yaml)
  ```
  docker-compose up
  ```
  

## How to Test
Below is a list of commands that you can send over mqtt to the *Command* Topic of your choosing (same *Command* Topic that you configured in the connector prior to building).
- [Call Command](/commands/callCommand.json)
- [Read Command](/commands/readCommand.json)
- [Write Command](/commands/writeCommand.json)
- [Create Subscription(s)](/commands/createSubscriptionCommand.json)
- [Remove Subscription(s)](/commands/removeSubscriptionCommand.json)
- [Remove All Subscriptions](/commands/removeAllSubscriptionsCommand.json)

**Note**: The current message models (API) is being reviewed and will change in the comming weeks *[2021-10-18]*
