using ChatSuite.Sdk.Security.Encryption;
using System.Text.Json;

namespace ChatSuite.Sdk.Connection.Events;

internal class PublicKeyAcquisitionEvent(IRegistry<CipherKeysTracker> encryptionKeyRegistry, IPlugin<int, CipherKeys> encryptionKeyGeneratorPlugin) : IEvent
{
	public event Action<Task<object>>? OnResultReady;
	public event Action<string>? OnErrored;

	public string? Target => TargetEvent.AcquireEncryptionPublicKey.ToString();

	public async Task HandleAsync(object argument)
	{
		var jsonElement = (JsonElement)argument;
		var requesterSystemUserId = jsonElement.GetString()!;
		var cipherKeys = await encryptionKeyGeneratorPlugin.RunAsync(CancellationToken.None);
		encryptionKeyRegistry[requesterSystemUserId] = CipherKeysTracker.Create(requesterSystemUserId, cipherKeys.Result) ?? throw new ArgumentNullException();
		OnResultReady?.Invoke(Task.FromResult<object>(new SharedPublicKey(requesterSystemUserId, cipherKeys.Result.PublicKey)));
	}

	public record SharedPublicKey(string RequesterSystemUserId, string PublicKey);
}
