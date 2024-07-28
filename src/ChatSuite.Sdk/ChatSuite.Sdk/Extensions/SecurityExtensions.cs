namespace ChatSuite.Sdk.Extensions;

internal static class SecurityExtensions
{
	public static async Task<string?> EncryptAsync(this ChatMessage chatMessage, string publicEncryptionKey, IPlugin<(string encryptionPublicKey, string stringToEncrypt), string> encryptionPlugin, CancellationToken cancellationToken, Action<IEnumerable<Response.Error>?>? onError =null)
	{
		encryptionPlugin.Input = (publicEncryptionKey, Newtonsoft.Json.JsonConvert.SerializeObject(chatMessage.Body ?? []));
		var result = await encryptionPlugin.RunAsync(cancellationToken);
		chatMessage.Body = [result.Result ?? string.Empty];
		var success = result.DenotesSuccess() ? Newtonsoft.Json.JsonConvert.SerializeObject(chatMessage) : null;
		if(success is null)
		{
			onError?.Invoke(result.Errors);
		}
		return success;
	}

	public static async Task<string?> DecryptAsync(this IPlugin<(string encryptionPrivateKey, string encryptedString), string> decryptionPlugin, string stringToDecrypt, string privateEncryptionKey, CancellationToken cancellationToken)
	{
		decryptionPlugin.Input = (privateEncryptionKey, stringToDecrypt);
		var result = await decryptionPlugin.RunAsync(cancellationToken);
		return result.Result;
	}
}
