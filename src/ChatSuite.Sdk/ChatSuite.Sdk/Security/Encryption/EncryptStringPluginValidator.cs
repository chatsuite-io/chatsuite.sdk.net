namespace ChatSuite.Sdk.Security.Encryption;

internal class EncryptStringPluginValidator : AbstractValidator<EncryptStringPlugin>
{
	public EncryptStringPluginValidator()
	{
		RuleFor(plugin => plugin.Input.stringToEncrypt).NotEmpty();
		RuleFor(plugin => plugin.Input.encryptionPublicKey).NotEmpty();
	}
}