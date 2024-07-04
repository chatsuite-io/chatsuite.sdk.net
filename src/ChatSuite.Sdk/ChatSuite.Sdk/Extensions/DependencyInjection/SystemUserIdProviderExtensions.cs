using ChatSuite.Core.Sdk.Plugins.SystemUser;

namespace ChatSuite.Sdk.Extensions.DependencyInjection;

public static class SystemUserIdProviderExtensions
{
    public static IServiceCollection AddUserIdProviders(this IServiceCollection services) => services
		.AddSingleton<IPlugin<MessageBase, string>, SystemUserIdProviderPlugin>()
		.AddSingleton<IPlugin<string, (string? spaceId, string? suite, string? reciever)>, SystemUserIdDecomposerPlugin>()
		.AddSingleton<IValidator<MessageBase>, SystemUserIdProviderPluginValidator>()
		.AddSingleton<IValidator<Metadata>, MessageMetadataValidator>()
		.AddKeyedSingleton<IValidator<string>, SystemUserIdValidator>(nameof(SystemUserIdDecomposerPlugin))
		.AddSingleton<IPlugin<string, (string? spaceId, string? suite, string? reciever)>, SystemUserIdDecomposerPlugin>();
}
