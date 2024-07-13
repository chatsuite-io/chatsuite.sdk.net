using ChatSuite.Sdk.Connection;
using ChatSuite.Sdk.Plugin.Security;

namespace ChatSuite.Sdk.Extensions.DependencyInjection;

public static class DependencyInjectionExtensions
{
	public static IServiceCollection AddChatSuiteClient(this IServiceCollection services) => services
		.AddTransient<IPlugin<ConnectionParameters, IClient?>, ChatClientBuilder>()
		.AddSingleton<IValidator<ConnectionParameters>, ConnectionParametersValidator>()
		.AddUserIdProviders()
		.AddTransient<IPlugin<EntraIDDaemonTokenAcquisitionSettings, string?>,EntraIDDaemonTokenAcquisitionPlugin>();
}
