// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

using System;
using OMP.Connector.Domain.Schema.Extenions;
using OMP.Connector.Domain.Schema.Interfaces;

namespace OMP.Connector.Domain.Schema.Factories
{
    public class ModelFactory
    {
        public static TType CreateInstance<TType>(string schemaContainerPath)
            where TType : IModel
        {
            var instance = Activator.CreateInstance<TType>();
            var type = typeof(TType);

            instance.Namespace = type.GetModelNameSpace();
            instance.Schema = ModelExtensions.GetSchemaId(schemaContainerPath, type);

            return instance;
        }
    }
}