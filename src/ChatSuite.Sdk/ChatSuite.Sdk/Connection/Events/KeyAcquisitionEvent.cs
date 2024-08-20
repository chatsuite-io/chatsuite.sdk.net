using ChatSuite.Sdk.Security.Encryption;

namespace ChatSuite.Sdk.Connection.Events;

internal class KeyAcquisitionEvent(IEncryptionKeyRegistry privateEncryptionKeyTracker, IPlugin<int, CipherKeys> encryptionKeyGeneratorPlugin) : IEvent
{
	public string? Target => TargetEvent.AcquireEncryptionPublicKey.ToString();

	public async Task Handle(object argument)
	{
		var requesterSystemUserId = (string)argument;
		var cipherKeys = await encryptionKeyGeneratorPlugin.RunAsync(CancellationToken.None);
		privateEncryptionKeyTracker[requesterSystemUserId] = cipherKeys.Result ?? throw new ArgumentNullException();
	}
}
