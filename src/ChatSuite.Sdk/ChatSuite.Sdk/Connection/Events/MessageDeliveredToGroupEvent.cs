namespace ChatSuite.Sdk.Connection.Events;

internal class MessageDeliveredToGroupEvent : IEvent
{
	public string? Target => TargetEvent.MessageDeliveredToGroup.ToString();

	public event Action<Task<object>>? OnResultReady;
	public event Action<string>? OnErrored;

	public Task HandleAsync(object argument)
	{
		var rawText = ((System.Text.Json.JsonElement)argument).GetRawText() ?? string.Empty;
		var chatMessage = Newtonsoft.Json.JsonConvert.DeserializeObject<ChatMessage>(rawText);
		if (chatMessage is not null)
		{
			OnResultReady?.Invoke(Task.FromResult((object)chatMessage!));
		}
		else
		{
			OnErrored?.Invoke($"The {nameof(ChatMessage)} object cannot be deserialized out of the received object.");
		}
		return Task.CompletedTask;
	}
}
