// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

using System.Diagnostics;
using System.Xml;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using OMP.PlantConnectivity.OpcUA.Sessions;
using Opc.Ua;

namespace OMP.PlantConnectivity.OpcUA.Serialization
{
    public class OpcUaNewtonsoftSerializer : IOmpOpcUaSerializer
    {
        private readonly IOpcUaSession opcUaSession;
        private readonly bool useReversibleEncoding;
        private readonly bool useGenericEncoderOnError;
        private readonly ILogger<OpcUaNewtonsoftSerializer> logger;
        private JsonEncoder encoder;

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
        }

        public void ResetEncoder()
        {
            this.encoder = CreateEncoder(opcUaSession, useReversibleEncoding);
        }

        public T? Deserialize<T>(string json, string? fieldName = default)
        {
            if (string.IsNullOrWhiteSpace(json))
                return default;

            //if (string.IsNullOrWhiteSpace(fieldName))
            //    fieldName = string.Empty;

            //var decoder = new JsonDecoder(json, opcUaSession.GetServiceMessageContext());
            //var roterutn = decoder.ReadDataValue(fieldName) as T;
            ////TODO: Implement this
            return JsonConvert.DeserializeObject<T>(json);

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
                    return JsonConvert.SerializeObject(value);

                var error = ex.Demystify();
                logger.LogError(error, "Unable to json encode value");
                throw error;
            }
        }

        private static JsonEncoder CreateEncoder(IOpcUaSession opcUaSession, bool useReversibleEncoding)
            => new(opcUaSession.GetServiceMessageContext(), useReversibleEncoding);

        //private string FindTypeAndSerialize<T>(T value, string? fieldName = default)
        //{
        //    if (string.IsNullOrWhiteSpace(fieldName))
        //        fieldName = string.Empty;

        //    //TODO: find a better way for this
        //    //encoder.WriteArray            
        //    if (value is bool boolean)
        //        encoder.WriteBoolean(fieldName, boolean);
        //    else if (value is IList<bool> bools)
        //        encoder.WriteBooleanArray(fieldName, bools);
        //    else if (value is byte bt)
        //        encoder.WriteByte(fieldName, bt);
        //    else if (value is IList<byte> bytes)
        //        encoder.WriteByteArray(fieldName, bytes);
        //    else if (value is string str)
        //        encoder.WriteString(fieldName, str);
        //    else if (value is IList<string> strings)
        //        encoder.WriteStringArray(fieldName, strings);
        //    else if (value is DataValue dataValue)
        //        encoder.WriteDataValue(fieldName, dataValue);
        //    else if (value is IList<DataValue> dataValueList)
        //        encoder.WriteDataValueArray(fieldName, dataValueList);
        //    else if (value is DateTime dateTime)
        //        encoder.WriteDateTime(fieldName, dateTime);
        //    else if (value is IList<DateTime> dateTimeList)
        //        encoder.WriteDateTimeArray(fieldName, dateTimeList);
        //    else if (value is DiagnosticInfo diagnosticInfo)
        //        encoder.WriteDiagnosticInfo(fieldName, diagnosticInfo);
        //    else if (value is IList<DiagnosticInfo> diagnosticInfoList)
        //        encoder.WriteDiagnosticInfoArray(fieldName, diagnosticInfoList);
        //    else if (value is double dbl)
        //        encoder.WriteDouble(fieldName, dbl);
        //    else if (value is IList<double> doubleList)
        //        encoder.WriteDoubleArray(fieldName, doubleList);
        //    //encoder.WriteEncodeable
        //    //encoder.WriteEncodeableArray
        //    //encoder.WriteEnumerated
        //    //encoder.WriteEnumeratedArray
        //    else if (value is ExpandedNodeId expandedNodeId)
        //        encoder.WriteExpandedNodeId(fieldName, expandedNodeId);
        //    else if (value is IList<ExpandedNodeId> expandedNodeIdmList)
        //        encoder.WriteExpandedNodeIdArray(fieldName, expandedNodeIdmList);
        //    else if (value is ExtensionObject extensionObject)
        //        encoder.WriteExtensionObject(fieldName, extensionObject);
        //    else if (value is IList<ExtensionObject> extensionObjectList)
        //        encoder.WriteExtensionObjectArray(fieldName, extensionObjectList);
        //    else if (value is float flt)
        //        encoder.WriteFloat(fieldName, flt);
        //    else if (value is IList<float> floatList)
        //        encoder.WriteFloatArray(fieldName, floatList);
        //    else if (value is Guid guid)
        //        encoder.WriteGuid(fieldName, guid);
        //    else if (value is IList<Guid> guidList)
        //        encoder.WriteGuidArray(fieldName, guidList);
        //    else if (value is short shrt)
        //        encoder.WriteInt16(fieldName, shrt);
        //    else if (value is IList<short> shortList)
        //        encoder.WriteInt16Array(fieldName, shortList);
        //    else if (value is int num)
        //        encoder.WriteInt32(fieldName, num);
        //    else if (value is IList<int> intList)
        //    {
        //        encoder.WriteVariant(fieldName, new Variant(intList));
        //        //encoder.WriteInt32Array(fieldName, intList);
        //    }
        //    else if (value is long lng)
        //        encoder.WriteInt64(fieldName, lng);
        //    else if (value is IList<long> longList)
        //        encoder.WriteInt64Array(fieldName, longList);
        //    else if (value is LocalizedText localizedText)
        //        encoder.WriteLocalizedText(fieldName, localizedText);
        //    else if (value is IList<LocalizedText> localizedTextList)
        //        encoder.WriteLocalizedTextArray(fieldName, localizedTextList);
        //    else if (value is NodeId nodeId)
        //        encoder.WriteNodeId(fieldName, nodeId);
        //    else if (value is IList<NodeId> nodeIdList)
        //        encoder.WriteNodeIdArray(fieldName, nodeIdList);
        //    else if (value is QualifiedName qualifiedName)
        //        encoder.WriteQualifiedName(fieldName, qualifiedName);
        //    else if (value is IList<QualifiedName> qualifiedNameList)
        //        encoder.WriteQualifiedNameArray(fieldName, qualifiedNameList);
        //    else if (value is sbyte sbt)
        //        encoder.WriteSByte(fieldName, sbt);
        //    else if (value is IList<sbyte> sbyteList)
        //        encoder.WriteSByteArray(fieldName, sbyteList);
        //    else if (value is StatusCode statusCode)
        //        encoder.WriteStatusCode(fieldName, statusCode);
        //    else if (value is IList<StatusCode> statusCodeList)
        //        encoder.WriteStatusCodeArray(fieldName, statusCodeList);
        //    else if (value is string strng)
        //        encoder.WriteString(fieldName, strng);
        //    else if (value is IList<string> stringList)
        //        encoder.WriteStringArray(fieldName, stringList);
        //    else if (value is ushort ushrt)
        //        encoder.WriteUInt16(fieldName, ushrt);
        //    else if (value is IList<ushort> ushortList)
        //        encoder.WriteUInt16Array(fieldName, ushortList);
        //    else if (value is uint unt)
        //        encoder.WriteUInt32(fieldName, unt);
        //    else if (value is IList<uint> uintList)
        //        encoder.WriteUInt32Array(fieldName, uintList);
        //    else if (value is ulong ulng)
        //        encoder.WriteUInt64(fieldName, ulng);
        //    else if (value is IList<ulong> ulongList)
        //        encoder.WriteUInt64Array(fieldName, ulongList);
        //    else if (value is Variant variant)
        //        encoder.WriteVariant(fieldName, variant);
        //    else if (value is IList<Variant> variantList)
        //        encoder.WriteVariantArray(fieldName, variantList);
        //    else if (value is XmlElement xmlElement)
        //        encoder.WriteXmlElement(fieldName, xmlElement);
        //    else if (value is IList<XmlElement> values)
        //        encoder.WriteXmlElementArray(fieldName, values);
        //    else if (value is IList<object> objectArray)// NB: Keep this if always at the END
        //        encoder.WriteObjectArray(fieldName, objectArray);

        //    return encoder.CloseAndReturnText();
        //}

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
            //encoder.WriteEncodeable
            //encoder.WriteEncodeableArray
            //encoder.WriteEnumerated
            //encoder.WriteEnumeratedArray
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
                return JsonConvert.SerializeObject(value);
            }

            return encoder.CloseAndReturnText();
        }
    }
}
