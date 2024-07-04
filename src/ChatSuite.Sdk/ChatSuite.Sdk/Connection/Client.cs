using Microsoft.AspNetCore.SignalR.Client;

namespace ChatSuite.Sdk.Connection;

internal class Client : IClient
{
	public event Func<Exception?, Task>? Closed;
	public event Func<string?, Task>? Reconnected;
	public event Func<Exception?, Task>? Reconnecting;

	private readonly List<IDisposable> _messageHandlers = [];
	private bool _disposed = false;
	private HubConnection? _hubConnection;

	public void Build()
	{
		_hubConnection ??= new HubConnectionBuilder()
			.WithUrl(ConnectionParameters!.Endpoint!, options =>
			{
				options.Headers.Add("x-ms-signalr-userid", SystemUserId!);
				options.Headers.Add("x-functions-key", ConnectionParameters!.SecretKey!);
			})
			.WithAutomaticReconnect([TimeSpan.Zero, TimeSpan.Zero, TimeSpan.FromMilliseconds(5)])
			.Build();
		_hubConnection.Closed += Closed;
		_hubConnection.Reconnected += Reconnected;
		_hubConnection.Reconnecting += Reconnecting;
	}

	public Task ConnectAsync(CancellationToken cancellationToken)
	{
		if(_hubConnection is null)
		{
			Build();
		}
		return _hubConnection!.StartAsync(cancellationToken);
	}

	public ConnectionParameters? ConnectionParameters { private get; set; }
	public string? SystemUserId { private get; set; }

	public void On(IEvent @event) => _messageHandlers.Add(_hubConnection?.On<object>(@event.Target ?? throw new ArgumentNullException(nameof(@event.Target)), @event.Handle) ?? throw new ApplicationException($"{nameof(Build)} method must be called first."));

	public void Dispose() => DisengageHndlers();

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
			GC.SuppressFinalize(this);
			_disposed = true;
		}
	}
	public bool IsConnected() => _hubConnection?.State == HubConnectionState.Connected;

	public bool IsDisconnected() => _hubConnection?.State == HubConnectionState.Disconnected || _hubConnection is null;

	public async Task<bool> SendMessageToUserAsync(string recipient, object message, CancellationToken cancellationToken)
	{
		if (IsConnected())
		{
			await _hubConnection!.SendAsync(ServerMethods.SendMessageToUserInGroup.ToString(), recipient,message, cancellationToken);
			return true;
		}
		return false;
	}

	public async Task<bool> SendMessageToGroupAsync(object message, CancellationToken cancellationToken)
	{
		if (IsConnected())
		{
			await _hubConnection!.SendAsync(ServerMethods.SendMessageToGroup.ToString(), message, cancellationToken);
			return true;
		}
		return false;
	}

	public async Task<bool> AddUserToGroupAsync(string username, CancellationToken cancellationToken)
	{
		if(IsConnected())
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
			await _hubConnection!.SendAsync(ServerMethods.ReportUserStatusToUserInGroup.ToString(), username,statusDetails, cancellationToken);
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

	private void DisengageHndlers()
	{
		if (_hubConnection is not null)
		{
			_hubConnection.Closed -= Closed;
			_hubConnection.Reconnecting -= Reconnecting;
			_hubConnection.Reconnected -= Reconnected;
		}
		_messageHandlers.ForEach(handler => handler.Dispose());
		_messageHandlers.Clear();
	}
}
