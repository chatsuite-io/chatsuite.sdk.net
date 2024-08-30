namespace ChatSuite.Sdk.Security.Encryption;

internal class DecryptStringPlugin : Plugin<(string encryptionPrivateKey, string encryptedString), string>, IInputValidator
{
	public List<string> RuleSets => [];

	public ValidationResult Validate() => new DecryptStringPluginValidator().Validate(this);

	protected override Task ExecuteAsync(Response<string> response, CancellationToken cancellationToken)
	{
		using var rsa = new RSACryptoServiceProvider(EncryptionKeyGeneratorPlugin.DwKeySize);
		rsa.ImportCspBlob(Convert.FromBase64String(Input.encryptionPrivateKey));
		var decryptedData = rsa.Decrypt(Convert.FromBase64String(Input.encryptedString), true);
		response.Result = Encoding.UTF8.GetString(decryptedData);
		return Task.CompletedTask;
	}
}
