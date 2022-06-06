// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using OMP.Connector.Domain.Models.Command.Requests.Basic.NodeCommands;
using OMP.Connector.Domain.Models.OpcUa;
using OMP.Connector.Domain.Models.OpcUa.Attributes;
using OMP.Connector.Domain.Models.OpcUa.Nodes;
using OMP.Connector.Domain.Schema;
using OMP.Connector.Domain.Schema.Abstraction;
using OMP.Connector.Domain.Schema.Messages;
using OMP.Connector.Domain.Schema.Request;
using OMP.Connector.Infrastructure.AutoMapper.Converters;
using Opc.Ua;
using Opc.Ua.Client;
using CallRequest = OMP.Connector.Domain.Schema.Request.Control.CallRequest;
using Formatting = Newtonsoft.Json.Formatting;
using OpcNode = OMP.Connector.Domain.Models.OpcUa.Nodes.Base.OpcNode;
using WriteRequest = OMP.Connector.Domain.Schema.Request.Control.WriteRequest;
using Xml = System.Xml;

namespace OMP.Connector.Infrastructure.AutoMapper
{
    public class EdgeProfile : Profile
    {
        private readonly ILoggerFactory _loggerFactory;

        public EdgeProfile(ILoggerFactory loggerFactory)
        {
            _loggerFactory = loggerFactory;

            NodeMappings();
            HelperMappings();
            CommandMappings();
        }

        private void NodeMappings()
        {
            CreateMap<Node, OpcNode>()
                .Include<DataTypeNode, OpcDataType>()
                .Include<MethodNode, OpcMethod>()
                .Include<ObjectNode, OpcObject>()
                .Include<ObjectTypeNode, OpcObjectType>()
                .Include<ReferenceTypeNode, OpcReferenceType>()
                .Include<VariableNode, OpcVariable>()
                .Include<VariableTypeNode, OpcVariableType>()
                .Include<ViewNode, OpcView>();

            CreateMap<DataTypeNode, OpcDataType>();
            CreateMap<MethodNode, OpcMethod>();
            CreateMap<ObjectNode, OpcObject>();
            CreateMap<Argument, OpcArgument>();
            CreateMap<ObjectTypeNode, OpcObjectType>();
            CreateMap<ReferenceTypeNode, OpcReferenceType>();

            CreateMap<VariableNode, VariableNodeWithType>();
            CreateMap<VariableNode, OpcVariable>()
                .Include<VariableNodeWithType, OpcVariable>()
                .ForMember(dest => dest.Value, opt => opt.MapFrom(src => src.Value.Value.ToString()));

            CreateMap<VariableNodeWithType, OpcVariable>()
                .ForMember(dest => dest.Value, opt => opt.MapFrom(src => src.Value.Value.ToString()))
                .ForMember(dest => dest.DataTypeName, opt => opt.MapFrom(src => src.DataTypeName));

            CreateMap<VariableTypeNode, OpcVariableType>()
                .Include<VariableTypeNodeWithType, OpcVariableType>()
                .ForMember(dest => dest.Value, opt => opt.MapFrom(src => src.Value.Value.ToString()));

            CreateMap<VariableTypeNodeWithType, OpcVariableType>()
                .ForMember(dest => dest.Value, opt => opt.MapFrom(src => src.Value.Value.ToString()))
                .ForMember(dest => dest.DataTypeName, opt => opt.MapFrom(src => src.DataTypeName));

            CreateMap<VariableTypeNode, VariableTypeNodeWithType>();

            CreateMap<ViewNode, OpcView>();
            CreateMap<NodeId, OpcNodeId>()
                .ForMember(dest => dest.FriendlyName, opt => opt.MapFrom(src => src.ToString()));
            CreateMap<ExpandedNodeId, OpcExpandedNodeId>();
            CreateMap<LocalizedText, OpcLocalizedText>();
            CreateMap<QualifiedName, OpcQualifiedName>();
            CreateMap<StatusCode, OpcStatusCode>();
            CreateMap<DataValue, OpcDataValue>()
                .ForMember(dest => dest.Value, opt => opt.MapFrom(f => this.DataValueValueConverter(f.Value)));
            CreateMap<Xml.XmlElement, OpcXmlElement>();
            CreateMap<Uuid, Guid>().ConvertUsing(c => new Guid(c.GuidString));

            CreateMap<Variant, OpcVariant>();
            CreateMap<TypeInfo, OpcTypeInfo>();

            this.CreateMap<VariableNode, BrowsedOpcNode>(MemberList.None)
                .ForMember(dest => dest.NodeClass, opt => opt.MapFrom(src => src.NodeClass.ToString()))
                .ForMember(dest => dest.BrowseName, opt => opt.MapFrom(src => src.BrowseName.Name))
                .ForMember(dest => dest.DisplayName, opt => opt.MapFrom(src => src.DisplayName.Text))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description.Text))
                .ForMember(dest => dest.WriteMask, opt => opt.MapFrom(src => src.WriteMask.ToString()))
                .ForMember(dest => dest.UserWriteMask, opt => opt.MapFrom(src => src.UserWriteMask.ToString()))
                .ForMember(dest => dest.NodeType, opt => opt.MapFrom(src => src.TypeId.ToString()))
                .ForMember(dest => dest.DataType, opt => opt.MapFrom(src => src.DataType.ToString()))
                .ForMember(dest => dest.ValueRank, opt => opt.MapFrom(src => src.ValueRank.ToString()))
                .ForMember(dest => dest.AccessLevel, opt => opt.MapFrom(src => src.AccessLevel.ToString()))
                .ForMember(dest => dest.UserAccessLevel, opt => opt.MapFrom(src => src.UserAccessLevel.ToString()))
                .ForMember(dest => dest.MinimumSamplingInterval, opt => opt.MapFrom(src => src.MinimumSamplingInterval.ToString()))
                .ForMember(dest => dest.Historizing, opt => opt.MapFrom(src => src.Historizing))
                //.ForAllOtherMembers(opt => opt.Ignore())
                ;

            this.CreateMap<BrowsedNode, DiscoveredOpcNode>(MemberList.None)
                .ForMember(dest => dest.NodeId, opt => opt.MapFrom(src => src.Node.NodeId))
                .ForMember(dest => dest.NodeClass, opt => opt.MapFrom(src => src.Node.NodeClass))
                .ForMember(dest => dest.DisplayName, opt => opt.MapFrom(src => src.Node.DisplayName))
                .ForMember(dest => dest.BrowseName, opt => opt.MapFrom(src => src.Node.BrowseName))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Node.Description))
                .ForMember(dest => dest.UserWriteMask, opt => opt.MapFrom(src => src.Node.UserWriteMask))
                .ForMember(dest => dest.WriteMask, opt => opt.MapFrom(src => src.Node.WriteMask))
                .ForMember(dest => dest.ChildNodes, opt => opt.MapFrom(src => src.ChildNodes))
                //.ForAllOtherMembers(opt => opt.Ignore())
                ;
        }

        public object DataValueValueConverter(object value)
            => value switch
            {
                ExtensionObject eo => eo.Body,
                ExtensionObject[] eoArray => new List<object>(eoArray.Select(x => x.Body)),
                _ => value
            };

        private void HelperMappings()
        {
            this.CreateMap<BrowsedNode, OpcNode>(MemberList.None)
                .ConstructUsing((s, ctx) => ctx.Mapper.Map<OpcNode>(s.Node))
                .ForMember(q => q.ChildNodes, option => option.MapFrom(q => q.ChildNodes))
                //.ForAllOtherMembers(o => o.Ignore())
                ;

            CreateMap<EndpointDescription, OpcUaEndpoint>()
                .ForMember(dest => dest.ServerCertificate,
                    opt => opt.MapFrom(src => JsonConvert.SerializeObject(src.ServerCertificate, Formatting.None)));

            CreateMap<OpcUaEndpoint, EndpointDescription>()
                .ForMember(dest => dest.ServerCertificate, opt => opt.MapFrom(src => JsonConvert.DeserializeObject<byte[]>($"\"{src.ServerCertificate}\"")))
                .ForMember(dest => dest.SecurityLevel, opt => opt.MapFrom(src => byte.Parse(src.SecurityLevel)))
                .ForMember(dest => dest.SecurityMode, opt => opt.MapFrom(src => JsonConvert.DeserializeObject<MessageSecurityMode>($"\"{src.SecurityMode}\"")))
                .ForMember(dest => dest.UserIdentityTokens, opt => opt.Ignore());

            this.CreateMap<MonitoredItem, OpcUaMonitoredItem>(MemberList.None)
                .ForMember(dest => dest.NodeId, opt => opt.MapFrom(src => src.ResolvedNodeId.ToString()))
                //.ForAllOtherMembers(opt => opt.Ignore())
                ;

            #region Cloning request without commands
            CreateMap<CommandRequest, CommandRequest>();
            CreateMap<RequestPayload, RequestPayload>()
                .ForMember(dest => dest.Requests, opt => opt.Ignore());
            #endregion
        }

        private void CommandMappings()
        {
            CreateMap<WriteRequest, WriteValue>()
                .ForMember(dest => dest.NodeId, opt => opt.MapFrom(src => NodeId.Parse(src.NodeId)))
                .ForMember(dest => dest.AttributeId, opt => opt.MapFrom(src => Attributes.Value))
                .ForMember(dest => dest.Value, opt => opt.ConvertUsing(new DataValueConverter(_loggerFactory.CreateLogger<DataValueConverter>()), src => src));

            CreateMap<WriteRequestWrapper, WriteValue>()
                .ForMember(dest => dest.NodeId, opt => opt.MapFrom(src => NodeId.Parse(src.RegisteredNodeId)))
                .ForMember(dest => dest.AttributeId, opt => opt.MapFrom(src => Attributes.Value))
                .ForMember(dest => dest.Value, opt => opt.ConvertUsing(new DataValueConverter(_loggerFactory.CreateLogger<DataValueConverter>()), src => src));

            CreateMap<CallRequest, CallMethodRequest>()
                .ForMember(dest => dest.MethodId, opt => opt.MapFrom(src => NodeId.Parse(src.NodeId)))
                .ForMember(dest => dest.InputArguments,
            opt => opt.MapFrom(src => new VariantCollection(
                src.Arguments.Select(i => new Variant(new Opc.Ua.KeyValuePair() { Key = i.Key, Value = i.Value })))));

            CreateMap<WriteRequest, WriteRequestWrapper>();
        }
    }
}