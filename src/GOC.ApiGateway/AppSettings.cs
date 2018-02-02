using System.Collections.Generic;
using Microphone.Consul;

namespace GOC.ApiGateway
{
    public class AppSettings
    {
        public CircuitBreakerSettings CircuitBreaker {get;set;}
        public WaitAndRetrySettings WaitAndRetry { get; set; }
        public ConsulOptions Consul { get; set; }
        public IdentitySettings Identity { get; set; }
        public RabbitMQSettings Rabbit
        {
            get;
            set;
        }
    }
    public class CircuitBreakerSettings
    {
        public double FailureThreshold { get; set; }
        public int SamplingDurationInSeconds { get; set; }
        public int MinimumThroughput { get; set; }
        public int DurationOfBreakInSeconds { get; set; }
    }
    public class WaitAndRetrySettings
    {
        public int RetryAttempts { get; set; }
    }
    public class IdentitySettings
    {
        public string Authority { get; set; }
        public string ApiName { get; set; }
        public string ApiSecret { get; set; }
        public IList<DownstreamClient> DownstreamClients { get; set; }
    }
    public class DownstreamClient
    {
        public string ApiName { get; set; }
        public string ApiSecret { get; set; }
    }
    public class RabbitMQSettings
    {
        public string Host { get; set; }
    }

}
