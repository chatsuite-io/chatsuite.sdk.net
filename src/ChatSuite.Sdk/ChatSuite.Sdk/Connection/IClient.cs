namespace ChatSuite.Sdk.Connection;

public interface IClient : IAsyncDisposable, IDisposable
{
	event Func<Exception?, Task>? Closed;
	event Func<string?, Task>? Reconnected;
	event Func<Exception?, Task>? Reconnecting;

	ConnectionParameters? ConnectionParameters { init; }
	Func<Task<string?>>? AccessTokenProvider { set; }
	IPlugin<(string encryptionPublicKey, string stringToEncrypt), string>? EncryptionPlugin { init; }
	IRegistry<CipherKeysTracker>? CipherKeysRegistry { init; }
	IPlugin<MessageBase, string>? SystemUserIdProviderPlugin { init; }
	void Build();
	bool IsConnected();
	bool IsDisconnected();
	Task StopAsync(CancellationToken cancellationToken);
	Task ConnectAsync(CancellationToken cancellationToken);
	void On(IEvent @event);
	IEvent AcquireUserConnectedEvent();
	IEvent AcquireUserDisconnectedEvent();
	IEvent AcquireUserMessageDeliveredEvent();
	IEvent AcquireGroupMessageDeliveredEvent();
	IEvent AcquireStatusReportReceivedEvent();
	Task<bool> SendMessageToUserAsync(string recipient, ChatMessage message, CancellationToken cancellationToken);
	Task<bool> SendEncryptedMessageToUserAsync(string recipient, ChatMessage message, CancellationToken cancellationToken);
	Task<bool> SendMessageToGroupAsync(ChatMessage message, CancellationToken cancellationToken);
	Task<bool> SendMessageToGroupAsync(ChatMessage message, IPlugin<(string encryptionPublicKey, string stringToEncrypt), string> encryptionPlugin, CancellationToken cancellationToken, Action<IEnumerable<Response.Error>?>? onError = null);
	Task<bool> AddUserToGroupAsync(string username, CancellationToken cancellationToken);
	Task<bool> RemoveUserFromGroupAsync(string username, CancellationToken cancellationToken);
	Task<bool> ReportStatusToUserAsync(string username, StatusDetails statusDetails, CancellationToken cancellationToken);
	Task<bool> ReportStatusToGroupAsync(StatusDetails statusDetails, CancellationToken cancellationToken);
#if DEBUG
	Task<string?> RequestPublicKeyAsync(string recipient, CancellationToken cancellationToken, int requestLifetimeInMilliseconds = 10000);
	Task<bool> IsUserOnlineAsync(string otherChatParty, CancellationToken cancellationToken, int requestLifetimeInMilliseconds = 10000);
#endif
}
