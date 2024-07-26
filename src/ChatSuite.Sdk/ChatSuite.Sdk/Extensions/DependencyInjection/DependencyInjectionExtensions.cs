using ChatSuite.Sdk.Connection;
using ChatSuite.Sdk.Plugins.Security;
using ChatSuite.Sdk.Plugins.Security.Encryption;
using ChatSuite.Sdk.Plugins.Security.EntraId;

namespace ChatSuite.Sdk.Extensions.DependencyInjection;

public static class DependencyInjectionExtensions
{
	public const string EncryptionPluginKey = "Encrypt";
	public const string DecryptionPluginKey = "Decrypt";

	public static IServiceCollection AddChatSuiteClient(this IServiceCollection services) => services
		.AddTransient<IPlugin<ConnectionParameters, IClient?>, ChatClientBuilder>()
		.AddSingleton<IValidator<ConnectionParameters>, ConnectionParametersValidator>()
		.AddUserIdProviders()
		.AddTransient<IPlugin<EntraIdDaemonTokenAcquisitionSettings, string?>, EntraIdDaemonTokenAcquisitionPlugin>();

	public static IServiceCollection AddEntraIdDaemonAccessTokenProvider(this IServiceCollection services, IConfiguration configuration)
		=> services
		.AddTransient<IAccessTokenProvider, EntraIdDaemonAccessTokenProvider>()
		.Configure<EntraIdDaemonTokenAcquisitionSettings>(configuration!.GetSection(nameof(EntraIdDaemonTokenAcquisitionSettings)));

	public static IServiceCollection AddEncryptionPlugins(this IServiceCollection services) => services
		.AddTransient<IPlugin<int, CipherKeys>, EncryptionKeyGeneratorPlugin>()
		.AddKeyedTransient<IPlugin<(string encryptionPublicKey, string stringToEncrypt), string>, EncryptStringPlugin>(EncryptionPluginKey);

	public static IServiceCollection AddDecryptionPlugins(this IServiceCollection services) => services
		.AddTransient<IPlugin<int, CipherKeys>, EncryptionKeyGeneratorPlugin>()
		.AddKeyedTransient<IPlugin<(string encryptionPrivateKey, string encryptedString), string>, DecryptStringPlugin>(DecryptionPluginKey);
}