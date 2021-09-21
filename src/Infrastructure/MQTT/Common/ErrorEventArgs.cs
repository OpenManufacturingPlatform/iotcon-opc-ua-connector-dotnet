using System;
using System.Collections.Generic;
using System.Diagnostics;
using OMP.Connector.Domain.Extensions;

namespace OMP.Connector.Infrastructure.MQTT.Common
{
    public class ErrorEventArgs
    {
        public Exception Exception { get; }

        public string Message { get; }

        public Dictionary<string, object> Data { get; set; } = new Dictionary<string, object>();

        public ErrorEventArgs() { }

        public ErrorEventArgs(Exception exception)
        {            
            
            this.Exception = exception.Demystify();
            this.Message = exception.GetMessage();
        }

        public ErrorEventArgs(string message, Exception exception)
        {
            this.Message = message;
            this.Exception = exception.Demystify();
        }
    }
}
