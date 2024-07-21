namespace ChatSuite.Sdk.Core.Logging;

public class DefaultLoggerProvider(ILoggerFactory loggerFactory) : ILoggerProvider
{
	public ILogger CreateLogger(string categoryName)
        => loggerFactory.CreateLogger(categoryName);

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing) { }
}

