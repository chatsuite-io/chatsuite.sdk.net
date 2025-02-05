﻿using ChatSuite.Sdk.Connection;
using ChatSuite.Sdk.Security.Encryption;
using ChatSuite.Sdk.Security.EntraId;

namespace ChatSuite.Sdk.Extensions.DependencyInjection;

public static class DependencyInjectionExtensions
{
	public const string EncryptionPluginKey = "Encrypt";
	public const string DecryptionPluginKey = "Decrypt";

	public static IServiceCollection AddChatSuiteClient(this IServiceCollection services) => services
		.AddTransient<IPlugin<ConnectionParameters, IClient?>, ChatClientBuilder>()
		.AddSingleton<IValidator<ConnectionParameters>, ConnectionParametersValidator>()
		.AddUserIdProviders()
		.AddTransient<IPlugin<EntraIdDaemonTokenAcquisitionSettings, string?>, EntraIdDaemonTokenAcquisitionPlugin>()
		.AddKeyedTransient<IEvent, UserConnectedEvent>(nameof(UserConnectedEvent))
		.AddKeyedTransient<IEvent, UserDisconnectedEvent>(nameof(UserDisconnectedEvent))
		.AddKeyedTransient<IEvent, PublicKeyAcquisitionEvent>(nameof(PublicKeyAcquisitionEvent))
		.AddKeyedTransient<IEvent, PublicKeyReceivedEvent>(nameof(PublicKeyReceivedEvent))
		.AddKeyedTransient<IEvent, MessageDeliveredToUserEvent>(nameof(MessageDeliveredToUserEvent))
		.AddKeyedTransient<IEvent, MessageDeliveredToGroupEvent>(nameof(MessageDeliveredToGroupEvent))
		.AddKeyedTransient<IEvent, StatusReportReceivedEvent>(nameof(StatusReportReceivedEvent))
		.AddKeyedTransient<IEvent, UserOnlineOfflineStatusReportReceived>(nameof(UserOnlineOfflineStatusReportReceived))
		.AddKeyedTransient<IEvent, SecureGroupUsersDelivered>(nameof(SecureGroupUsersDelivered));

	public static IServiceCollection AddEntraIdDaemonAccessTokenProvider(this IServiceCollection services, IConfiguration configuration) => services
		.AddTransient<IAccessTokenProvider, EntraIdDaemonAccessTokenProvider>()
		.Configure<EntraIdDaemonTokenAcquisitionSettings>(configuration!.GetSection(nameof(EntraIdDaemonTokenAcquisitionSettings)));

	public static IServiceCollection AddEncryptionPlugins(this IServiceCollection services) => services
		.AddTransient<IPlugin<int, CipherKeys>, EncryptionKeyGeneratorPlugin>()
		.AddKeyedTransient<IPlugin<(string encryptionPublicKey, string stringToEncrypt), string>, EncryptStringPlugin>(EncryptionPluginKey);

	public static IServiceCollection AddDecryptionPlugins(this IServiceCollection services) => services
		.AddTransient<IPlugin<int, CipherKeys>, EncryptionKeyGeneratorPlugin>()
		.AddKeyedTransient<IPlugin<(string encryptionPrivateKey, string encryptedString), string>, DecryptStringPlugin>(DecryptionPluginKey);

	public static IServiceCollection AddEncryptionKeyRegistry(this IServiceCollection services) => services
		.AddSingleton<IRegistry<CipherKeysTracker>, EncryptionKeyRegistry>();

	public static IServiceCollection AddSecureGroupUsersRegistry(this IServiceCollection services) => services
		.AddSingleton<IRegistry<SecureGroupUsers>, SecureGroupUsersRegistry>();
}