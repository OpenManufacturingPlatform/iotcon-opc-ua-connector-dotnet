namespace OMP.Connector.Domain.Schema.Extentions
{
    public static class StringExtensions
    {
        internal static T ToEnum<T>(this string data)
            => $"\"{data}\"".Deserialize<T>();
    }
}