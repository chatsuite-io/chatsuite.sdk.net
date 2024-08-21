namespace ChatSuite.Sdk.Connection;

public interface IEvent
{
	event Action<Task<object>>? OnResultReady;
	string? Target { get; }
	Task Handle(object argument);
}