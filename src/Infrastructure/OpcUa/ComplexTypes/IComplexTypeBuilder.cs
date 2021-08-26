using System;
using Opc.Ua;

namespace OMP.Connector.Infrastructure.OpcUa.ComplexTypes
{
    /// <summary>
    /// Interface to dynamically build custom 
    /// enum types and structured types.
    /// </summary>
    public interface IComplexTypeBuilder
    {
        /// <summary>
        /// Target namespace information.
        /// </summary>
        string TargetNamespace { get; }
        int TargetNamespaceIndex { get; }

        /// <summary>
        /// Create an enum type from a binary schema definition.
        /// Available before OPC UA V1.04.
        /// </summary>
        Type AddEnumType(Opc.Ua.Schema.Binary.EnumeratedType enumeratedType);

        /// <summary>
        /// Create an enum type from an EnumDefinition in an ExtensionObject.
        /// Available since OPC UA V1.04 in the DataTypeDefinition attribute.
        /// </summary>
        Type AddEnumType(QualifiedName typeName, ExtensionObject typeDefinition);

        /// <summary>
        /// Create an enum type from an EnumValue property of a DataType node.
        /// Available before OPC UA V1.04.
        /// </summary>
        Type AddEnumType(QualifiedName typeName, ExtensionObject[] enumDefinition);

        /// <summary>
        /// Create an enum type from the EnumString array of a DataType node.
        /// Available before OPC UA V1.04.
        /// </summary>
        Type AddEnumType(QualifiedName typeName, LocalizedText[] enumDefinition);

        /// <summary>
        /// Create a complex type from a StructureDefinition.
        /// Available since OPC UA V1.04 in the DataTypeDefinition attribute.
        /// </summary>
        IComplexTypeFieldBuilder AddStructuredType(
            QualifiedName name,
            StructureDefinition structureDefinition);
    }
}