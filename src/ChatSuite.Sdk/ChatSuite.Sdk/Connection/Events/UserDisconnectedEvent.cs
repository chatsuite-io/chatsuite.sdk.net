namespace ChatSuite.Sdk.Connection.Events;

internal class UserDisconnectedEvent : IEvent
{
	public string? Target => TargetEvent.OnUserDisconnected.ToString();

	public event Action<Task<object>>? OnResultReady;
	public event Action<string>? OnErrored;

	public Task HandleAsync(object argument)
	{
		var rawText = ((System.Text.Json.JsonElement)argument).GetRawText() ?? string.Empty;
		try
		{
			var statusMessage = Newtonsoft.Json.JsonConvert.DeserializeObject<StatusMessage>(rawText) ?? throw new ApplicationException($"The expected payload must be a {nameof(StatusMessage)}.");
			OnResultReady?.Invoke(Task.FromResult((object)statusMessage));
		}
		catch (Exception ex)
		{
			OnErrored?.Invoke(ex.Message);
		}
		return Task.CompletedTask;
	}
}