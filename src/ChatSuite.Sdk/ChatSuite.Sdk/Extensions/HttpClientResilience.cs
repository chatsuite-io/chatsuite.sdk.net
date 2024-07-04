using Polly.Extensions.Http;
using Polly.Registry;

namespace ChatSuite.Sdk.Extensions;

internal static class HttpClientResilienceExtensions
{
    private static readonly SemaphoreSlim _semaphore1 = new(1, 1);
    private static readonly SemaphoreSlim _semaphore2 = new(1, 1);

    public static IHttpClientBuilder AddHttpRequestExceptionPolicy(
        this IHttpClientBuilder httpClientBuilder,
        IPolicyRegistry<string> policyRegistry,
        int maxNumberOfRetries = 3,
        int delayBetweenRetriesInMilliseconds = 10)
    {
        const string PolicyKey = "exceptionretrypolicy";
        _semaphore1.Wait();
        try
        {
            var exceptionRetryPolicy = Policy<HttpResponseMessage>
            .Handle<HttpRequestException>()
            .Or<OperationCanceledException>()
            .WaitAndRetryAsync(maxNumberOfRetries, _ => TimeSpan.FromMilliseconds(delayBetweenRetriesInMilliseconds));
            if (!policyRegistry.TryGet<IsPolicy>(PolicyKey, out var _))
            {
                policyRegistry.Add(PolicyKey, exceptionRetryPolicy);
            }
            return httpClientBuilder.AddPolicyHandlerFromRegistry(PolicyKey);
        }
        finally
        {
            _semaphore1.Release();
        }
    }

    public static IHttpClientBuilder AddTransientHttpErrorWaitAndRetryPolicy(
        this IHttpClientBuilder httpClientBuilder,
        IPolicyRegistry<string> policyRegistry,
        int maxNumberOfRetries = 3,
        int delayBetweenRetriesInMilliseconds = 10)
    {
        const string PolicyKey = "transienterrorpolicy";
        _semaphore2.Wait();
        try
        {
            if (!policyRegistry.TryGet<IsPolicy>(PolicyKey, out var _))
            {
                policyRegistry.Add(PolicyKey, HttpPolicyExtensions
                .HandleTransientHttpError()
                .WaitAndRetryAsync(maxNumberOfRetries, _ => TimeSpan.FromSeconds(delayBetweenRetriesInMilliseconds)));
            }
            return httpClientBuilder.AddPolicyHandlerFromRegistry(PolicyKey);
        }
        finally
        {
            _semaphore2.Release();
        }
    }
}
