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

	public ConnectionParameters? ConnectionParameters { private get; init; }
	public Func<Task<string?>>? AccessTokenProvider { private get; set; }
	public IPlugin<(string encryptionPublicKey, string stringToEncrypt), string>? EncryptionPlugin { private get; init; }
	internal string? SystemUserId { private get; set; }
	public IRegistry<CipherKeysTracker>? CipherKeysRegistry { private get; init; }
	public IPlugin<MessageBase, string>? SystemUserIdProviderPlugin { get; init; }

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
		RegisterEventToSendEncryptionPublicKey();
		_messageHandlers.Add(@event.Target!, new(@event, _hubConnection?.On<object>(@event.Target ?? throw new ArgumentNullException(@event.Target, nameof(@event.Target)), @event.HandleAsync) ?? throw new ApplicationException($"{nameof(Build)} method must be called first.")));

		void RegisterEventToSendEncryptionPublicKey()
		{
			if (@event.Target == TargetEvent.AcquireEncryptionPublicKey.ToString())
			{
				@event.OnResultReady += async result =>
				{
					var reportedPublicKey = (PublicKeyAcquisitionEvent.SharedPublicKey)(await result);
					await _hubConnection!.SendAsync(ServerMethods.ShareEncryptionPublicKey.ToString(), reportedPublicKey.RequesterSystemUserId, reportedPublicKey.PublicKey, CancellationToken.None);
				};
			}
		}
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

	public async Task<bool> SendMessageToUserAsync(string otherChatParty, ChatMessage message, CancellationToken cancellationToken)
	{
		if (IsConnected())
		{
			await _hubConnection!.SendAsync(ServerMethods.SendMessageToUserInGroup.ToString(), otherChatParty, Newtonsoft.Json.JsonConvert.SerializeObject(PreserveGroup(message)), cancellationToken);
			return true;
		}
		return false;
	}

	public async Task<bool> SendEncryptedMessageToUserAsync(string otherChatParty, ChatMessage message, CancellationToken cancellationToken)
	{
		ArgumentNullException.ThrowIfNull(EncryptionPlugin, nameof(EncryptionPlugin));
		if (IsConnected())
		{
			var publicKey = await ConcludePublicKeyAsync(otherChatParty, cancellationToken);
			var encryptedMessage = await PreserveGroup(message).EncryptAndSerializeAsync(publicKey!, EncryptionPlugin, cancellationToken);
			if (encryptedMessage is not null)
			{
				await _hubConnection!.SendAsync(ServerMethods.SendMessageToUserInGroup.ToString(), otherChatParty, encryptedMessage, cancellationToken: cancellationToken);
				return true;
			}
		}
		return false;
	}


	public async Task<bool> SendMessageToGroupAsync(ChatMessage message, CancellationToken cancellationToken)
	{
		if (IsConnected())
		{
			await _hubConnection!.SendAsync(ServerMethods.SendMessageToGroup.ToString(), Newtonsoft.Json.JsonConvert.SerializeObject(PreserveGroup(message)), cancellationToken);
			return true;
		}
		return false;
	}

	public async Task<bool> SendMessageToGroupAsync(ChatMessage message, IPlugin<(string encryptionPublicKey, string stringToEncrypt), string> encryptionPlugin, CancellationToken cancellationToken, Action<IEnumerable<Response.Error>?>? onError = null)
	{
		if (IsConnected())
		{
			var encryptedMessage = await PreserveGroup(message).EncryptAndSerializeAsync("publicKey", encryptionPlugin, cancellationToken);
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

	public Task StopAsync(CancellationToken cancellationToken) => _hubConnection?.StopAsync(cancellationToken) ?? Task.CompletedTask;

	public IEvent AcquireUserConnectedEvent() => _messageHandlers[TargetEvent.OnUserConnected.ToString()].Event;
	public IEvent AcquireUserDisconnectedEvent() => _messageHandlers[TargetEvent.OnUserDisconnected.ToString()].Event;
	public IEvent AcquireUserMessageDeliveredEvent() => _messageHandlers[TargetEvent.MessageDeliveredToUser.ToString()].Event;
	public IEvent AcquireGroupMessageDeliveredEvent() => _messageHandlers[TargetEvent.MessageDeliveredToGroup.ToString()].Event;

#if DEBUG
	public
#else
	private
#endif
		async Task<string?> RequestPublicKeyAsync(string otherChatParty, CancellationToken cancellationToken, int requestLifetimeInMilliseconds = 10000)
	{
		var lifeTimeCancellationTokenSource = new CancellationTokenSource();
		using var linkedCancellationToken = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, lifeTimeCancellationTokenSource.Token);
		lifeTimeCancellationTokenSource.CancelAfter(requestLifetimeInMilliseconds);
		string? publicKey = null;
		try
		{
			publicKey = await SendPublicKeyRequestAsync(otherChatParty, linkedCancellationToken.Token);
		}
		catch (OperationCanceledException oex) when (oex.CancellationToken == linkedCancellationToken.Token)
		{
			if(cancellationToken.IsCancellationRequested)
			{
				throw new OperationCanceledException(oex.Message, oex, cancellationToken);
			}
			else if(lifeTimeCancellationTokenSource.IsCancellationRequested)
			{
				throw new TimeoutException($"The lifetime of the request to acquire {otherChatParty}'s public key has expired after {requestLifetimeInMilliseconds} milliseconds.");
			}
		}
		return publicKey;
	}

#if DEBUG
	public
#else
	private
#endif
		async Task<bool> IsUserOnlineAsync(string otherChatParty, CancellationToken cancellationToken, int requestLifetimeInMilliseconds = 10000)
	{
		var lifeTimeCancellationTokenSource = new CancellationTokenSource();
		using var linkedCancellationToken = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, lifeTimeCancellationTokenSource.Token);
		lifeTimeCancellationTokenSource.CancelAfter(requestLifetimeInMilliseconds);
		var isOnline = false;
		try
		{
			isOnline = await SendUserOnlineStatusQueryRequestAsync(otherChatParty, linkedCancellationToken.Token) ?? false;
		}
		catch (OperationCanceledException oex) when (oex.CancellationToken == linkedCancellationToken.Token)
		{
			if (cancellationToken.IsCancellationRequested)
			{
				throw new OperationCanceledException(oex.Message, oex, cancellationToken);
			}
			else if (lifeTimeCancellationTokenSource.IsCancellationRequested)
			{
				throw new TimeoutException($"The lifetime of the request to acquire {otherChatParty}'s public key has expired after {requestLifetimeInMilliseconds} milliseconds.");
			}
		}
		return isOnline;
	}

	private async Task<string?> ConcludePublicKeyAsync(string otherChatParty,CancellationToken cancellationToken, int requestLifetimeInMilliseconds = 10000)
	{
		ArgumentNullException.ThrowIfNull(CipherKeysRegistry, nameof(CipherKeysRegistry));
		ArgumentNullException.ThrowIfNull(SystemUserIdProviderPlugin, nameof(SystemUserIdProviderPlugin));
		ArgumentNullException.ThrowIfNull(ConnectionParameters, nameof(ConnectionParameters));
		SystemUserIdProviderPlugin.Input = ConnectionParameters with { User = otherChatParty };
		var otherChatPartySystemUserId = ((await SystemUserIdProviderPlugin.RunAsync(cancellationToken))?.Result) ?? throw new ApplicationException($"Failed to conclude the system userId for {otherChatParty}");
		if (await IsUserOnlineAsync(otherChatParty, cancellationToken, requestLifetimeInMilliseconds))
		{
			var publicKey = await RequestPublicKeyAsync(otherChatParty, cancellationToken, requestLifetimeInMilliseconds);
			CipherKeysRegistry[otherChatPartySystemUserId] = new()
			{
				OtherPartySystemUserId = otherChatPartySystemUserId,
				PublicKey = publicKey
			};
			return publicKey;
		}
		else
		{
			return CipherKeysRegistry[otherChatPartySystemUserId]?.PublicKey;
		}
	}

	private ChatMessage PreserveGroup(ChatMessage chatMessage)
	{
		chatMessage.Metadata ??= new()
		{
			SpaceId = ConnectionParameters?.Metadata?.SpaceId,
			Suite = ConnectionParameters?.Metadata?.Suite
		};
		return chatMessage;
	}

	private async Task<string?> SendPublicKeyRequestAsync(string otherChatParty, CancellationToken cancellationToken)
	{
		var taskCompletionSource = new TaskCompletionSource<string?>();
		try
		{
			if (IsConnected())
			{
				var handler = _messageHandlers[TargetEvent.PublicKeyReceived.ToString()]!;
				handler.Event.OnResultReady += async result =>
				{
					var jsonElement = (System.Text.Json.JsonElement)await result;
					taskCompletionSource.TrySetResult(jsonElement.GetString()!);
				};
				await _hubConnection!.SendAsync(ServerMethods.AcquireEncryptionPublicKey.ToString(), otherChatParty, cancellationToken);
			}
			else
			{
				taskCompletionSource.TrySetResult(null);
			}
		}
		catch (Exception ex)
		{
			taskCompletionSource.TrySetException(ex);
		}
		using (cancellationToken.Register(() => taskCompletionSource.TrySetCanceled()))
		{
			return await taskCompletionSource.Task;
		}
	}

	private async Task<bool?> SendUserOnlineStatusQueryRequestAsync(string otherChatParty, CancellationToken cancellationToken)
	{
		var taskCompletionSource = new TaskCompletionSource<bool?>();
		try
		{
			if (IsConnected())
			{
				var handler = _messageHandlers[TargetEvent.OnlineOfflineStatusReported.ToString()]!;
				handler.Event.OnResultReady += async result =>
				{
					taskCompletionSource.TrySetResult((bool)await result);
				};
				await _hubConnection!.SendAsync(ServerMethods.UserConnectionStatus.ToString(), otherChatParty, cancellationToken);
			}
			else
			{
				taskCompletionSource.TrySetResult(null);
			}
		}
		catch (Exception ex)
		{
			taskCompletionSource.TrySetException(ex);
		}
		using (cancellationToken.Register(() => taskCompletionSource.TrySetCanceled()))
		{
			return await taskCompletionSource.Task;
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
