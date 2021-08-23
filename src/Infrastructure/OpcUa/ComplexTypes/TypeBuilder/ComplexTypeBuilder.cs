/* ========================================================================
 * Copyright (c) 2005-2019 The OPC Foundation, Inc. All rights reserved.
 *
 * OPC Foundation MIT License 1.00
 * 
 * Permission is hereby granted, free of charge, to any person
 * obtaining a copy of this software and associated documentation
 * files (the "Software"), to deal in the Software without
 * restriction, including without limitation the rights to use,
 * copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the
 * Software is furnished to do so, subject to the following
 * conditions:
 * 
 * The above copyright notice and this permission notice shall be
 * included in all copies or substantial portions of the Software.
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
 * EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
 * OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
 * NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
 * HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
 * WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
 * FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
 * OTHER DEALINGS IN THE SOFTWARE.
 *
 * The complete license agreement can be found here:
 * http://opcfoundation.org/License/MIT/1.00/
 * ======================================================================*/


using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using OMP.Connector.Infrastructure.Kafka.ComplexTypes.Types;
using Opc.Ua;

namespace OMP.Connector.Infrastructure.Kafka.ComplexTypes.TypeBuilder
{
    /// <summary>
    /// Build an assembly with custom enum types and 
    /// complex types based on the BaseComplexType class
    /// using System.Reflection.Emit.
    /// </summary>
    public class ComplexTypeBuilder : IComplexTypeBuilder
    {
        #region Constructors
        /// <summary>
        /// Initializes the object with default values.
        /// </summary>
        public ComplexTypeBuilder(
            AssemblyModule moduleFactory,
            string targetNamespace,
            int targetNamespaceIndex,
            string moduleName = null)
        {
            this.m_targetNamespace = targetNamespace;
            this.m_targetNamespaceIndex = targetNamespaceIndex;
            this.m_moduleName = this.FindModuleName(moduleName, targetNamespace, targetNamespaceIndex);
            this.m_moduleBuilder = moduleFactory.GetModuleBuilder();
        }
        #endregion

        #region Public Members
        public string TargetNamespace => this.m_targetNamespace;
        public int TargetNamespaceIndex => this.m_targetNamespaceIndex;

        /// <summary>
        /// Create an enum type from a binary schema definition.
        /// Available before OPC UA V1.04.
        /// </summary>
        public Type AddEnumType(Opc.Ua.Schema.Binary.EnumeratedType enumeratedType)
        {
            if (enumeratedType == null)
            {
                throw new ArgumentNullException(nameof(enumeratedType));
            }
            var enumBuilder = this.m_moduleBuilder.DefineEnum(
                this.GetFullQualifiedTypeName(enumeratedType.Name),
                TypeAttributes.Public,
                typeof(int));
            enumBuilder.DataContractAttribute(this.m_targetNamespace);
            foreach (var enumValue in enumeratedType.EnumeratedValue)
            {
                var newEnum = enumBuilder.DefineLiteral(enumValue.Name, enumValue.Value);
                newEnum.EnumMemberAttribute(enumValue.Name, enumValue.Value);
            }
            return enumBuilder.CreateTypeInfo();
        }

        /// <summary>
        /// Create an enum type from an EnumDefinition in an ExtensionObject.
        /// Available since OPC UA V1.04 in the DataTypeDefinition attribute.
        /// </summary>
        public Type AddEnumType(QualifiedName typeName, ExtensionObject typeDefinition)
        {
            var enumDefinition = typeDefinition.Body as EnumDefinition;
            if (enumDefinition == null)
            {
                throw new ArgumentNullException(nameof(typeDefinition));
            }

            var enumBuilder = this.m_moduleBuilder.DefineEnum(
                this.GetFullQualifiedTypeName(typeName),
                TypeAttributes.Public,
                typeof(int));
            enumBuilder.DataContractAttribute(this.m_targetNamespace);
            foreach (var enumValue in enumDefinition.Fields)
            {
                var newEnum = enumBuilder.DefineLiteral(enumValue.Name, (int)enumValue.Value);
                newEnum.EnumMemberAttribute(enumValue.Name, (int)enumValue.Value);
            }
            return enumBuilder.CreateTypeInfo();
        }

        /// <summary>
        /// Create an enum type from an EnumValue property of a DataType node.
        /// Available before OPC UA V1.04.
        /// </summary>
        public Type AddEnumType(QualifiedName typeName, ExtensionObject[] enumDefinition)
        {
            if (enumDefinition == null)
            {
                throw new ArgumentNullException(nameof(enumDefinition));
            }

            var enumBuilder = this.m_moduleBuilder.DefineEnum(
                this.GetFullQualifiedTypeName(typeName),
                TypeAttributes.Public,
                typeof(int));
            enumBuilder.DataContractAttribute(this.m_targetNamespace);
            foreach (var extensionObject in enumDefinition)
            {
                var enumValue = extensionObject.Body as EnumValueType;
                var name = enumValue.DisplayName.Text;
                var newEnum = enumBuilder.DefineLiteral(name, (int)enumValue.Value);
                newEnum.EnumMemberAttribute(name, (int)enumValue.Value);
            }
            return enumBuilder.CreateTypeInfo();
        }

        /// <summary>
        /// Create an enum type from the EnumString array of a DataType node.
        /// Available before OPC UA V1.04.
        /// </summary>
        public Type AddEnumType(QualifiedName typeName, LocalizedText[] enumDefinition)
        {
            if (enumDefinition == null)
            {
                throw new ArgumentNullException(nameof(enumDefinition));
            }

            var enumBuilder = this.m_moduleBuilder.DefineEnum(
                this.GetFullQualifiedTypeName(typeName),
                TypeAttributes.Public,
                typeof(int));
            enumBuilder.DataContractAttribute(this.m_targetNamespace);
            int value = 0;
            foreach (var enumValue in enumDefinition)
            {
                var name = enumValue.Text;
                var newEnum = enumBuilder.DefineLiteral(name, value);
                newEnum.EnumMemberAttribute(name, value);
                value++;
            }
            return enumBuilder.CreateTypeInfo();
        }

        /// <summary>
        /// Create a complex type from a StructureDefinition.
        /// Available since OPC UA V1.04 in the DataTypeDefinition attribute.
        /// </summary>
        public IComplexTypeFieldBuilder AddStructuredType(
            QualifiedName name,
            StructureDefinition structureDefinition)
        {
            if (structureDefinition == null)
            {
                throw new ArgumentNullException(nameof(structureDefinition));
            }
            Type baseType;
            switch (structureDefinition.StructureType)
            {
                case StructureType.StructureWithOptionalFields: baseType = typeof(OptionalFieldsComplexType); break;
                case StructureType.Union: baseType = typeof(UnionComplexType); break;
                case StructureType.Structure:
                default: baseType = typeof(BaseComplexType); break;
            }
            var structureBuilder = this.m_moduleBuilder.DefineType(
                this.GetFullQualifiedTypeName(name),
                TypeAttributes.Public | TypeAttributes.Class | TypeAttributes.Serializable,
                baseType);
            structureBuilder.DataContractAttribute(this.m_targetNamespace);
            structureBuilder.StructureDefinitonAttribute(structureDefinition);
            return new ComplexTypeFieldBuilder(structureBuilder, structureDefinition.StructureType);
        }
        #endregion

        #region Private Members
        /// <summary>
        /// Create a unique namespace module name for the type.
        /// </summary>
        private string FindModuleName(string moduleName, string targetNamespace, int targetNamespaceIndex)
        {
            if (String.IsNullOrWhiteSpace(moduleName))
            {
                Uri uri = new Uri(targetNamespace, UriKind.RelativeOrAbsolute);
                var tempName = uri.IsAbsoluteUri ? uri.AbsolutePath : uri.ToString();

                tempName = tempName.Replace("/", "");
                var splitName = tempName.Split(':');
                moduleName = splitName.Last();
            }
            return moduleName;
        }

        /// <summary>
        /// Creates a unique full qualified type name for the assembly.
        /// </summary>
        /// <param name="browseName">The browse name of the type.</param>
        private string GetFullQualifiedTypeName(QualifiedName browseName)
        {
            var result = "Opc.Ua.ComplexTypes." + this.m_moduleName + ".";
            if (browseName.NamespaceIndex > 1)
            {
                result += browseName.NamespaceIndex + ".";
            }
            return result + this.ReplaceInvalidCSharpSymbolChars(browseName.Name);
        }

        private string ReplaceInvalidCSharpSymbolChars(string browseName)
        {
            var newChar = "_";
            return browseName
                .Replace(" ", newChar)
                .Replace("-", newChar)
                .Replace(".", newChar)
                .Replace("\"", string.Empty);
        }
        #endregion

        #region Private Fields
        private ModuleBuilder m_moduleBuilder;
        private string m_targetNamespace;
        private string m_moduleName;
        private int m_targetNamespaceIndex;
        #endregion
    }

}