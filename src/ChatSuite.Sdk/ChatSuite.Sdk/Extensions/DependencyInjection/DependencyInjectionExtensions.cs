using ChatSuite.Sdk.Connection;
using ChatSuite.Sdk.Plugin.Security;
using ChatSuite.Sdk.Plugins.Security;

namespace ChatSuite.Sdk.Extensions.DependencyInjection;

public static class DependencyInjectionExtensions
{
	public static IServiceCollection AddChatSuiteClient(this IServiceCollection services, IConfiguration configuration) => services
		.AddTransient<IPlugin<ConnectionParameters, IClient?>, ChatClientBuilder>()
		.AddSingleton<IValidator<ConnectionParameters>, ConnectionParametersValidator>()
		.AddUserIdProviders()
		.AddTransient<IPlugin<EntraIdDaemonTokenAcquisitionSettings, string?>, EntraIdDaemonTokenAcquisitionPlugin>()
		.AddEntraIDDaemonAccessTokenProvider(configuration);

	public static IServiceCollection AddEntraIDDaemonAccessTokenProvider(this IServiceCollection services, IConfiguration configuration)
		=> services
		.AddTransient<IAccessTokenProvider, EntraIdDaemonAccessTokenProvider>()
		.Configure<EntraIdDaemonTokenAcquisitionSettings>(configuration!.GetSection(nameof(EntraIdDaemonTokenAcquisitionSettings)));
}
