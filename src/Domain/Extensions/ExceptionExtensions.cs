using System;
using System.Diagnostics;

namespace OMP.Connector.Domain.Extensions
{
    public static class ExceptionExtensions
    {
        public static string GetMessage(this Exception exception)
        {
            var error = exception.Demystify();
            var message = error.Message;

            if (exception.InnerException != default)
            {
                message = $"{message}{Constants.ErrorMessageSeparator}{error.InnerException?.GetMessage()}";
            }

            return message;
        }
    }
}