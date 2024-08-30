using System.Diagnostics;

namespace ChatSuite.Sdk.IntegrationTests.Framework;

internal abstract class TestEvent(ITestOutputHelper testOutputHelper) : IEvent
{
	public abstract string? Target {  get; }

	public event Action<Task<object>>? OnResultReady;
	public event Action<string>? OnErrored;

	public virtual Task HandleAsync(object argument)
	{
		testOutputHelper.WriteLine("@argument", argument);
		OnResultReady?.Invoke(Task.FromResult(argument));
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
					OnErrored?.Invoke("The timeout expired.");
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
