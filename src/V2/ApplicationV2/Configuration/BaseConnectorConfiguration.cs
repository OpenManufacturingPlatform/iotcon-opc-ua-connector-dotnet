// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

using System.Dynamic;
using Microsoft.Extensions.Configuration;

namespace ApplicationV2.Configuration
{
    public abstract class BaseConnectorConfiguration
    {
        protected IConfiguration Configuration { get; set; }
        public object Settings { get; set; }
        public virtual T GetConfig<T>()
           where T : new()
            => this.Configuration.Get<T>();

        public virtual void SetNativeConfig(IConfiguration configuration)
        {
            this.Configuration = configuration;
            this.Settings = this.ConvertToObject(configuration);
        }

        protected virtual object ConvertToObject(IConfiguration configuration)
        {
            return configuration.GetChildren()
                 .ToDictionary(kv => kv.Key, kv => Convert.ChangeType(kv.Value, typeof(object)))
                 .Aggregate(new ExpandoObject() as IDictionary<string, Object>,
                             (a, p) => { a.Add(p); return a; });
        }
    }
}
