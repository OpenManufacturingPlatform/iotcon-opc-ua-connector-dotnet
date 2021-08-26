using System;
using System.Reflection;
using System.Runtime.Serialization;
using OMP.Connector.Infrastructure.OpcUa.ComplexTypes.Types;

namespace OMP.Connector.Infrastructure.OpcUa.ComplexTypes
{
    /// <summary>
    /// Attribute for a complex type property.
    /// </summary>
    public class ComplexTypePropertyAttribute
    {
        public readonly PropertyInfo PropertyInfo;
        public readonly StructureFieldAttribute FieldAttribute;
        public readonly DataMemberAttribute DataAttribute;

        public ComplexTypePropertyAttribute(
            PropertyInfo propertyInfo,
            StructureFieldAttribute fieldAttribute,
            DataMemberAttribute dataAttribute
        )
        {
            this.PropertyInfo = propertyInfo;
            this.FieldAttribute = fieldAttribute;
            this.DataAttribute = dataAttribute;
        }

        /// <summary>
        /// PropertyInfo
        /// </summary>
        public string Name => this.PropertyInfo.Name;

        /// <summary>
        /// Get the value of a property.
        /// </summary>
        public object GetValue(object o)
        {
            return this.PropertyInfo.GetValue(o);
        }

        /// <summary>
        /// Set the value of a property.
        /// </summary>
        public void SetValue(object o, object v)
        {
            this.PropertyInfo.SetValue(o, v);
        }

        /// <summary>
        /// Type of property.
        /// </summary>
        public Type PropertyType => this.PropertyInfo.PropertyType;

        /// <summary>
        /// StructureFieldAttribute
        /// </summary>
        public bool IsOptional => this.FieldAttribute.IsOptional;
        public int ValueRank => this.FieldAttribute.ValueRank;

        /// <summary>
        /// DataMemberAttribute
        /// </summary>
        public int Order => this.DataAttribute.Order;

        /// <summary>
        /// Optional field mask of the property.
        /// </summary>
        public UInt32 OptionalFieldMask;
    }
}