
namespace ChatSuite.Sdk.Plugins.Security.Encryption;

internal class EncryptStringPluginValidator : AbstractValidator<EncryptStringPlugin>
{
	public EncryptStringPluginValidator()
	{
		RuleFor(plugin => plugin.Input.stringToEncrypt).NotEmpty();
		RuleFor(plugin => plugin.Input.encryptionPublicKey).NotEmpty();
	}
}