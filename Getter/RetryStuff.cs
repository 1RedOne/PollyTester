namespace Getter
{
    using System;
    using System.Net;
    using System.Net.Http;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Polly;
    using Polly.Extensions.Http;
    using Polly.Timeout;

    using Task = System.Threading.Tasks.Task;
    using TimeSpan = System.TimeSpan;

    public static class HttpRetryExtensions
    {
        private static readonly Random jitterer = new Random();

        public static IHttpClientBuilder AddRetries(this IHttpClientBuilder builder)
        {
            return builder.AddPolicyHandler((services, request) =>
                Policy.WrapAsync(
                    GlobalHttpTimeoutPolicy(services),
                    TooManyRequestsAsyncPolicy(services),
                    TransientHttpErrorAsyncPolicy(services)));
        }

        public static IAsyncPolicy<HttpResponseMessage> TooManyRequestsAsyncPolicy(IServiceProvider services)
        {
            return Policy<HttpResponseMessage>
                .Handle<HttpRequestException>()
                .OrResult(response => response.StatusCode == HttpStatusCode.TooManyRequests)
                .WaitAndRetryAsync(
                    retryCount: 3,
                    sleepDurationProvider: (retryAttempt, response, context) =>
                    {
                        // https://docs.microsoft.com/en-us/graph/throttling?view=graph-rest-beta#best-practices-to-handle-throttling
                        var header = response?.Result?.Headers?.RetryAfter;

                        if (header != null && header.Delta.HasValue)
                        {
                            return header.Delta.Value;
                        }
                        else
                        {
                            return TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)) + TimeSpan.FromMilliseconds(jitterer.Next(0, 500));
                        }
                    },
                    onRetryAsync: (response, timespan, retryAttempt, context) =>
                    {
                        var logger = services.GetService<ILogger<HttpResponseMessage>>();

                        logger?.LogWarning($"Service delivery attempt {retryAttempt} failed with TooManyRequests, next attempt in {timespan.TotalMilliseconds} ms.");
                        return Task.CompletedTask;
                    });
        }

        public static IAsyncPolicy<HttpResponseMessage> TransientHttpErrorAsyncPolicy(IServiceProvider services)
        {
            return HttpPolicyExtensions
                .HandleTransientHttpError() // 408 - Request Timeout and 5XX - Internal Server Error or GateWay Timeout
                .WaitAndRetryAsync(
                    retryCount: 5,
                    retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)) + TimeSpan.FromMilliseconds(jitterer.Next(0, 500)),
                    onRetryAsync: (response, timespan, retryAttempt, context) =>
                    {
                        var logger = services.GetService<ILogger<HttpResponseMessage>>();
                        var request = response?.Result?.RequestMessage?.RequestUri;

                        logger?.LogInformation($"Attempt {retryAttempt} / 5 for url {request} failed with {response?.Result?.StatusCode}, next attempt in {timespan.TotalMilliseconds} ms.");
                        return Task.CompletedTask;
                    });
        }

        /// <summary>
        /// Polly is implemented as an HttpMessageHandler, which is at a different level than httpClient
        /// This means httpClient does not know Polly is intercepting and retrying its request.
        /// HttpClient has its own timeout of 100 seconds.  For this reason, we add a global timeout of 85 seconds.
        /// Previously we waited 95 seconds but the interaction of the policies lead to still encountering this issue
        /// </summary>
        /// <returns></returns>
        public static IAsyncPolicy<HttpResponseMessage> GlobalHttpTimeoutPolicy(IServiceProvider services)
        {
            return Policy.TimeoutAsync<HttpResponseMessage>(3, TimeoutStrategy.Pessimistic, (context, timespan, task) =>
            {
                var logger = services.GetService<ILogger<HttpResponseMessage>>();
                task.ContinueWith(t =>
                { // ContinueWith important!: the abandoned task may very well still be executing, when the caller times out on waiting for it!
                    if (t.IsFaulted)
                    {
                        logger?.LogError($"{context.PolicyKey} at {context.OperationKey}: execution timed out after {timespan.TotalSeconds} seconds, eventually terminated with: {t.Exception}.");
                    }
                    else if (t.IsCanceled)
                    {
                        // (If the executed delegates do not honour cancellation, this IsCanceled branch may never be hit.  It can be good practice however to include, in case a Policy configured with TimeoutStrategy.Pessimistic is used to execute a delegate honouring cancellation.)
                        logger?.LogError($"{context.PolicyKey} at {context.OperationKey}: execution timed out after {timespan.TotalSeconds} seconds, task cancelled.");
                    }
                });
                return Task.CompletedTask;
            });
        }
    }
}