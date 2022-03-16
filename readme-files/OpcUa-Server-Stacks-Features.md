# OPC UA Server Stacks Comparison

The following matrix shows a comparison about the features of common OPC UA server stacks. The comparison was done in December 2021 with the focus on higher-level languages. For an up to date list of supported features, please refer to each individual OPC UA stack repository.

Cells with a question mark we could not determine exactly. If you have information on these, please create a pull request.

<br>

|            | NodeOPCUA | Eclipse Milo | OPCFoundation |
| :--------- | :----- | :----------- | :------------ |  
| Repository Link | [NodeOPCUA](https://node-opcua.github.io/) | [Eclipse](https://github.com/eclipse/milo) | [UA-.NETStandard](https://github.com/OPCFoundation/UA-.NETStandard) |
| Programming language | JavaScript, TypeScript | Java | C#  |
| Supports OPC UA specification 1.03 (*Published 2015*) | :heavy_check_mark: | :heavy_check_mark: | :heavy_check_mark: |
| Supports OPC UA specification 1.04 (*Published 2017*) | :grey_question: | :x: | :heavy_check_mark: |
| Import of NodesSet2.xml | :heavy_check_mark: | :x: | :x: |
| Proprietary xml import | :x: | :x: | :heavy_check_mark: (predefined xml output from model compiler) |
| Add Namespace / Address space by code | :heavy_check_mark: | :heavy_check_mark: | :heavy_check_mark: |
| Server Discovery | :heavy_check_mark: | :heavy_check_mark: | :heavy_check_mark: |
| Read, Write, Call |:heavy_check_mark: | :heavy_check_mark: | :heavy_check_mark: |
| Registered Read and Write | :heavy_check_mark: | :heavy_check_mark: | :heavy_check_mark: |
| Subscriptions (create, modify, delete) | :heavy_check_mark: | :heavy_check_mark: | :heavy_check_mark: |
| OPC UA Alarms & Conditions | :heavy_check_mark: | :heavy_check_mark: | :heavy_check_mark: |
| Value Simulation | :heavy_check_mark: | :heavy_check_mark: | :heavy_check_mark: |
| Struct/Extension Objects | :heavy_check_mark: | :heavy_check_mark: | :heavy_check_mark: |
| Support for special chars in node ids (including user defined types)  <br>(for simulation of Siemens OPC UA Models) | :grey_question: | :grey_question: | :heavy_check_mark: |
| Transport protocol | <ul><li>OPC.TCP - Binary</li><li>HTTP/HTTPS - Binary</li></ul> | <ul><li>OPC.TCP - Binary</li><li>HTTP/HTTPS - Binary</li></ul> | <ul><li>OPC.TCP - Binary</li><li>HTTP/HTTPS - Binary</li></ul> |
| Security Policies | <ul><li>None</li><li>Basic128Rsa15</li><li>Basic256</li><li>Basic256Sha256</li></ul>| <ul><li>None</li><li>Basic256Sha256</li></ul> | <ul><li>None</li><li>Basic256Sha256</li><li>Aes128\_Sha256\_RsaOaep</li><li>Aes256\_Sha256\_RsaPss</li><li>Basic256Sha256</li></ul>  |
| Authentication | <ul><li>Anonymous</li><li>Username / Password</li><li>X.509 Certificate</li></ul> | <ul><li>Anonymous</li><li>Username / Password</li><li>X.509 Certificate</li></ul> | <ul><li>Anonymous</li><li>Username / Password</li><li>X.509 Certificate</li></ul> |
| Message encryption and signing | Basic256Sha256 Sign & Encrypt | Basic256Sha256 Sign & Encrypt | <ul><li>Basic256Sha256 Sign & Encrypt (sign only also available)</li><li>Aes128\_Sha256\_RsaOaep Sign & Encrypt (sign only also available)</li><li>Aes256\_Sha256\_RsaPss Sign & Encrypt (sign only also available)</li></ul> |
| Certificates | NodeOPCUA generates a server certificate and has a flag to accept unknown clients certificates automatically, which allows any client with a certificate to connect using a secured endpoint. | Milo also generates the server certificates, however Milo requires clients certificates to be moved manually from the rejected certificates folder to the trusted certificates folder in order for clients to connect. | Server configuration has a flag to auto-accept unknown client certificates, which allows any client with a certificate to connect using a secured endpoint. |
