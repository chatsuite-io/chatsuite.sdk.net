﻿using ChatSuite.Sdk.Extensions.DependencyInjection;

namespace ChatSuite.Sdk.Connection.Events;

internal class MessageDeliveredToUserEvent(
	IRegistry<CipherKeysTracker> encryptionKeyRegistry,
	IPlugin<MessageBase, string> systemUserIdProvider,
	[FromKeyedServices(DependencyInjectionExtensions.DecryptionPluginKey)]IPlugin<(string encryptionPrivateKey, string encryptedString), string> decryptionPlugin) : IEvent
{
	public string? Target => TargetEvent.MessageDeliveredToUser.ToString();

	public event Action<Task<object>>? OnResultReady;
	public event Action<string>? OnErrored;

	public async Task HandleAsync(object argument)
	{
		var rawText = ((System.Text.Json.JsonElement)argument).GetRawText() ?? string.Empty;
		var chatMessage = Newtonsoft.Json.JsonConvert.DeserializeObject<ChatMessage>(rawText);
		if (chatMessage is not null)
		{
			if (chatMessage.Metadata?.IsMessageEncrypted ?? false)
			{
				systemUserIdProvider.Input = chatMessage;
				var systemUserId = (await systemUserIdProvider.RunAsync(CancellationToken.None)).Result;
				var encryptionPrivateKey = encryptionKeyRegistry[systemUserId!]!.PrivateKey!;
				var message = await decryptionPlugin.DecryptAsync((string)chatMessage.Body!.Single(), encryptionPrivateKey, CancellationToken.None);
				chatMessage.Body = [message!];
			}
			OnResultReady?.Invoke(Task.FromResult((object)chatMessage!));
		}
		else
		{
			OnErrored?.Invoke($"The {nameof(ChatMessage)} object cannot be deserialized out of the received object.");
		}
	}
}
