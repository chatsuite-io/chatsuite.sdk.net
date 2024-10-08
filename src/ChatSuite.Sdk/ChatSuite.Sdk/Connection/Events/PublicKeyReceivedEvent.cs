﻿namespace ChatSuite.Sdk.Connection.Events;

internal class PublicKeyReceivedEvent : IEvent
{
	public string? Target => TargetEvent.PublicKeyReceived.ToString();

	public event Action<Task<object>>? OnResultReady;
	public event Action<string>? OnErrored;

	public Task HandleAsync(object argument)
	{
		OnResultReady?.Invoke(Task.FromResult(argument));
		return Task.CompletedTask;
	}
}