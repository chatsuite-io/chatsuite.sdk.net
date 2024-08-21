using ChatSuite.Sdk.Connection.Events;
using Microsoft.AspNetCore.SignalR.Client;

namespace ChatSuite.Sdk.Connection;

internal class Client : IClient
{
	public event Func<Exception?, Task>? Closed;
	public event Func<string?, Task>? Reconnected;
	public event Func<Exception?, Task>? Reconnecting;

	private readonly Dictionary<string, MessageHandler> _messageHandlers = [];
	private bool _disposed = false;
	private HubConnection? _hubConnection;

	public ConnectionParameters? ConnectionParameters { private get; set; }
	public Func<Task<string?>>? AccessTokenProvider { private get; set; }
	internal string? SystemUserId { private get; set; }

	public void Build()
	{
		if (_hubConnection is null)
		{
			_hubConnection ??= new HubConnectionBuilder()
				.WithUrl(ConnectionParameters!.Endpoint!, options =>
				{
					options.Headers.Add("x-ms-signalr-userid", SystemUserId!);
					options.Headers.Add("x-functions-key", ConnectionParameters!.SecretKey!);
					options.AccessTokenProvider = AccessTokenProvider;
				})
				.WithAutomaticReconnect([TimeSpan.Zero, TimeSpan.Zero, TimeSpan.FromMilliseconds(5)])
				.Build();
			_hubConnection.Closed += Closed;
			_hubConnection.Reconnected += Reconnected;
			_hubConnection.Reconnecting += Reconnecting;
		}
	}

	public Task ConnectAsync(CancellationToken cancellationToken) => _hubConnection!.StartAsync(cancellationToken);

	public void On(IEvent @event)
	{
		if(@event.Target == TargetEvent.AcquireEncryptionPublicKey.ToString())
		{
			@event.OnResultReady += async obj =>
			{
				var reportedPublicKey = (KeyAcquisitionEvent.SharedPublicKey)(await obj);
				await _hubConnection!.SendAsync(ServerMethods.ShareEncryptionPublicKey.ToString(), reportedPublicKey.RequesterSystemUserId, reportedPublicKey.PublicKey, CancellationToken.None);
			};
		}
		_messageHandlers.Add(@event.Target!, new(@event, _hubConnection?.On<object>(@event.Target ?? throw new ArgumentNullException(@event.Target, nameof(@event.Target)), @event.Handle) ?? throw new ApplicationException($"{nameof(Build)} method must be called first.")));
	}

	public void Dispose() => DisengageHandlers();

	public async ValueTask DisposeAsync()
	{
		if (!_disposed)
		{
			await StopAsync(CancellationToken.None);
			if (_hubConnection is not null)
			{
				await _hubConnection.DisposeAsync();
			}
			Dispose();
			_hubConnection = null;
			GC.SuppressFinalize(this);
			_disposed = true;
		}
	}
	public bool IsConnected() => _hubConnection?.State == HubConnectionState.Connected;

	public bool IsDisconnected() => _hubConnection?.State == HubConnectionState.Disconnected || _hubConnection is null;

	public async Task<bool> SendMessageToUserAsync(string recipient, ChatMessage message, CancellationToken cancellationToken)
	{
		if (IsConnected())
		{
			await _hubConnection!.SendAsync(ServerMethods.SendMessageToUserInGroup.ToString(), recipient, Newtonsoft.Json.JsonConvert.SerializeObject(message), cancellationToken);
			return true;
		}
		return false;
	}

	public async Task<bool> SendMessageToUserAsync(string recipient, ChatMessage message, IPlugin<(string encryptionPublicKey, string stringToEncrypt), string> encryptionPlugin, CancellationToken cancellationToken, Action<IEnumerable<Response.Error>?>? onError = null)
	{
		if (IsConnected())
		{
			var publicKey = await RequestPublicKeyAsync();
			var encryptedMessage = await message.EncryptAndSerializeAsync(publicKey, encryptionPlugin, cancellationToken);
			if (encryptedMessage is not null)
			{
				await _hubConnection!.SendAsync(ServerMethods.SendMessageToUserInGroup.ToString(), arg1: recipient, arg2: encryptedMessage, cancellationToken: cancellationToken);
				return true;
			}
		}
		return false;
		async Task<string?> RequestPublicKeyAsync()
		{
			var taskCompletionSource = new TaskCompletionSource<string?>();
			try
			{
				var handler = _messageHandlers[TargetEvent.PublicKeyReceived.ToString()]!;
				handler.Event.OnResultReady += async obj =>
				{
					var receivedKey = (KeyAcquisitionEvent.SharedPublicKey)(await obj);
					if(receivedKey?.RequesterSystemUserId == recipient)
					{
						taskCompletionSource.SetResult(receivedKey?.PublicKey);
					}
				};
				await _hubConnection!.SendAsync(ServerMethods.AcquireEncryptionPublicKey.ToString(), recipient, SystemUserId, cancellationToken);
			}
			catch (Exception ex)
			{
				taskCompletionSource.SetException(ex);
			}
			return null;
		}
	}


	public async Task<bool> SendMessageToGroupAsync(ChatMessage message, CancellationToken cancellationToken)
	{
		if (IsConnected())
		{
			await _hubConnection!.SendAsync(ServerMethods.SendMessageToGroup.ToString(), Newtonsoft.Json.JsonConvert.SerializeObject(message), cancellationToken);
			return true;
		}
		return false;
	}

	public async Task<bool> SendMessageToGroupAsync(ChatMessage message, IPlugin<(string encryptionPublicKey, string stringToEncrypt), string> encryptionPlugin, CancellationToken cancellationToken, Action<IEnumerable<Response.Error>?>? onError = null)
	{
		if (IsConnected())
		{
			var encryptedMessage = await message.EncryptAndSerializeAsync("publicKey", encryptionPlugin, cancellationToken);
			if (encryptedMessage is not null)
			{
				await _hubConnection!.SendAsync(ServerMethods.SendMessageToUserInGroup.ToString(), encryptedMessage, cancellationToken, onError);
				return true;
			}
		}
		return false;
	}

	public async Task<bool> AddUserToGroupAsync(string username, CancellationToken cancellationToken)
	{
		if (IsConnected())
		{
			await _hubConnection!.SendAsync(ServerMethods.JoinUserToGroup.ToString(), username, cancellationToken);
			return true;
		}
		return false;
	}

	public async Task<bool> RemoveUserFromGroupAsync(string username, CancellationToken cancellationToken)
	{
		if (IsConnected())
		{
			await _hubConnection!.SendAsync(ServerMethods.LeaveUserFromGroup.ToString(), username, cancellationToken);
			return true;
		}
		return false;
	}

	public async Task<bool> ReportStatusToUserAsync(string username, StatusDetails statusDetails, CancellationToken cancellationToken)
	{
		if (IsConnected())
		{
			await _hubConnection!.SendAsync(ServerMethods.ReportUserStatusToUserInGroup.ToString(), username, statusDetails, cancellationToken);
			return true;
		}
		return false;
	}

	public async Task<bool> ReportStatusToGroupAsync(StatusDetails statusDetails, CancellationToken cancellationToken)
	{
		if (IsConnected())
		{
			await _hubConnection!.SendAsync(ServerMethods.ReportUserStatusToGroup.ToString(), statusDetails, cancellationToken);
			return true;
		}
		return false;
	}

	public async Task StopAsync(CancellationToken cancellationToken)
	{
		if (_hubConnection is not null)
		{
			await _hubConnection.StopAsync(cancellationToken);
		}
	}

	private void DisengageHandlers()
	{
		if (_hubConnection is not null)
		{
			_hubConnection.Closed -= Closed;
			_hubConnection.Reconnecting -= Reconnecting;
			_hubConnection.Reconnected -= Reconnected;
		}
		foreach (var handler in _messageHandlers)
		{
			handler.Value.Handler.Dispose();
		}
		_messageHandlers.Clear();
	}

	private record MessageHandler(IEvent Event, IDisposable Handler);
}
