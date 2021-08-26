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
using System.Reflection;
using System.Reflection.Emit;
using OMP.Connector.Infrastructure.OpcUa.ComplexTypes.Types;
using Opc.Ua;

namespace OMP.Connector.Infrastructure.OpcUa.ComplexTypes.TypeBuilder
{
    /// <summary>
    /// Builder for property fields.
    /// </summary>
    public class ComplexTypeFieldBuilder : IComplexTypeFieldBuilder
    {
        #region Constructors
        public ComplexTypeFieldBuilder(
            System.Reflection.Emit.TypeBuilder structureBuilder,
            StructureType structureType)
        {
            this.m_structureBuilder = structureBuilder;
            this.m_structureType = structureType;
        }
        #endregion

        #region Public Properties
        /// <summary>
        /// Build the StructureTypeId attribute for a complex type.
        /// </summary>
        public void AddTypeIdAttribute(
            ExpandedNodeId complexTypeId,
            ExpandedNodeId binaryEncodingId,
            ExpandedNodeId xmlEncodingId
            )
        {
            this.m_structureBuilder.StructureTypeIdAttribute(
                complexTypeId,
                binaryEncodingId,
                xmlEncodingId
                );
        }

        /// <summary>
        /// Create a property field of a class with get and set.
        /// </summary>
        public void AddField(StructureField field, Type fieldType, int order)
        {
        	if(fieldType == null) { fieldType = field.ValueRank >= 0 ? this.m_structureBuilder.MakeArrayType() : this.m_structureBuilder; }
            var fieldBuilder = this.m_structureBuilder.DefineField("_" + field.Name, fieldType, FieldAttributes.Private);
            var propertyBuilder = this.m_structureBuilder.DefineProperty(
                field.Name,
                PropertyAttributes.None,
                fieldType,
                null);
            var methodAttributes =
                System.Reflection.MethodAttributes.Public |
                System.Reflection.MethodAttributes.HideBySig |
                System.Reflection.MethodAttributes.Virtual;

            var setBuilder = this.m_structureBuilder.DefineMethod("set_" + field.Name, methodAttributes, null, new[] { fieldType });
            var setIl = setBuilder.GetILGenerator();
            setIl.Emit(OpCodes.Ldarg_0);
            setIl.Emit(OpCodes.Ldarg_1);
            setIl.Emit(OpCodes.Stfld, fieldBuilder);
            if (this.m_structureType == StructureType.Union)
            {
                // set the union selector to the new field index
                FieldInfo unionField = typeof(UnionComplexType).GetField(
                    "m_switchField",
                    BindingFlags.NonPublic |
                    BindingFlags.Instance);
                setIl.Emit(OpCodes.Ldarg_0);
                setIl.Emit(OpCodes.Ldc_I4, order);
                setIl.Emit(OpCodes.Stfld, unionField);
            }
            setIl.Emit(OpCodes.Ret);

            var getBuilder = this.m_structureBuilder.DefineMethod("get_" + field.Name, methodAttributes, fieldType, Type.EmptyTypes);
            var getIl = getBuilder.GetILGenerator();
            getIl.Emit(OpCodes.Ldarg_0);
            getIl.Emit(OpCodes.Ldfld, fieldBuilder);
            getIl.Emit(OpCodes.Ret);

            propertyBuilder.SetGetMethod(getBuilder);
            propertyBuilder.SetSetMethod(setBuilder);
            propertyBuilder.DataMemberAttribute(field.Name, false, order);
            propertyBuilder.StructureFieldAttribute(field);
        }

        /// <summary>
        /// Finish the type creation and returns the new type.
        /// </summary>
        public Type CreateType()
        {
            var complexType = this.m_structureBuilder.CreateTypeInfo().AsType();
            this.m_structureBuilder = null;
            return complexType;
        }
        #endregion

        internal System.Reflection.Emit.TypeBuilder TypeBuilder { get => this.m_structureBuilder; }
        #region Private Member
        private System.Reflection.Emit.TypeBuilder m_structureBuilder;
        private StructureType m_structureType;
        #endregion
    }
}//namespace
