// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace OMP.Connector.Domain.Schema.Helpers
{
    public static class ReflectiveEnumerator
    {
        private static readonly IEnumerable<string> AssemblyNames = new List<string>
        {
            "IoT.Models",
            "IoT.SchemaGenerator"
        };

        public static IEnumerable<Type> GetEnumerableOfType<T>()
            => GetEnumerableOfClassType<T>();

        private static IEnumerable<Type> GetEnumerableOfClassType<T>()
            => GetAssemblies().SelectMany(x => x.GetTypes()).Where(myType =>
                myType.IsClass && (myType.IsSubclassOf(typeof(T)) || typeof(T).IsAssignableFrom(myType)) &&
                !myType.IsAbstract && myType != typeof(T));

        private static IEnumerable<Assembly> GetAssemblies()
        {
            var assemblies = new List<Assembly>();
            foreach (var assemblyName in AssemblyNames)
            {
                try
                {
                    assemblies.Add(Assembly.Load(assemblyName));
                }
                catch
                {
                    // ignored
                }
            }

            return assemblies;
        }
    }
}