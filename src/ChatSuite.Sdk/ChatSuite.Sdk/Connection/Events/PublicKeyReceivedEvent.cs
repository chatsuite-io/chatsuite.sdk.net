namespace ChatSuite.Sdk.Connection.Events;

internal class PublicKeyReceivedEvent : IEvent
{
	public string? Target => TargetEvent.PublicKeyReceived.ToString();

	public event Action<Task<object>>? OnResultReady;

	public Task Handle(object argument)
	{
		OnResultReady?.Invoke(Task.FromResult(argument));
		return Task.CompletedTask;
	}
}