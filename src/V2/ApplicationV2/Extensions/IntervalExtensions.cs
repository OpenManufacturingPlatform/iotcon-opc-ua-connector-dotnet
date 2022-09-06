// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

using System;

namespace OMP.PlantConnectivity.OpcUA.Extensions
{
    public static class IntervalExtensions
    {
        public static int ToMilliseconds(this int seconds)
            => (int)TimeSpan.FromSeconds(seconds).TotalMilliseconds;
    }
}
