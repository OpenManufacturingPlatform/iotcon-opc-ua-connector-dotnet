FROM registry.access.redhat.com/ubi8/ubi:latest as builder

RUN dnf -y install dotnet-sdk-5.0

COPY . /usr/src

WORKDIR /usr/src

RUN dotnet publish --nologo --configuration Release samples/MqttSample/MqttSample.csproj -p:PublishProfile=PublishSingleFile

FROM registry.access.redhat.com/ubi8/ubi-minimal:latest

RUN microdnf -y install dotnet-runtime-5.0

COPY --from=builder /usr/src/samples/MqttSample/bin/Release/net5.0/publish /publish

CMD [ "publish/OMP.Connector.EdgeModule" ]
