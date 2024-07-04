namespace ChatSuite.Sdk.Connection;

public interface IEvent
{
	string? Target { get; }
	Task Handle(object argument);
}