namespace ChatSuite.Sdk.Connection.Events;

internal class UserToUserMessageReceivedEvent(IEncryptionKeyRegistry encryptionKeyRegistry) : IEvent
{
	public string? Target => TargetEvent.MessageDeliveredToUser.ToString();

	public event Action<Task<object>>? OnResultReady;

	public Task Handle(object argument)
	{
		OnResultReady?.Invoke(Task.FromResult(argument));
		return Task.CompletedTask;
	}
}
