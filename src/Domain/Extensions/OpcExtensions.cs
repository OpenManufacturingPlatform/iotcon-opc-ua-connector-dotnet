using System;
using OMP.Connector.Domain.Models.OpcUa.Attributes;
using Opc.Ua;

namespace OMP.Connector.Domain.Extensions
{
    public static class OpcExtensions
    {
        public static Type GetSystemType(this OpcNodeId opcNodeId, int valueRank)
            => GetSystemType(opcNodeId.FriendlyName, valueRank);
        
        private static Type GetSystemType(string nodeIdentifier, int valueRank)
        {
            try
            {
                var nodeId = NodeId.Parse(nodeIdentifier);
                var systemType = DataTypes.GetSystemType(nodeId, EncodeableFactory.GlobalFactory);

                if (valueRank >= ValueRanks.OneOrMoreDimensions)
                    systemType = systemType.MakeArrayType();

                return systemType;
            }
            catch (Exception)
            {
                return typeof(object);
            }
        }
    }
}
