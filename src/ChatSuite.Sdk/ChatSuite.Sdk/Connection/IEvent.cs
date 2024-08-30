namespace ChatSuite.Sdk.Connection;

public interface IEvent
{
	event Action<Task<object>>? OnResultReady;
	event Action<string>? OnErrored;
	string? Target { get; }
	Task HandleAsync(object argument);
}