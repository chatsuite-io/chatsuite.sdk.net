﻿using System.Diagnostics;

namespace ChatSuite.Sdk.IntegrationTests.Framework;

internal abstract class TestEvent(ITestOutputHelper testOutputHelper) : IEvent
{
	public abstract string? Target {  get; }

	public virtual Task Handle(object argument)
	{
		testOutputHelper.WriteLine("@argument", argument);
		return Task.CompletedTask;
	}

	public async Task<bool> WaitAsync(Func<bool> predicate, CancellationToken cancellationToken)
	{
		var stopWatch = new Stopwatch();
		stopWatch.Start();
		try
		{
			do
			{
				cancellationToken.ThrowIfCancellationRequested();
				if (stopWatch.ElapsedMilliseconds >= 30000)
				{
					return false;
				}
				await Task.Delay(1, cancellationToken);
			}
			while (!predicate());
			return true;
		}
		finally
		{
			stopWatch.Stop();
		}
	}
}
