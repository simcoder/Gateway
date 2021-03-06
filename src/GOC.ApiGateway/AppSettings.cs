﻿using System.Collections.Generic;
using Microphone.Consul;

namespace GOC.ApiGateway
{
    public class AppSettings
    {
        public CircuitBreakerSettings CircuitBreaker {get;set;}
        public WaitAndRetrySettings WaitAndRetry { get; set; }
        public ConsulOptions Consul { get; set; }
        public IdentitySettings Identity { get; set; }
        public RabbitMQSettings Rabbit { get; set; }
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
        public string TokenEndpoint { get; set; }
        public string ApiClientId { get; set; }
        public string ApiClientSecret { get; set; }
        public string TokenEndpointUrl
        {
            get => $"{Authority}/{TokenEndpoint}";
        }
        public IEnumerable<ApiResource> Resources { get; set; }
    }
    //TODO do this in json config file
    public class ApiResource
    {
        public string ResourceFriendlyName { get; set; }
        public string ResourceName { get; set; }
    }
    public class RabbitMQSettings
    {
        public string Host { get; set; }
    }

}
