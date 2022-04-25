using System;

namespace OMP.Connector.Infrastructure.OpcUa.Extensions
{
    public static class IntervalExtensions
    {
        public static int ToMilliseconds(this int seconds)
            => (int)TimeSpan.FromSeconds(seconds).TotalMilliseconds;
    }
}
