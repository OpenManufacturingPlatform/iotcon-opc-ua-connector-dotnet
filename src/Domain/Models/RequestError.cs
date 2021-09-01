using System;
using System.Diagnostics;

namespace OMP.Connector.Domain.Models
{
    public class RequestError
    {
        public string ErrorSource { get; set; }

        public string ErrorMessage { get; set; }

        public Exception Exception { get; }

        public RequestError(string errorSource, Exception ex)
        {
            this.Exception = ex.Demystify();
            this.ErrorMessage = ex.ToStringDemystified();
            this.ErrorSource = errorSource;
        }

        public RequestError(string errorSource, string errorMessage)
        {
            this.ErrorSource = errorSource;
            this.ErrorMessage = errorMessage;
        }
    }
}
