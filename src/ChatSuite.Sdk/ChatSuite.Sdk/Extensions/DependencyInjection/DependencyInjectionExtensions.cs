using ChatSuite.Sdk.Connection;
using ChatSuite.Sdk.Plugins.Security;
using ChatSuite.Sdk.Plugins.Security.EntraId;

namespace ChatSuite.Sdk.Extensions.DependencyInjection;

public static class DependencyInjectionExtensions
{
	public static IServiceCollection AddChatSuiteClient(this IServiceCollection services) => services
		.AddTransient<IPlugin<ConnectionParameters, IClient?>, ChatClientBuilder>()
		.AddSingleton<IValidator<ConnectionParameters>, ConnectionParametersValidator>()
		.AddUserIdProviders()
		.AddTransient<IPlugin<EntraIdDaemonTokenAcquisitionSettings, string?>, EntraIdDaemonTokenAcquisitionPlugin>();

	public static IServiceCollection AddEntraIdDaemonAccessTokenProvider(this IServiceCollection services, IConfiguration configuration)
		=> services
		.AddTransient<IAccessTokenProvider, EntraIdDaemonAccessTokenProvider>()
		.Configure<EntraIdDaemonTokenAcquisitionSettings>(configuration!.GetSection(nameof(EntraIdDaemonTokenAcquisitionSettings)));
}
