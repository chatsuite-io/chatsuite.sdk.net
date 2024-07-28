namespace ChatSuite.Sdk.Connection;

public interface IClient : IAsyncDisposable, IDisposable
{
	event Func<Exception?, Task>? Closed;
	event Func<string?, Task>? Reconnected;
	event Func<Exception?, Task>? Reconnecting;

	ConnectionParameters? ConnectionParameters { set; }
	Func<Task<string?>>? AccessTokenProvider { set; }

	void Build();
	bool IsConnected();
	bool IsDisconnected();
	Task StopAsync(CancellationToken cancellationToken);
	Task ConnectAsync(CancellationToken cancellationToken);
	void On(IEvent @event);
	Task<bool> SendMessageToUserAsync(string recipient, ChatMessage message, CancellationToken cancellationToken);
	Task<bool> SendMessageToUserAsync(string recipient, ChatMessage message, IPlugin<(string encryptionPublicKey, string stringToEncrypt), string> encryptionPlugin, CancellationToken cancellationToken, Action<IEnumerable<Response.Error>?>? onError = null);
	Task<bool> SendMessageToGroupAsync(ChatMessage message, CancellationToken cancellationToken);
	Task<bool> SendMessageToGroupAsync(ChatMessage message, IPlugin<(string encryptionPublicKey, string stringToEncrypt), string> encryptionPlugin, CancellationToken cancellationToken, Action<IEnumerable<Response.Error>?>? onError = null);
	Task<bool> AddUserToGroupAsync(string username, CancellationToken cancellationToken);
	Task<bool> RemoveUserFromGroupAsync(string username, CancellationToken cancellationToken);
	Task<bool> ReportStatusToUserAsync(string username, StatusDetails statusDetails, CancellationToken cancellationToken);
	Task<bool> ReportStatusToGroupAsync(StatusDetails statusDetails, CancellationToken cancellationToken);
}
