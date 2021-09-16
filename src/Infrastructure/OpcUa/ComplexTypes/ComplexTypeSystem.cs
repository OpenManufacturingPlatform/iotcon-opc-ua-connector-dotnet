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
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using OMP.Connector.Domain;
using OMP.Connector.Infrastructure.OpcUa.ComplexTypes.TypeBuilder;
using Opc.Ua;
using Opc.Ua.Client;
using static Opc.Ua.Utils;

namespace OMP.Connector.Infrastructure.OpcUa.ComplexTypes
{
    /// <summary>
    /// Manages the custom types of a server for a client session.
    /// Loads the custom types into the type factory
    /// of a client session, to allow for decoding and encoding
    /// of custom enumeration types and structured types.
    /// </summary>
    /// <remarks>
    /// Support for V1.03 dictionaries and all V1.04 data type definitions
    /// with the following known restrictions:
    /// - Support only for V1.03 structured types which can be mapped to the V1.04
    /// structured type definition. Unsupported V1.03 types are ignored.
    /// - V1.04 OptionSet does not create the enumeration flags. 
    /// </remarks>
    public class ComplexTypeSystem : IComplexTypeSystem
    {

        #region Constructors
        /// <summary>
        /// Initializes the type system with a session to load the custom types.
        /// </summary>
        public ComplexTypeSystem(Session session)
        {
            this.Initialize(session, new ComplexTypeBuilderFactory());
        }

        /// <summary>
        /// Initializes the type system with a session to load the custom types
        /// and a customized type builder factory
        /// </summary>
        public ComplexTypeSystem(
            Session session,
            IComplexTypeFactory complexTypeBuilderFactory)
        {
            this.Initialize(session, complexTypeBuilderFactory);
        }

        private void Initialize(
            Session session,
            IComplexTypeFactory complexTypeBuilderFactory)
        {
            this.m_session = session;
            this.m_complexTypeBuilderFactory = complexTypeBuilderFactory;
        }
        #endregion

        #region Public Members
        /// <summary>
        /// Load a single custom type with subtypes.
        /// </summary>
        /// <remarks>
        /// Uses inverse references on the server to find the super type(s).
        /// If the new structure contains a type dependency to a yet
        /// unknown type, it loads also the dependent type(s).
        /// For servers without DataTypeDefinition support all
        /// custom types are loaded.
        /// </remarks>
        public async Task<Type> LoadType(ExpandedNodeId nodeId, bool subTypes = false, bool throwOnError = false)
        {
            try
            {
                var subTypeNodes = this.LoadDataTypes(nodeId, subTypes, true);
                var subTypeNodesWithoutKnownTypes = this.RemoveKnownTypes(subTypeNodes);

                if (subTypeNodesWithoutKnownTypes.Count > 0)
                {
                    IList<INode> serverEnumTypes = new List<INode>();
                    IList<INode> serverStructTypes = new List<INode>();
                    foreach (var node in subTypeNodesWithoutKnownTypes)
                    {
                        this.AddEnumerationOrStructureType(node, serverEnumTypes, serverStructTypes);
                    }

                    // load server types
                    if (this.DisableDataTypeDefinition || !this.LoadBaseDataTypes(serverEnumTypes, serverStructTypes))
                    {
                        if (!this.DisableDataTypeDictionary)
                        {
                            await this.LoadDictionaryDataTypes(serverEnumTypes, false).ConfigureAwait(false);
                        }
                    }
                }
                return this.GetSystemType(nodeId);
            }
            catch (ServiceResultException sre)
            {
                Utils.Trace(sre, $"Failed to load the custom type {nodeId}.");
                if (throwOnError)
                {
                    throw;
                }
                return null;
            }
        }

        /// <summary>
        /// Load all custom types of a namespace.
        /// </summary>
        /// <remarks>
        /// If a new type in the namespace contains a type dependency to an
        /// unknown type in another namespace, it loads also the dependent type(s).
        /// For servers without DataTypeDefinition support all
        /// custom types are loaded.
        /// </remarks>
        public async Task<bool> LoadNamespace(string nameSpace, bool throwOnError = false)
        {
            try
            {
                int index = this.m_session.NamespaceUris.GetIndex(nameSpace);
                if (index < 0)
                {
                    throw new ServiceResultException($"Bad argument {nameSpace}. Namespace not found.");
                }
                ushort nameSpaceIndex = (ushort)index;
                var serverEnumTypes = this.LoadDataTypes(DataTypeIds.Enumeration);
                var serverStructTypes = this.LoadDataTypes(DataTypeIds.Structure, true);
                // filter for namespace
                serverEnumTypes = serverEnumTypes.Where(rd => rd.NodeId.NamespaceIndex == nameSpaceIndex).ToList();
                serverStructTypes = serverStructTypes.Where(rd => rd.NodeId.NamespaceIndex == nameSpaceIndex).ToList();
                // load types
                if (this.DisableDataTypeDefinition || !this.LoadBaseDataTypes(serverEnumTypes, serverStructTypes))
                {
                    if (this.DisableDataTypeDictionary)
                    {
                        return false;
                    }
                    return await this.LoadDictionaryDataTypes(serverEnumTypes, false).ConfigureAwait(false);
                }
                return true;
            }
            catch (ServiceResultException sre)
            {
                Utils.Trace(sre, $"Failed to load the custom type dictionary.");
                if (throwOnError)
                {
                    throw;
                }
                return false;
            }
        }

        /// <summary>
        /// Load all custom types from a server into the session system type factory.
        /// </summary>
        /// <remarks>
        /// The loader follows the following strategy:
        /// - Load all DataType nodes of the Enumeration subtypes.
        /// - Load all DataType nodes of the Structure subtypes.
        /// - Create all enumerated custom types using the DataTypeDefinion attribute, if available.
        /// - Create all remaining enumerated custom types using the EnumValues or EnumStrings property, if available.
        /// - Create all structured types using the DataTypeDefinion attribute, if available.
        /// if there are type definitions remaining
        /// - Load the binary schema dictionaries with type definitions.
        /// - Create all remaining enumerated custom types using the dictionaries.
        /// - Convert all structured types in the dictionaries to the DataTypeDefinion attribute, if possible.
        /// - Create all structured types from the dictionaries using the converted DataTypeDefinion attribute..
        /// </remarks>
        public async Task<bool> Load(bool onlyEnumTypes = false, bool throwOnError = false)
        {
            try
            {
                // load server types
                IList<INode> serverEnumTypes = this.LoadDataTypes(DataTypeIds.Enumeration);
                IList<INode> serverStructTypes = onlyEnumTypes ? new List<INode>() : this.LoadDataTypes(DataTypeIds.Structure, true);
                if (this.DisableDataTypeDefinition || !this.LoadBaseDataTypes(serverEnumTypes, serverStructTypes))
                {
                    if (this.DisableDataTypeDictionary)
                    {
                        return false;
                    }
                    return await this.LoadDictionaryDataTypes(serverEnumTypes, true).ConfigureAwait(false);
                }
                return true;
            }
            catch (ServiceResultException sre)
            {
                Utils.Trace(sre, $"Failed to load the custom type dictionary.");
                if (throwOnError)
                {
                    throw;
                }
                return false;
            }
        }

        /// <summary>
        /// Get the types defined in this type system.
        /// </summary>
        public Type[] GetDefinedTypes()
        {
            return this.m_complexTypeBuilderFactory.GetTypes();
        }
        #endregion

        #region Internal Properties
        /// <summary>
        /// Disable the use of DataTypeDefinition to create the complex type definition.
        /// </summary>
        internal bool DisableDataTypeDefinition { get; set; } = false;

        /// <summary>
        /// Disable the use of DataType Dictinaries to create the complex type definition.
        /// </summary>
        internal bool DisableDataTypeDictionary { get; set; } = false;
        #endregion

        #region Private Members
        /// <summary>
        /// Load listed custom types from dictionaries
        /// into the sessions system type factory.
        /// </summary>
        /// <remarks>
        /// Loads all custom types at this time to avoid
        /// complexity when resolving type dependencies.
        /// </remarks>
        private async Task<bool> LoadDictionaryDataTypes(
            IList<INode> serverEnumTypes,
            bool fullTypeList
            )
        {
            // build a type dictionary with all known new types
            var allEnumTypes = fullTypeList ? serverEnumTypes : this.LoadDataTypes(DataTypeIds.Enumeration);
            var typeDictionary = new Dictionary<XmlQualifiedName, NodeId>();

            // strip known types from list
            serverEnumTypes = this.RemoveKnownTypes(allEnumTypes);

            // load the binary schema dictionaries from the server
            var typeSystem = await this.m_session.LoadDataTypeSystem().ConfigureAwait(false);

            // sort dictionaries with import dependencies to the end of the list
            var sortedTypeSystem = typeSystem.OrderBy(t => t.Value.TypeDictionary?.Import?.Count()).ToList();

            bool allTypesLoaded = true;

            // create custom types for all dictionaries
            foreach (var dictionaryId in sortedTypeSystem)
            {
                try
                {
                    var dictionary = dictionaryId.Value;
                    if (dictionary.TypeDictionary == null ||
                        dictionary.TypeDictionary.Items == null)
                    {
                        continue;
                    }
                    var targetDictionaryNamespace = dictionary.TypeDictionary.TargetNamespace;
                    var targetNamespaceIndex = this.m_session.NamespaceUris.GetIndex(targetDictionaryNamespace);
                    var structureList = new List<Opc.Ua.Schema.Binary.TypeDescription>();
                    var enumList = new List<Opc.Ua.Schema.Binary.TypeDescription>();

                    // split into enumeration and structure types and sort
                    // types with dependencies to the end of the list.
                    this.SplitAndSortDictionary(dictionary, structureList, enumList);

                    // create assembly for all types in the same module
                    var complexTypeBuilder = this.m_complexTypeBuilderFactory.Create(
                        targetDictionaryNamespace,
                        targetNamespaceIndex,
                        dictionary.Name);

                    // Add all unknown enumeration types in dictionary
                    this.AddEnumTypes(complexTypeBuilder, typeDictionary, enumList, allEnumTypes, serverEnumTypes);

                    // handle structures
                    int lastStructureCount = 0;
                    while (structureList.Count > 0 &&
                        structureList.Count != lastStructureCount)
                    {
                        lastStructureCount = structureList.Count;
                        var retryStructureList = new List<Opc.Ua.Schema.Binary.TypeDescription>();
                        // build structured types
                        foreach (var item in structureList)
                        {
                            if (item is Opc.Ua.Schema.Binary.StructuredType structuredObject)
                            {   // note: the BrowseName contains the actual Value string of the DataType node.
                                var nodeId = dictionary.DataTypes.FirstOrDefault(d => d.Value.BrowseName.Name == item.Name).Value;
                                if (nodeId == null)
                                {
                                    Utils.Trace(TraceMasks.Error, $"Skip the type definition of {item.Name} because the data type node was not found.");
                                    continue;
                                }

                                // find the data type node and the binary encoding id
                                ExpandedNodeId typeId;
                                ExpandedNodeId binaryEncodingId;
                                DataTypeNode dataTypeNode;
                                bool newTypeDescription = this.BrowseTypeIdsForDictionaryComponent(
                                    ExpandedNodeId.ToNodeId(nodeId.NodeId, this.m_session.NamespaceUris),
                                    out typeId,
                                    out binaryEncodingId,
                                    out dataTypeNode);

                                if (!newTypeDescription)
                                {
                                    Utils.Trace(TraceMasks.Error, $"Skip the type definition of {item.Name} because the data type node was not found.");
                                    continue;
                                }

                                if (this.GetSystemType(typeId) != null)
                                {
                                    var qName = structuredObject.QName ?? new XmlQualifiedName(structuredObject.Name, targetDictionaryNamespace);
                                    typeDictionary[qName] = ExpandedNodeId.ToNodeId(typeId, this.m_session.NamespaceUris);
                                    Utils.Trace(TraceMasks.Information, $"Skip the type definition of {item.Name} because the type already exists.");
                                    continue;
                                }

                                // Use DataTypeDefinition attribute, if available (>=V1.04)
                                StructureDefinition structureDefinition = null;
                                if (!this.DisableDataTypeDefinition)
                                {
                                    structureDefinition = this.GetStructureDefinition(dataTypeNode);
                                }
                                if (structureDefinition == null)
                                {
                                    try
                                    {
                                        // convert the binary schema description to a StructureDefinition
                                        structureDefinition = structuredObject.ToStructureDefinition(
                                            binaryEncodingId,
                                            typeDictionary,
                                            this.m_session.NamespaceUris);
                                    }
                                    catch (DataTypeNotFoundException typeNotFoundException)
                                    {
                                        Utils.Trace(typeNotFoundException,
                                            $"Skipped the type definition of {item.Name}. Retry in next round.");
                                        retryStructureList.Add(item);
                                        continue;
                                    }
                                    catch (DataTypeNotSupportedException typeNotSupportedException)
                                    {
                                        Utils.Trace(typeNotSupportedException,
                                            $"Skipped the type definition of {item.Name} because it is not supported.");
                                        continue;
                                    }
                                    catch (ServiceResultException sre)
                                    {
                                        Utils.Trace(sre, $"Skip the type definition of {item.Name}.");
                                        continue;
                                    }
                                }

                                Type complexType = null;
                                if (structureDefinition != null)
                                {
                                    var encodingIds = this.BrowseForEncodings(typeId, this.m_supportedEncodings,
                                        out binaryEncodingId, out ExpandedNodeId xmlEncodingId);
                                    try
                                    {
                                        // build the actual .Net structured type in assembly
                                        complexType = this.AddStructuredType(
                                            complexTypeBuilder,
                                            structureDefinition,
                                            dataTypeNode.BrowseName,
                                            typeId,
                                            binaryEncodingId,
                                            xmlEncodingId
                                            );
                                    }
                                    catch (DataTypeNotFoundException typeNotFoundException)
                                    {
                                        Utils.Trace(typeNotFoundException,
                                            $"Skipped the type definition of {item.Name}. Retry in next round.");
                                        retryStructureList.Add(item);
                                        continue;
                                    }
                                    catch (DataTypeNotSupportedException typeNotSupportedException)
                                    {
                                        Utils.Trace(typeNotSupportedException,
                                            $"Skipped the type definition of {item.Name} because it is not supported.");
                                        continue;
                                    }

                                    // Add new type to factory
                                    if (complexType != null)
                                    {
                                        // match namespace and add new type to type factory
                                        foreach (var encodingId in encodingIds)
                                        {
                                            this.AddEncodeableType(encodingId, complexType);
                                        }
                                        this.AddEncodeableType(typeId, complexType);
                                        var qName = structuredObject.QName ?? new XmlQualifiedName(structuredObject.Name, targetDictionaryNamespace);
                                        typeDictionary[qName] = ExpandedNodeId.ToNodeId(typeId, this.m_session.NamespaceUris);
                                    }
                                }

                                if (complexType == null)
                                {
                                    retryStructureList.Add(item);
                                    Utils.Trace(TraceMasks.Error, $"Skipped the type definition of {item.Name}. Retry in next round.");
                                }
                            }
                        }
                        structureList = retryStructureList;
                    }
                    allTypesLoaded = allTypesLoaded && structureList.Count == 0;
                }
                catch (ServiceResultException sre)
                {
                    Utils.Trace(sre,
                        $"Unexpected error processing {dictionaryId.Value.Name}.");
                }
            }
            return allTypesLoaded;
        }

        /// <summary>
        /// Load all custom types with DataTypeDefinition into the type factory.
        /// </summary>
        /// <returns>true if all types were loaded, false otherwise</returns>
        private bool LoadBaseDataTypes(
            IList<INode> serverEnumTypes,
            IList<INode> serverStructTypes
            )
        {
            bool repeatDataTypeLoad = false; ;
            IList<INode> enumTypesToDoList = new List<INode>();
            IList<INode> structTypesToDoList = new List<INode>();

            do
            {
                // strip known types
                serverEnumTypes = this.RemoveKnownTypes(serverEnumTypes);
                serverStructTypes = this.RemoveKnownTypes(serverStructTypes);

                repeatDataTypeLoad = false;
                try
                {
                    enumTypesToDoList = this.LoadBaseEnumDataTypes(serverEnumTypes);
                    structTypesToDoList = this.LoadBaseStructureDataTypes(serverStructTypes);
                }
                catch (DataTypeNotFoundException dtnfex)
                {
                    // add missing type to list
                    var dataTypeNode = this.m_session.NodeCache.Find(dtnfex.nodeId);
                    if (dataTypeNode != null)
                    {
                        this.AddEnumerationOrStructureType(dataTypeNode, serverEnumTypes, serverStructTypes);
                        repeatDataTypeLoad = true;
                    }
                }
            } while (repeatDataTypeLoad);

            // all types loaded
            return enumTypesToDoList.Count == 0 && structTypesToDoList.Count == 0;
        }

        /// <summary>
        /// Load all custom types with DataTypeDefinition into the type factory.
        /// </summary>
        /// <returns>true if all types were loaded, false otherwise</returns>
        private IList<INode> LoadBaseEnumDataTypes(
            IList<INode> serverEnumTypes
            )
        {
            // strip known types
            serverEnumTypes = this.RemoveKnownTypes(serverEnumTypes);

            // add new enum Types for all namespaces
            var enumTypesToDoList = new List<INode>();
            int namespaceCount = this.m_session.NamespaceUris.Count;

            // create enumeration types for all namespaces
            for (uint i = 0; i < namespaceCount; i++)
            {
                IComplexTypeBuilder complexTypeBuilder = null;
                var enumTypes = serverEnumTypes.Where(node => node.NodeId.NamespaceIndex == i).ToList();
                if (enumTypes.Count != 0)
                {
                    if (complexTypeBuilder == null)
                    {
                        string targetNamespace = this.m_session.NamespaceUris.GetString(i);
                        complexTypeBuilder = this.m_complexTypeBuilderFactory.Create(
                            targetNamespace,
                            (int)i);
                    }
                    foreach (var enumType in enumTypes)
                    {
                        var newType = this.AddEnumType(complexTypeBuilder, enumType as DataTypeNode);
                        if (newType != null)
                        {
                            // match namespace and add to type factory
                            this.AddEncodeableType(enumType.NodeId, newType);
                        }
                        else
                        {
                            enumTypesToDoList.Add(enumType);
                        }
                    }
                }
            }

            // all types loaded, return remaining
            return enumTypesToDoList;
        }

        /// <summary>
        /// Load all structure custom types with DataTypeDefinition into the type factory.
        /// </summary>
        /// <returns>true if all types were loaded, false otherwise</returns>
        private IList<INode> LoadBaseStructureDataTypes(
            IList<INode> serverStructTypes
            )
        {
            // strip known types
            serverStructTypes = this.RemoveKnownTypes(serverStructTypes);

            // add new enum Types for all namespaces
            int namespaceCount = this.m_session.NamespaceUris.Count;

            bool retryAddStructType;
            var structTypesToDoList = new List<INode>();
            var structTypesWorkList = serverStructTypes;

            // create structured types for all namespaces
            do
            {
                retryAddStructType = false;
                for (uint i = 0; i < namespaceCount; i++)
                {
                    IComplexTypeBuilder complexTypeBuilder = null;
                    var structTypes = structTypesWorkList.Where(node => node.NodeId.NamespaceIndex == i).ToList();
                    if (structTypes.Count != 0)
                    {
                        if (complexTypeBuilder == null)
                        {
                            string targetNamespace = this.m_session.NamespaceUris.GetString(i);
                            complexTypeBuilder = this.m_complexTypeBuilderFactory.Create(
                                targetNamespace,
                                (int)i);
                        }
                        foreach (INode structType in structTypes)
                        {
                            Type newType = null;
                            if (!(structType is DataTypeNode dataTypeNode))
                            {
                                continue;
                            }
                            var structureDefinition = this.GetStructureDefinition(dataTypeNode);
                            if (structureDefinition != null)
                            {
                                var encodingIds = this.BrowseForEncodings(structType.NodeId, this.m_supportedEncodings,
                                    out ExpandedNodeId binaryEncodingId, out ExpandedNodeId xmlEncodingId);
                                try
                                {
                                    newType = this.AddStructuredType(
                                        complexTypeBuilder,
                                        structureDefinition,
                                        dataTypeNode.BrowseName,
                                        structType.NodeId,
                                        binaryEncodingId,
                                        xmlEncodingId
                                        );
                                }
                                catch (DataTypeNotFoundException dtnfex)
                                {
                                    var typeMatch = structTypesWorkList.Where(n => n.NodeId == dtnfex.nodeId).FirstOrDefault();
                                    if (typeMatch == null)
                                    {
                                        throw;
                                    }
                                    else
                                    {   // known missing type, retry on next round
                                        Utils.Trace(dtnfex,
                                            $"Skipped the type definition of {dataTypeNode.BrowseName.Name}. Retry in next round.");
                                        retryAddStructType = true;
                                    }
                                }
                                catch (DataTypeNotSupportedException dtnsex)
                                {
                                    Utils.Trace(dtnsex,
                                        $"Skipped the type definition of {dataTypeNode.BrowseName.Name} because it is not supported.");
                                    continue;
                                }
                                catch
                                {
                                    // creating the new type failed, likely a missing dependency, retry later
                                    retryAddStructType = true;
                                }
                                if (newType != null)
                                {
                                    foreach (var encodingId in encodingIds)
                                    {
                                        this.AddEncodeableType(encodingId, newType);
                                    }
                                    this.AddEncodeableType(structType.NodeId, newType);
                                }
                            }
                            if (newType == null)
                            {
                                structTypesToDoList.Add(structType);
                            }
                        }
                    }
                }
                // due to type dependencies, retry missing types until there is no more progress
                if (retryAddStructType &&
                    structTypesWorkList.Count != structTypesToDoList.Count)
                {
                    structTypesWorkList = structTypesToDoList;
                    structTypesToDoList = new List<INode>();
                }
            } while (retryAddStructType);

            // all types loaded
            return structTypesToDoList;
        }

        /// <summary>
        /// Return the structure definition from a DataTypeDefinition
        /// </summary>
        private StructureDefinition GetStructureDefinition(DataTypeNode dataTypeNode)
        {
            if (dataTypeNode.DataTypeDefinition?.Body is StructureDefinition structureDefinition)
            {
                // Validate the DataTypeDefinition structure,
                // but not if the type is supported
                if (structureDefinition.Fields == null ||
                    structureDefinition.BaseDataType.IsNullNodeId ||
                    structureDefinition.BinaryEncodingId.IsNull)
                {
                    return null;
                }
                // Validate the structure according to Part3, Table 36
                foreach (var field in structureDefinition.Fields)
                {
                    // validate if the DataTypeDefinition is correctly
                    // filled out, some servers don't do it yet...
                    if (field.BinaryEncodingId.IsNull ||
                        field.DataType.IsNullNodeId ||
                        field.TypeId.IsNull ||
                        field.Name == null)
                    {
                        return null;
                    }
                    if (!(field.ValueRank == -1 ||
                        field.ValueRank >= 1))
                    {
                        return null;
                    }
                    if (structureDefinition.StructureType == StructureType.Structure &&
                        field.IsOptional)
                    {
                        return null;
                    }
                }
                return structureDefinition;
            }
            return null;

        }

        /// <summary>
        /// Helper to ensure the expanded nodeId contains a valid namespaceUri.
        /// </summary>
        /// <param name="expandedNodeId">The expanded nodeId.</param>
        /// <param name="namespaceTable">The session namespace table.</param>
        /// <returns>The normalized expanded nodeId.</returns>
        private ExpandedNodeId NormalizeExpandedNodeId(ExpandedNodeId expandedNodeId)
        {
            var nodeId = ExpandedNodeId.ToNodeId(expandedNodeId, this.m_session.NamespaceUris);
            return NodeId.ToExpandedNodeId(nodeId, this.m_session.NamespaceUris);
        }

        /// <summary>
        /// Browse for the type and encoding id for a dictionary component.
        /// </summary>
        /// <remarks>
        /// According to Part 5 Annex D, servers shall provide the bi-directional
        /// references between data types, data type encodings, data type description
        /// and data type dictionary.
        /// To find the typeId and encodingId for a dictionary type definition:
        /// i) inverse browse the description to get the encodingid
        /// ii) from the description inverse browse for encoding 
        /// to get the subtype typeid 
        /// iii) load the DataType node 
        /// </remarks>
        /// <param name="nodeId"></param>
        /// <param name="typeId"></param>
        /// <param name="encodingId"></param>
        /// <param name="dataTypeNode"></param>
        /// <returns>true if successful, false otherwise</returns>
        private bool BrowseTypeIdsForDictionaryComponent(
            NodeId nodeId,
            out ExpandedNodeId typeId,
            out ExpandedNodeId encodingId,
            out DataTypeNode dataTypeNode)
        {
            typeId = ExpandedNodeId.Null;
            encodingId = ExpandedNodeId.Null;
            dataTypeNode = null;

            var references = this.m_session.NodeCache.FindReferences(
                nodeId,
                ReferenceTypeIds.HasDescription,
                true,
                false
                );

            if (references.Count == 1)
            {
                encodingId = references[0].NodeId;
                references = this.m_session.NodeCache.FindReferences(
                    encodingId,
                    ReferenceTypeIds.HasEncoding,
                    true,
                    false
                    );
                encodingId = this.NormalizeExpandedNodeId(encodingId);

                if (references.Count == 1)
                {
                    typeId = references[0].NodeId;
                    dataTypeNode = this.m_session.NodeCache.Find(typeId) as DataTypeNode;
                    typeId = this.NormalizeExpandedNodeId(typeId);
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Browse for the property.
        /// </summary>
        /// <remarks>
        /// Browse for property (type description) of an enum datatype.
        /// </remarks>
        /// <param name="nodeId"></param>
        /// <returns></returns>
        private INode BrowseForSingleProperty(
            ExpandedNodeId nodeId)
        {
            var references = this.m_session.NodeCache.FindReferences(
                nodeId,
                ReferenceTypeIds.HasProperty,
                false,
                false
                );
            return references.FirstOrDefault();
        }

        /// <summary>
        /// Browse for the encodings of a type.
        /// </summary>
        /// <remarks>
        /// Browse for binary encoding of a structure datatype.
        /// </remarks>
        private IList<NodeId> BrowseForEncodings(
            ExpandedNodeId nodeId,
            string[] supportedEncodings,
            out ExpandedNodeId binaryEncodingId,
            out ExpandedNodeId xmlEncodingId)
        {
            var references = this.m_session.NodeCache.FindReferences(
                nodeId,
                ReferenceTypeIds.HasEncoding,
                false,
                false
                );
            binaryEncodingId = references.Where(r => r.BrowseName.Name == BrowseNames.DefaultBinary).FirstOrDefault()?.NodeId;
            binaryEncodingId = this.NormalizeExpandedNodeId(binaryEncodingId);
            xmlEncodingId = references.Where(r => r.BrowseName.Name == BrowseNames.DefaultXml).FirstOrDefault()?.NodeId;
            xmlEncodingId = this.NormalizeExpandedNodeId(xmlEncodingId);
            return references.Where(r => supportedEncodings.Contains(r.BrowseName.Name))
                .Select(r => ExpandedNodeId.ToNodeId(r.NodeId, this.m_session.NamespaceUris)).ToList();
        }

        /// <summary>
        /// Load all subTypes and optionally nested subtypes of a type definition.
        /// Filter for all subtypes or only subtypes outside the default namespace.
        /// </summary>
        private IList<INode> LoadDataTypes(
            ExpandedNodeId dataType,
            bool nestedSubTypes = false,
            bool addRootNode = false,
            bool filterUATypes = true)
        {
            var result = new List<INode>();
            var nodesToBrowse = new ExpandedNodeIdCollection {
                dataType
            };

            if (addRootNode)
            {
                var rootNode = this.m_session.NodeCache.Find(dataType);
                if (!(rootNode is DataTypeNode))
                {
                    throw new ServiceResultException("Root Node is not a DataType node.");
                }
                result.Add(rootNode);
            }

            while (nodesToBrowse.Count > 0)
            {
                var nextNodesToBrowse = new ExpandedNodeIdCollection();
                foreach (var node in nodesToBrowse)
                {
                    var response = this.m_session.NodeCache.FindReferences(
                        node,
                        ReferenceTypeIds.HasSubtype,
                        false,
                        false);

                    if (nestedSubTypes)
                    {
                        nextNodesToBrowse.AddRange(response.Select(r => r.NodeId).ToList());
                    }
                    if (filterUATypes)
                    {
                        // filter out default namespace
                        result.AddRange(response.Where(rd => rd.NodeId.NamespaceIndex != 0));
                    }
                    else
                    {
                        result.AddRange(response);
                    }
                }
                nodesToBrowse = nextNodesToBrowse;
            }

            return result;
        }

        /// <summary>
        /// Add data type to enumeration or structure base type list depending on supertype.
        /// </summary>
        private void AddEnumerationOrStructureType(INode dataTypeNode, IList<INode> serverEnumTypes, IList<INode> serverStructTypes)
        {
            NodeId superType = ExpandedNodeId.ToNodeId(dataTypeNode.NodeId, this.m_session.NamespaceUris);
            do
            {
                superType = this.m_session.NodeCache.FindSuperType(superType);
                if (superType.IsNullNodeId)
                {
                    throw new ServiceResultException(StatusCodes.BadNodeIdInvalid, $"SuperType for {dataTypeNode.NodeId} not found.");
                }
                if (superType == DataTypeIds.Enumeration)
                {
                    serverEnumTypes.Insert(0, dataTypeNode);
                    break;
                }
                else if (superType == DataTypeIds.Structure)
                {
                    serverStructTypes.Insert(0, dataTypeNode);
                    break;
                }
                else if (TypeInfo.GetBuiltInType(superType) != BuiltInType.Null)
                {
                    break;
                }
            } while (true);
        }

        /// <summary>
        /// Remove all known types in the type factory from a list of DataType nodes.
        /// </summary>
        private IList<INode> RemoveKnownTypes(IList<INode> nodeList)
        {
            return nodeList.Where(
                node => this.GetSystemType(node.NodeId) == null).ToList();
        }

        /// <summary>
        /// Get the factory system type for an expanded node id.
        /// </summary>
        private Type GetSystemType(ExpandedNodeId nodeId)
        {
            if (!nodeId.IsAbsolute)
            {
                nodeId = this.NormalizeExpandedNodeId(nodeId);
            }
            return this.m_session.Factory.GetSystemType(nodeId);
        }

        /// <summary>
        /// Add an enum type defined in a binary schema dictionary.
        /// </summary>
        private void AddEnumTypes(
            IComplexTypeBuilder complexTypeBuilder,
            Dictionary<XmlQualifiedName, NodeId> typeDictionary,
            IList<Opc.Ua.Schema.Binary.TypeDescription> enumList,
            IList<INode> allEnumerationTypes,
            IList<INode> enumerationTypes
            )
        {
            foreach (var item in enumList)
            {
                Type newType = null;
                DataTypeNode enumDescription = null;
                DataTypeNode enumType = enumerationTypes.FirstOrDefault(node =>
                    node.BrowseName.Name == item.Name &&
                    (node.BrowseName.NamespaceIndex == complexTypeBuilder.TargetNamespaceIndex ||
                    complexTypeBuilder.TargetNamespaceIndex == -1))
                    as DataTypeNode;
                if (enumType != null)
                {
                    enumDescription = enumType;
                    // try dictionary enum definition
                    var enumeratedObject = item as Opc.Ua.Schema.Binary.EnumeratedType;
                    if (enumeratedObject != null)
                    {
                        // 1. use Dictionary entry
                        newType = complexTypeBuilder.AddEnumType(enumeratedObject);
                    }
                    if (newType == null)
                    {
                        var dataType = this.m_session.NodeCache.Find(enumType.NodeId) as DataTypeNode;
                        if (dataType != null)
                        {
                            if (dataType.DataTypeDefinition != null)
                            {
                                // 2. use DataTypeDefinition 
                                newType = complexTypeBuilder.AddEnumType(enumType.BrowseName.Name, dataType.DataTypeDefinition);
                            }
                            else
                            {
                                // browse for EnumFields or EnumStrings property
                                var property = this.BrowseForSingleProperty(enumType.NodeId);
                                var enumArray = this.m_session.ReadValue(
                                    ExpandedNodeId.ToNodeId(property.NodeId, this.m_session.NamespaceUris));
                                if (enumArray.Value is ExtensionObject[])
                                {
                                    // 3. use EnumValues
                                    newType = complexTypeBuilder.AddEnumType(enumType.BrowseName.Name, (ExtensionObject[])enumArray.Value);
                                }
                                else if (enumArray.Value is LocalizedText[])
                                {
                                    // 4. use EnumStrings
                                    newType = complexTypeBuilder.AddEnumType(enumType.BrowseName.Name, (LocalizedText[])enumArray.Value);
                                }
                            }
                        }
                    }
                }
                else
                {
                    enumDescription = allEnumerationTypes.Where(node =>
                        node.BrowseName.Name == item.Name &&
                        (node.BrowseName.NamespaceIndex == complexTypeBuilder.TargetNamespaceIndex ||
                        complexTypeBuilder.TargetNamespaceIndex == -1)).FirstOrDefault()
                        as DataTypeNode;
                }
                if (enumDescription != null)
                {
                    var qName = new XmlQualifiedName(item.Name, complexTypeBuilder.TargetNamespace);
                    typeDictionary[qName] = enumDescription.NodeId;
                }
                if (newType != null)
                {
                    // match namespace and add to type factory
                    this.AddEncodeableType(enumType.NodeId, newType);
                }
            }
        }

        /// <summary>
        /// Helper to add new type with absolute ExpandedNodeId.
        /// </summary>
        private void AddEncodeableType(ExpandedNodeId nodeId, Type type)
        {
            if (NodeId.IsNull(nodeId) || type == null)
            {
                return;
            }
            var internalNodeId = this.NormalizeExpandedNodeId(nodeId);
            Utils.TraceDebug($"Adding Type {type.FullName} as: {internalNodeId}");
            this.m_session.Factory.AddEncodeableType(internalNodeId, type);
        }

        /// <summary>
        /// Add an enum type defined in a DataType node.
        /// </summary>
        private Type AddEnumType(
            IComplexTypeBuilder complexTypeBuilder,
            DataTypeNode enumTypeNode
            )
        {
            Type newType = null;
            if (enumTypeNode != null)
            {
                QualifiedName name = enumTypeNode.BrowseName;
                if (enumTypeNode.DataTypeDefinition != null)
                {
                    // 1. use DataTypeDefinition 
                    newType = complexTypeBuilder.AddEnumType(name, enumTypeNode.DataTypeDefinition);
                }
                else
                {
                    // browse for EnumFields or EnumStrings property
                    var property = this.BrowseForSingleProperty(enumTypeNode.NodeId);
                    if (property != null)
                    {
                        var enumArray = this.m_session.ReadValue(
                            ExpandedNodeId.ToNodeId(property.NodeId,
                            this.m_session.NamespaceUris));
                        if (enumArray.Value is ExtensionObject[])
                        {
                            // 2. use EnumValues
                            newType = complexTypeBuilder.AddEnumType(name, (ExtensionObject[])enumArray.Value);
                        }
                        else if (enumArray.Value is LocalizedText[])
                        {
                            // 3. use EnumStrings
                            newType = complexTypeBuilder.AddEnumType(name, (LocalizedText[])enumArray.Value);
                        }
                    }
                }
            }
            return newType;
        }

        /// <summary>
        /// Add structured type to assembly with StructureDefinition.
        /// </summary>
        private Type AddStructuredType(
            IComplexTypeBuilder complexTypeBuilder,
            StructureDefinition structureDefinition,
            QualifiedName typeName,
            ExpandedNodeId complexTypeId,
            ExpandedNodeId binaryEncodingId,
            ExpandedNodeId xmlEncodingId
            )
        {
            // check all types
            var typeList = new List<Type>();
            foreach (StructureField field in structureDefinition.Fields)
            {
                var newType = this.GetFieldType(field);
                var isNestedType = field.DataType.Equals(complexTypeId);
                if (newType == null && !isNestedType)
                {
                    throw new DataTypeNotFoundException(field.DataType);
                }
                typeList.Add(newType);
            }

            var fieldBuilder = complexTypeBuilder.AddStructuredType(
                typeName,
                structureDefinition
                );

            fieldBuilder.AddTypeIdAttribute(complexTypeId, binaryEncodingId, xmlEncodingId);

            int order = 1;
            var typeListEnumerator = typeList.GetEnumerator();
            foreach (StructureField field in structureDefinition.Fields)
            {
                typeListEnumerator.MoveNext();
                if (typeListEnumerator.Current != null)
                {
                    fieldBuilder.AddField(field, typeListEnumerator.Current, order);
                }
                else
                {
                    if (field.ValueRank < 0)
                    {
                        fieldBuilder.AddField(field, (fieldBuilder as ComplexTypeFieldBuilder).TypeBuilder, order);
                    }
                    else
                    {
                        var arrayType = (fieldBuilder as ComplexTypeFieldBuilder).TypeBuilder.MakeArrayType();
                        fieldBuilder.AddField(field, arrayType, order);
                    }
                }
                order++;
            }

            return fieldBuilder.CreateType();
        }

        /// <summary>
        /// Determine the type of a field in a StructureField definition.
        /// </summary>
        private Type GetFieldType(StructureField field)
        {
            Type collectionType = null;

            if (field.ValueRank != ValueRanks.Scalar &&
                field.ValueRank != ValueRanks.OneDimension)
            {
                throw new DataTypeNotSupportedException(field.DataType, $"The ValueRank {field.ValueRank} is not supported.");
            }

            Type fieldType = field.DataType.NamespaceIndex == 0 ?
                TypeInfo.GetSystemType(field.DataType, this.m_session.Factory) :
                this.GetSystemType(field.DataType);
            if (fieldType == null)
            {
                var superType = this.GetBuiltInSuperType(field.DataType);
                if (superType != null &&
                    !superType.IsNullNodeId)
                {
                    field.DataType = superType;
                    return this.GetFieldType(field);
                }
                return null;
            }

            if (field.DataType.NamespaceIndex == 0)
            {
                if (field.ValueRank == ValueRanks.OneDimension)
                {
                    if (fieldType == typeof(Byte[]))
                    {
                        collectionType = typeof(ByteStringCollection);
                    }
                    else if (fieldType == typeof(Single))
                    {
                        collectionType = typeof(FloatCollection);
                    }
                    else
                    {
                        var assemblyQualifiedName = typeof(StatusCode).Assembly;
                        String collectionClassName = "Opc.Ua." + fieldType.Name + "Collection, " + assemblyQualifiedName;
                        collectionType = Type.GetType(collectionClassName);
                    }
                }
            }
            else
            {
                if (field.ValueRank == ValueRanks.OneDimension)
                {
                    String collectionClassName = (fieldType.Namespace != null) ? fieldType.Namespace + "." : "";
                    collectionClassName += fieldType.Name + "Collection, " + fieldType.Assembly;
                    collectionType = Type.GetType(collectionClassName);
                }
            }

            if (field.ValueRank == ValueRanks.OneDimension)
            {
                fieldType = collectionType ?? fieldType.MakeArrayType();
            }

            return fieldType;
        }

        /// <summary>
        /// Find superType for a datatype.
        /// </summary>
        private NodeId GetBuiltInSuperType(NodeId dataType)
        {
            var superType = dataType;
            do
            {
                superType = this.m_session.NodeCache.FindSuperType(superType);
                if (superType == null ||
                    superType.IsNullNodeId)
                {
                    return null;
                }
                if (superType.NamespaceIndex == 0)
                {
                    if (superType == DataTypeIds.Enumeration ||
                        superType == DataTypeIds.Structure)
                    {
                        return null;
                    }
                    break;
                }
            } while (true);
            return superType;
        }

        /// <summary>
        /// Split the dictionary types into a list of structures and enumerations.
        /// Sort the structures by dependencies, with structures with dependent
        /// types at the end of the list, so they can be added to the factory in order.
        /// </summary>
        private void SplitAndSortDictionary(
                DataDictionary dictionary,
                List<Opc.Ua.Schema.Binary.TypeDescription> structureList,
                List<Opc.Ua.Schema.Binary.TypeDescription> enumList
                )
        {
            foreach (var item in dictionary.TypeDictionary.Items)
            {
                if (item is Opc.Ua.Schema.Binary.StructuredType structuredObject)
                {
                    var dependentFields = structuredObject.Field.Where(f => f.TypeName.Namespace == dictionary.TypeDictionary.TargetNamespace);
                    if (!dependentFields.Any())
                    {
                        structureList.Insert(0, structuredObject);
                    }
                    else
                    {
                        structureList.Add(structuredObject);
                    }
                }
                else if (item is Opc.Ua.Schema.Binary.EnumeratedType)
                {
                    enumList.Add(item);
                }
                else if (item is Opc.Ua.Schema.Binary.OpaqueType)
                {
                    // TODO: Opaque types not supported yet
                }
                else
                {
                    throw new ServiceResultException(StatusCodes.BadUnexpectedError, $"Unexpected Type in binary schema: {item.GetType().Name}.");
                }
            }
        }
        #endregion

        #region Private Fields
        private Session m_session;
        private IComplexTypeFactory m_complexTypeBuilderFactory;
        private string[] m_supportedEncodings = new string[] { BrowseNames.DefaultBinary, BrowseNames.DefaultXml, BrowseNames.DefaultJson };
        private const string kOpcComplexTypesPrefix = "Opc.Ua.ComplexTypes.";
        #endregion
    }
}//namespace
