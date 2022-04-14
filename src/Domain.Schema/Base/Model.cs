// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

using System.ComponentModel;
using Newtonsoft.Json;
using OMP.Connector.Domain.Schema.Attributes;
using OMP.Connector.Domain.Schema.Attributes.Examples;
using OMP.Connector.Domain.Schema.Attributes.Regex;
using OMP.Connector.Domain.Schema.Interfaces;

namespace OMP.Connector.Domain.Schema.Base
{
    public abstract class Model<TMetaDataType> : IModel
        where TMetaDataType : IMetaData
    {
        [Guid]
        [GuidExamples]
        [JsonProperty("id", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public string Id { get; set; }

        public abstract string Namespace { get; set; }

        [JsonSchemaSchema]
        [SchemaExamples]
        [JsonProperty("$schema", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        [Description("The $schema of the business object")]
        public virtual string Schema { get; set; }

        [JsonProperty("metadata", NullValueHandling = NullValueHandling.Ignore)]
        public virtual TMetaDataType MetaData { get; set; }
    }
}