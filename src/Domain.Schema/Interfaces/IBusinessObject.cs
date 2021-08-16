using System.Collections.Generic;
using Omp.Connector.Domain.Schema.Abstraction.Relations;

namespace Omp.Connector.Domain.Schema.Interfaces
{
    public interface IBusinessObject : IModel
    {
        IEnumerable<AllowedRelation> AllowedRelations { get; }
        string Etag { get; }
    }
}