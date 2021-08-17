namespace Omp.Connector.Domain.Schema.Interfaces
{
    public interface IModel
    {
        string Id { get; set; }
        string Namespace { get; set; }
        string Schema { get; set; }
    }
}