
namespace ChatSuite.Sdk.Plugins.Security.Encryption;

internal class DecryptStringPluginValidator : AbstractValidator<DecryptStringPlugin>
{
	public DecryptStringPluginValidator()
	{
		RuleFor(plugin => plugin.Input.encryptedString).NotEmpty();
		RuleFor(plugin => plugin.Input.encryptionPrivateKey).NotEmpty();
	}
}