using System;

namespace OMP.Connector.Domain.Exceptions
{
    public class RetryException: Exception
    {
        public int RetryAttempt { get; set; }

        public RetryException(string message, Exception exception, int retryAttempt): base(message, exception)
        {
            this.RetryAttempt = retryAttempt;
        }
    }
}