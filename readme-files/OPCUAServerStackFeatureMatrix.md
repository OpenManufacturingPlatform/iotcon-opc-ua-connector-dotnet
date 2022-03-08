|     | NodeJs | Eclipse Milo | OPCFoundation |
| --- | --- | --- | --- |  
|     | [https://node-opcua.github.io/](https://node-opcua.github.io/) | [https://github.com/eclipse/milo](https://github.com/eclipse/milo) | [https://github.com/OPCFoundation/UA-.NETStandard](https://github.com/OPCFoundation/UA-.NETStandard) |
| Programming language | JavaScript TypeScript | Java | C#  |
| Supports OPCU UA specification 1.03  <br>(Published 2015) | :heavy_check_mark: | :heavy_check_mark: | :heavy_check_mark: |
| Supports OPCU UA specification 1.04<br>(Published 2017) | :grey_question: | :x: | :heavy_check_mark: |
| NodesSet2.xml import | :heavy_check_mark: | :x: | :x: |
| Proprietary xml import | :x: | :x: | :heavy_check_mark: (predefined xml output from model compiler) |
| Add Namespace / Addressspace by code | :heavy_check_mark: | :heavy_check_mark: | :heavy_check_mark: |
| Server Discovery | :heavy_check_mark: | :heavy_check_mark: | :heavy_check_mark: |
| Read, Write, Call |:heavy_check_mark: | :heavy_check_mark: | :heavy_check_mark: |
| Registered Read and Write | :heavy_check_mark: | :heavy_check_mark: | :heavy_check_mark: |
| Subscriptions (create, modify, delete) | :heavy_check_mark: | :heavy_check_mark: | :heavy_check_mark: |
| Alarms & Conditions | :heavy_check_mark: | :heavy_check_mark: | :heavy_check_mark: |
| Value Simulation | :heavy_check_mark: | :heavy_check_mark: | :heavy_check_mark: |
| Struct/Extension Objects | :heavy_check_mark: | :heavy_check_mark: | :heavy_check_mark: |
| Support for special chars in node ids (including user defined types)  <br>(for simulation of Siemens OPC UA Models) | :grey_question: | :grey_question: | :heavy_check_mark: |
| Transport protocol | OPC.TCP - Binary<br>HTTP/HTTPS - Binary | OPC.TCP - Binary<br>HTTP/HTTPS - Binary | OPC.TCP - Binary<br>HTTP/HTTPS - Binary |
| Security Policies | None<br>Basic128Rsa15<br>Basic256<br>Basic256Sha256 | None<br>Basic256Sha256 | None<br>Basic256Sha256<br>Aes128\_Sha256\_RsaOaep<br>Aes256\_Sha256\_RsaPss |
| Authentication | Anonymous<br>Username / Password<br>X509 Certificate | Anonymous<br>Username / Password<br>X509 Certificate | Anonymous<br>Username / Password<br>X509 Certificate |
| Message Encryption and signing | Basic256Sha256 Sign & Encrypt | Basic256Sha256 Sign & Encrypt | Basic256Sha256 Sign & Encrypt (sign only also available)<br>Aes128\_Sha256\_RsaOaep Sign & Encrypt (sign only also available)<br>Aes256\_Sha256\_RsaPss Sign & Encrypt (sign only also available) |
| Certificates | node-opcua generates a server certificate and has a flag to accept unknown clients certificates automatically, which allows any client with a certificate to connect using a secured endpoint | Milo also generates the server certificates, however Milo requires clients certificates to be moved manually from the rejected certificates folder to the trusted certificates folder in order for clients to connect.<br>According to a Milo contributor, it's not possible to force the server to accept unknow clients certificates [https://githubmemory.com/repo/eclipse/milo/issues/655?page=2](https://githubmemory.com/repo/eclipse/milo/issues/655?page=2) | Server configuration has a flag to auto-accept unknown client certificates, which allows any client with a certificate to connect using a secured endpoint |