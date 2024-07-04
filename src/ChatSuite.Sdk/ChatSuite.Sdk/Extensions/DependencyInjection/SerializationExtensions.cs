using System.Text.Json;

namespace ChatSuite.Sdk.Extensions.DependencyInjection;

public static class SerializationExtensions
{
    public static IServiceCollection AddDefaultJsonSerializationOptions(this IServiceCollection services) =>
        services.Configure<JsonSerializerOptions>(options =>
        {
            options.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
            options.PropertyNameCaseInsensitive = true;
        });
}