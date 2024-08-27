﻿using ChatSuite.Sdk.Security.Encryption;
using System.Text.Json;

namespace ChatSuite.Sdk.Connection.Events;

internal class PublicKeyAcquisitionEvent(IEncryptionKeyRegistry encryptionKeyRegistry, IPlugin<int, CipherKeys> encryptionKeyGeneratorPlugin) : IEvent
{
	public event Action<Task<object>>? OnResultReady;
	public string? Target => TargetEvent.AcquireEncryptionPublicKey.ToString();

	public async Task Handle(object argument)
	{
		var jsonElement = (JsonElement)argument;
		var requesterSystemUserId = jsonElement.GetString()!;
		var cipherKeys = await encryptionKeyGeneratorPlugin.RunAsync(CancellationToken.None);
		encryptionKeyRegistry[requesterSystemUserId] = cipherKeys.Result ?? throw new ArgumentNullException();
		OnResultReady?.Invoke(Task.FromResult<object>(new SharedPublicKey(requesterSystemUserId, cipherKeys.Result.PublicKey)));
	}

	public record SharedPublicKey(string RequesterSystemUserId, string PublicKey);
}