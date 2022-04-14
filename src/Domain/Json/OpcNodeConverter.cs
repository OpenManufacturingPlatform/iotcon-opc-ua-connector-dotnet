// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OMP.Connector.Domain.Json.Base;
using OMP.Connector.Domain.Models.OpcUa.Nodes;
using OMP.Connector.Domain.Models.OpcUa.Nodes.Base;
using Opc.Ua;

namespace OMP.Connector.Domain.Json
{
    public class OpcNodeConverter : CustomConverter<OpcNode>
    {
        protected override OpcNode Create(Type objectType, JObject jObject)
        {
            var nodeClass = this.GetPropertyValue<int>(jObject, "nodeClass");

            return (NodeClass)nodeClass switch
            {
                NodeClass.Object => (OpcNode)new OpcObject(),
                NodeClass.Variable => new OpcVariable(),
                NodeClass.Method => new OpcMethod(),
                NodeClass.ObjectType => new OpcObjectType(),
                NodeClass.VariableType => new OpcVariableType(),
                NodeClass.ReferenceType => new OpcReferenceType(),
                NodeClass.DataType => new OpcDataType(),
                NodeClass.View => new OpcView(),
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            => serializer.Serialize(writer, value, value.GetType());
    }
}