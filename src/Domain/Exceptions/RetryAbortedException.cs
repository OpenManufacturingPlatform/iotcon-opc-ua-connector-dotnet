using System;

namespace OMP.Connector.Domain.Exceptions
{
    public class RetryAbortedException: RetryException
    {
        public RetryAbortedException(Exception exception, int retryAttempt)
            : base("Retry of operation was aborted.", exception, retryAttempt) { }
    }
}