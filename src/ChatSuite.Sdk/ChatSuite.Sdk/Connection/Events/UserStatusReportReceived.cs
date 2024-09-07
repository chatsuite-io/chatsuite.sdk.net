namespace ChatSuite.Sdk.Connection.Events;

internal class UserStatusReportReceived : IEvent
{
	public string? Target => TargetEvent.OnlineOfflineStatusReported.ToString();

	public event Action<Task<object>>? OnResultReady;
	public event Action<string>? OnErrored;

	public Task HandleAsync(object argument)
	{
		var rawText = ((System.Text.Json.JsonElement)argument).GetRawText() ?? string.Empty;
		var statusMessage = Newtonsoft.Json.JsonConvert.DeserializeObject<StatusMessage>(rawText);
		if(statusMessage is not null)
		{
			OnResultReady?.Invoke(Task.FromResult((object)(statusMessage.StatusDetails?.Title?.ToLower() == StatusMessage.OnlineStatus.ToLower())));
		}
		else
		{
			OnErrored?.Invoke($"The {nameof(StatusMessage)} object cannot be deserialized out of the received object.");
		}
		return Task.CompletedTask;
	}
}
