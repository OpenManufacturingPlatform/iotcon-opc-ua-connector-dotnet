// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

using System.Threading.Tasks.Dataflow;

namespace ApplicationV2
{
    public class BatchHandler<T>
    {
        private readonly BatchBlock<T> batchBlock;
        private readonly ActionBlock<T[]> actionBlock;

        public BatchHandler(int batchSize, Action<T[]> action)
        {
            this.batchBlock = new BatchBlock<T>(batchSize);
            this.actionBlock = new ActionBlock<T[]>(action);
            this.batchBlock.LinkTo(this.actionBlock);
            this.batchBlock.Completion.ContinueWith(delegate { this.actionBlock.Complete(); });
        }

        public void RunBatches(List<T> data)
        {
            data.ForEach(async item => { await this.batchBlock.SendAsync(item); });

            //create final batch even if remaining items < batch size
            this.batchBlock.TriggerBatch();

            this.batchBlock.Complete();
            this.actionBlock.Completion.Wait();
        }
    }
}
