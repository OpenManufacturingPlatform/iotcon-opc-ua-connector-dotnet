// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

namespace OMP.PlantConnectivity.OpcUa.Extensions
{
    internal static class IntervalExtensions
    {
        public static int ToMilliseconds(this int seconds)
            => (int)TimeSpan.FromSeconds(seconds).TotalMilliseconds;
    }
}
