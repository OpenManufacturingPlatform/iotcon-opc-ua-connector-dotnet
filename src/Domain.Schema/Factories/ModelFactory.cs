using System;
using Omp.Connector.Domain.Schema.Extentions;
using Omp.Connector.Domain.Schema.Interfaces;

namespace Omp.Connector.Domain.Schema.Factories
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