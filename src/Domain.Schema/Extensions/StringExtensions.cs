namespace OMP.Connector.Domain.Schema.Extenions
{
    public static class StringExtensions
    {
        internal static T ToEnum<T>(this string data)
            => $"\"{data}\"".Deserialize<T>();
    }
}