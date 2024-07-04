using ChatSuite.Sdk.Connection;
using ChatSuite.Sdk.Plugin.Security;
using ChatSuite.Sdk.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

namespace ChatSuite.Sdk.Extensions.DependencyInjection;

public static class DependencyInjectionExtensions
{
	public static IServiceCollection AddChatSuiteClient(this IServiceCollection services) => services
		.AddTransient<IPlugin<ConnectionParameters, IClient?>, ChatClientBuilder>()
		.AddTransient<IClient, Connection.Client>()
		.AddSingleton<IValidator<ConnectionParameters>, ConnectionParametersValidator>()
		.AddUserIdProviders()
		.AddTransient<IPlugin<EntraIdTokenAcquisitionSettings, string?>,EntraIDTokenAcquisitionPlugin>();
}
