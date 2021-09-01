using System;
using Opc.Ua;

namespace OMP.Connector.Infrastructure.OpcUa.ComplexTypes
{
    /// <summary>
    /// DataType is not supported due to structure or value rank.
    /// </summary>
    public class DataTypeNotSupportedException : Exception
    {
        public ExpandedNodeId nodeId;
        public string typeName;

        public DataTypeNotSupportedException(ExpandedNodeId nodeId)
        {
            this.nodeId = nodeId;
        }

        public DataTypeNotSupportedException(string typeName, string message)
            : base(message)
        {
            this.nodeId = NodeId.Null;
            this.typeName = typeName;
        }

        public DataTypeNotSupportedException(ExpandedNodeId nodeId, string message)
            : base(message)
        {
            this.nodeId = nodeId;
        }

        public DataTypeNotSupportedException(ExpandedNodeId nodeId, string message, Exception inner)
            : base(message, inner)
        {
            this.nodeId = nodeId;
        }
    }
}