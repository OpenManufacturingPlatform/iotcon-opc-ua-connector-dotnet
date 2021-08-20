using System;
using System.Collections.Generic;
using System.Linq;

namespace OMP.Connector.Tests.Support
{
    public static class BatchSupport
    {
        public static List<int> CalculateBatches(int batchSize, int nrOfItems)
        {
            var totalBatches = (int)Math.Ceiling(nrOfItems / (double)batchSize);
            var batchSizes = Enumerable.Repeat(batchSize, totalBatches).ToList();
            var remainder = nrOfItems % batchSize;
            if (remainder != 0)
            {
                batchSizes[totalBatches - 1] = remainder;
            }
            return batchSizes;
        }
    }
}
