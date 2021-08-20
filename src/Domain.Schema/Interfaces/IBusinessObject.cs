using System.Collections.Generic;
using OMP.Connector.Domain.Schema.Abstraction.Relations;

namespace OMP.Connector.Domain.Schema.Interfaces
{
    public interface IBusinessObject : IModel
    {
        IEnumerable<AllowedRelation> AllowedRelations { get; }
        string Etag { get; }
    }
}