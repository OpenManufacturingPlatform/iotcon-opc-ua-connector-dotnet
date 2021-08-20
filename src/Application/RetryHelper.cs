using System;
using System.Threading.Tasks;
using OMP.Connector.Domain.Exceptions;

namespace OMP.Connector.Application
{
    public static class RetryHelper
    {
        public static async Task RetryOnExceptionAsync<TException>(
            int maxRetryCallCount,
            TimeSpan delayInSeconds,
            Func<Task> operation,
            Action<TException, int> handleErrorMessage,
            Func<TException, bool> stopRetryWhen = null) where TException : Exception
        {
            if (maxRetryCallCount <= 0)
                throw new ArgumentOutOfRangeException(nameof(maxRetryCallCount));

            var attempts = 0;
            do
            {
                try
                {
                    attempts++;
                    await operation();
                    break;
                }
                catch (TException ex)
                {
                    if (attempts == maxRetryCallCount)
                        throw new RetryFailedException(ex, attempts);

                    if (stopRetryWhen != default && stopRetryWhen.Invoke(ex))
                        throw new RetryAbortedException(ex, attempts);

                    handleErrorMessage(ex, attempts);
                    await Task.Delay(delayInSeconds);
                }
            } while (true);
        }
    }
}