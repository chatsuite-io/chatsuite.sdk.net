
namespace ChatSuite.Sdk.Plugins.Security.Encryption;

internal class EncryptStringPlugin : Plugin<(string encryptionPublicKey, string stringToEncrypt), string>, IInputValidator
{
	public List<string> RuleSets => [];

	public ValidationResult Validate() => new EncryptStringPluginValidator().Validate(this);

	protected override Task ExecuteAsync(Response<string> response, CancellationToken cancellationToken)
	{
		using var rsa = new RSACryptoServiceProvider(EncryptionKeyGeneratorPlugin.DwKeySize);
		rsa.ImportCspBlob(Convert.FromBase64String(Input.encryptionPublicKey));
		var encryptedData = rsa.Encrypt(Encoding.UTF8.GetBytes(Input.stringToEncrypt), false);
		response.Result = Convert.ToBase64String(encryptedData);
		return Task.CompletedTask;
	}
}
