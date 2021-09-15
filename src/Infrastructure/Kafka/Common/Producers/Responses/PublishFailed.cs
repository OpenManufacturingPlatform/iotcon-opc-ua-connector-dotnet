using System;

namespace OMP.Connector.Infrastructure.Kafka.Common.Producers.Responses
{
    public class PublishFailed
    {
        public string Error { get; }
        public Exception Ex { get; }

        public PublishFailed(string error)
        {
            Error = error;
        }
        public PublishFailed(Exception ex)
        {
            Ex = ex;
            Error = GetErrorString(ex);
        }

        public PublishFailed(string message, Exception ex)
        {
            Ex = ex;
            Error = message;
        }

        private static string GetErrorString(Exception ex)
        {
            if (ex is null)
                return default;

            if (ex.InnerException is null)
                return ex.Message;

            return $"{ex.Message}\t|\t{GetErrorString(ex.InnerException)}";
        }
    }
}
