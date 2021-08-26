using System;
using Opc.Ua;

namespace OMP.Connector.Infrastructure.OpcUa.ComplexTypes
{
    /// <summary>
    /// Interface to build property fields.
    /// </summary>
    public interface IComplexTypeFieldBuilder
    {
        /// <summary>
        /// Build the StructureTypeId attribute for a complex type.
        /// </summary>
        void AddTypeIdAttribute(
            ExpandedNodeId complexTypeId,
            ExpandedNodeId binaryEncodingId,
            ExpandedNodeId xmlEncodingId
        );

        /// <summary>
        /// Create a property field of a class with get and set.
        /// </summary>
        void AddField(StructureField field, Type fieldType, int order);

        /// <summary>
        /// Finish the type creation and returns the new type.
        /// </summary>
        Type CreateType();
    }
}