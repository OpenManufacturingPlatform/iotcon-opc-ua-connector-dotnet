// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

using System.Diagnostics;
using System.Xml;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using OMP.PlantConnectivity.OpcUa.Sessions;
using Opc.Ua;

namespace OMP.PlantConnectivity.OpcUa.Serialization
{
    public class OpcUaNewtonsoftSerializer : IOmpOpcUaSerializer
    {
        private readonly IOpcUaSession opcUaSession;
        private readonly bool useReversibleEncoding;
        private readonly bool useGenericEncoderOnError;
        private readonly ILogger<OpcUaNewtonsoftSerializer> logger;
        private JsonEncoder encoder;
        private JsonSerializerSettings jsonSerializerSettings;

        public OpcUaNewtonsoftSerializer(
            IOpcUaSession opcUaSession, 
            bool useReversibleEncoding,
            bool useGenericEncoderOnError,
            ILogger<OpcUaNewtonsoftSerializer> logger)
        {
            this.opcUaSession = opcUaSession;
            this.useReversibleEncoding = useReversibleEncoding;
            this.useGenericEncoderOnError = useGenericEncoderOnError;
            this.logger = logger;
            this.encoder = CreateEncoder(opcUaSession, useReversibleEncoding);
            this.jsonSerializerSettings = new JsonSerializerSettings();
            jsonSerializerSettings.Converters.Add(new Newtonsoft.Json.Converters.StringEnumConverter());
        }

        public void ResetEncoder()
        {
            this.encoder = CreateEncoder(opcUaSession, useReversibleEncoding);
        }

        public T? Deserialize<T>(string json, string? fieldName = default)
        {
            if (string.IsNullOrWhiteSpace(json))
                return default;

            return JsonConvert.DeserializeObject<T>(json, jsonSerializerSettings);
        }

        public string Serialize<T>(T value, string? fieldName = default)
        {
            try
            {
                ResetEncoder();
                return FindTypeAndSerialize(value, fieldName);
            }
            catch (Exception ex)
            {                
                if (useGenericEncoderOnError)
                    return JsonConvert.SerializeObject(value, jsonSerializerSettings);

                var error = ex.Demystify();
                logger.LogError(error, "Unable to json encode value");
                throw error;
            }
        }

        private static JsonEncoder CreateEncoder(IOpcUaSession opcUaSession, bool useReversibleEncoding)
            => new(opcUaSession.GetServiceMessageContext(), useReversibleEncoding);

        private string FindTypeAndSerialize<T>(T value, string? fieldName = default)
        {
            if (string.IsNullOrWhiteSpace(fieldName))
                fieldName = string.Empty;
            
            if (value is DataValue dataValue)
                encoder.WriteDataValue(fieldName, dataValue);
            else if (value is IList<DataValue> dataValueList)
                encoder.WriteDataValueArray(fieldName, dataValueList);
            else if (value is DiagnosticInfo diagnosticInfo)
                encoder.WriteDiagnosticInfo(fieldName, diagnosticInfo);
            else if (value is IList<DiagnosticInfo> diagnosticInfoList)
                encoder.WriteDiagnosticInfoArray(fieldName, diagnosticInfoList);
            else if (value is ExpandedNodeId expandedNodeId)
                encoder.WriteExpandedNodeId(fieldName, expandedNodeId);
            else if (value is IList<ExpandedNodeId> expandedNodeIdmList)
                encoder.WriteExpandedNodeIdArray(fieldName, expandedNodeIdmList);
            else if (value is ExtensionObject extensionObject)
                encoder.WriteExtensionObject(fieldName, extensionObject);
            else if (value is IList<ExtensionObject> extensionObjectList)
                encoder.WriteExtensionObjectArray(fieldName, extensionObjectList);
            else if (value is LocalizedText localizedText)
                encoder.WriteLocalizedText(fieldName, localizedText);
            else if (value is IList<LocalizedText> localizedTextList)
                encoder.WriteLocalizedTextArray(fieldName, localizedTextList);
            else if (value is NodeId nodeId)
                encoder.WriteNodeId(fieldName, nodeId);
            else if (value is IList<NodeId> nodeIdList)
                encoder.WriteNodeIdArray(fieldName, nodeIdList);
            else if (value is QualifiedName qualifiedName)
                encoder.WriteQualifiedName(fieldName, qualifiedName);
            else if (value is IList<QualifiedName> qualifiedNameList)
                encoder.WriteQualifiedNameArray(fieldName, qualifiedNameList);
            else if (value is StatusCode statusCode)
                encoder.WriteStatusCode(fieldName, statusCode);
            else if (value is IList<StatusCode> statusCodeList)
                encoder.WriteStatusCodeArray(fieldName, statusCodeList);
            else if (value is Variant variant)
                encoder.WriteVariant(fieldName, variant);
            else if (value is IList<Variant> variantList)
                encoder.WriteVariantArray(fieldName, variantList);
            else if (value is XmlElement xmlElement)
                encoder.WriteXmlElement(fieldName, xmlElement);
            else if (value is IList<XmlElement> values)
                encoder.WriteXmlElementArray(fieldName, values);
            else if (value is IList<object> objectArray)// NB: Keep this if-statement always at the END
                encoder.WriteObjectArray(fieldName, objectArray);
            else
            {
                // for all simple types and their lists equivalents, use normal serialization
                return JsonConvert.SerializeObject(value, jsonSerializerSettings);
            }

            return encoder.CloseAndReturnText();
        }
    }
}
