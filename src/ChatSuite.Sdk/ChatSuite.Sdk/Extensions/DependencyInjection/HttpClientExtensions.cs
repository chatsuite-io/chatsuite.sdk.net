using Polly.Registry;

namespace ChatSuite.Sdk.Extensions.DependencyInjection;

public static class HttpClientExtensions
{
    public static IHttpClientBuilder AddResilientHttpClient<T>(
        this IServiceCollection services,
        string httpClientName,
        IPolicyRegistry<string> policyRegistry)
        where T : class, new() => services
            .AddHttpClient<T>(httpClientName)
            .AddHttpRequestExceptionPolicy(policyRegistry)
            .AddTransientHttpErrorWaitAndRetryPolicy(policyRegistry);
}
