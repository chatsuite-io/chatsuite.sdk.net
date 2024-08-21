using ChatSuite.Sdk.Security.Encryption;

namespace ChatSuite.Sdk.Connection.Events;

internal class KeyAcquisitionEvent(IEncryptionKeyRegistry privateEncryptionKeyTracker, IPlugin<int, CipherKeys> encryptionKeyGeneratorPlugin) : IEvent
{
	public event Action<Task<object>>? OnResultReady;
	public string? Target => TargetEvent.AcquireEncryptionPublicKey.ToString();

	public async Task Handle(object argument)
	{
		var requesterSystemUserId = (string)argument;
		var cipherKeys = await encryptionKeyGeneratorPlugin.RunAsync(CancellationToken.None);
		privateEncryptionKeyTracker[requesterSystemUserId] = cipherKeys.Result ?? throw new ArgumentNullException();
		OnResultReady?.Invoke(Task.FromResult<object>(new SharedPublicKey(requesterSystemUserId, cipherKeys.Result.PublicKey)));
	}

	public record SharedPublicKey(string RequesterSystemUserId, string PublicKey);
}

internal class PublicKeyReceivedEvent : IEvent
{
	public string? Target => TargetEvent.PublicKeyReceived.ToString();

	public event Action<Task<object>>? OnResultReady;

	public Task Handle(object argument)
	{
		OnResultReady?.Invoke(Task.FromResult(argument));
		return Task.CompletedTask;
	}
}