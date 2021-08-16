
using Omp.Connector.Domain.Schema.Enums;

namespace Omp.Connector.Domain.Schema.Abstraction.Relations
{
    public class AllowedRelation
    {
        public AllowedRelation(BusinessObjectName businessObjectName, RelationDirection direction, Relation relation)
        {
            this.BusinessObjectName = businessObjectName;
            this.Direction = direction;
            this.Relation = relation;
        }

        public BusinessObjectName BusinessObjectName { get; }
        public RelationDirection Direction { get; }
        public Relation Relation { get; }
    }
}
