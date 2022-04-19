## OPC UA Server Stack Feature Matrix

<br>
For an up to date list of supported features, please refer to each OPC UA stack repository.

<br>
<br>

|            | NodeOPCUA | Eclipse Milo | OPCFoundation |
| :--------- | :----- | :----------- | :------------ |  
| Github Repository | [NodeOPCUA](https://node-opcua.github.io/) | [Milo](https://github.com/eclipse/milo) | [UA-.NETStandard](https://github.com/OPCFoundation/UA-.NETStandard) |
| Programming Language | JavaScript TypeScript | Java | C#  |
| Supports OPCU UA specification 1.03 (Published 2015) | :heavy_check_mark: | :heavy_check_mark: | :heavy_check_mark: |
| Supports OPCU UA specification 1.04 (Published 2017) | :grey_question: | :x: | :heavy_check_mark: |
| NodesSet2.xml import | :heavy_check_mark: | :x: | :x: (Proprietary xml import) |
| Add Namespace / Addressspace by code | :heavy_check_mark: | :heavy_check_mark: | :heavy_check_mark: |
| Value Simulation | :heavy_check_mark: | :heavy_check_mark: | :heavy_check_mark: |
| Server Discovery | :heavy_check_mark: | :heavy_check_mark: | :heavy_check_mark: |
| Read, Write, Call |:heavy_check_mark: | :heavy_check_mark: | :heavy_check_mark: |
| Registered Read and Write | :heavy_check_mark: | :heavy_check_mark: | :heavy_check_mark: |
| Subscriptions (create, modify, delete) | :heavy_check_mark: | :heavy_check_mark: | :heavy_check_mark: |
| Alarms & Conditions | :heavy_check_mark: | :heavy_check_mark: | :heavy_check_mark: |
| Struct/Extension Objects | :heavy_check_mark: | :heavy_check_mark: | :heavy_check_mark: |
| Auto Accept Unknown Client Certificate | :heavy_check_mark: | :x: | :heavy_check_mark: |
| Security Policies | None<br>Basic128Rsa15<br>Basic256<br>Basic256Sha256 | None<br>Basic256Sha256 | None<br>Basic256Sha256<br>Aes128\_Sha256\_RsaOaep<br>Aes256\_Sha256\_RsaPss |
| Authentication | Anonymous<br>Username / Password<br>X509 Certificate | Anonymous<br>Username / Password<br>X509 Certificate | Anonymous<br>Username / Password<br>X509 Certificate |
| Message Encryption & Signing | Basic256Sha256 Sign & Encrypt | None <br> Basic128Rsa15 <br> Basic256 <br> Basic256Sha256 <br> Aes128_Sha256_RsaOaep <br> Aes256_Sha256_RsaPss | Basic256Sha256 Sign & Encrypt (sign only also available)<br>Aes128\_Sha256\_RsaOaep Sign & Encrypt (sign only also available)<br>Aes256\_Sha256\_RsaPss Sign & Encrypt (sign only also available) |
| Transport protocol | OPC.TCP - Binary<br>HTTP/HTTPS - Binary | OPC.TCP - Binary<br>HTTP/HTTPS - Binary | OPC.TCP - Binary<br>HTTP/HTTPS - Binary |
