using ChatSuite.Sdk.Extensions.DependencyInjection;

namespace ChatSuite.Sdk.Connection.Events;

internal class UserToUserMessageReceivedEvent(
	IEncryptionKeyRegistry encryptionKeyRegistry,
	IPlugin<MessageBase, string> systemUserIdProvider,
	[FromKeyedServices(DependencyInjectionExtensions.DecryptionPluginKey)]IPlugin<(string encryptionPrivateKey, string encryptedString), string> decryptionPlugin) : IEvent
{
	public string? Target => TargetEvent.MessageDeliveredToUser.ToString();

	public event Action<Task<object>>? OnResultReady;

	public async Task HandleAsync(object argument)
	{
		var rawText = ((System.Text.Json.JsonElement)argument).GetRawText() ?? string.Empty;
		var chatMessage = Newtonsoft.Json.JsonConvert.DeserializeObject<ChatMessage>(rawText)!;
		if(chatMessage?.Metadata?.IsMessageEncrypted ?? false)
		{
			systemUserIdProvider.Input = chatMessage;
			var systemUserId = (await systemUserIdProvider.RunAsync(CancellationToken.None)).Result;
			var encryptionPrivateKey = encryptionKeyRegistry[systemUserId!]!.PrivateKey;
			var message = await decryptionPlugin.DecryptAsync((string)chatMessage.Body!.Single(), encryptionPrivateKey, CancellationToken.None);
			chatMessage.Body = [message!];
		}
		OnResultReady?.Invoke(Task.FromResult((object)chatMessage));
	}
}
