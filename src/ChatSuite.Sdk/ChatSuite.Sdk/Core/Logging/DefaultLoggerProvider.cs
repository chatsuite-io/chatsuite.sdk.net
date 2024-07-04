namespace ChatSuite.Sdk.Core.Logging;

public class DefaultLoggerProvider : ILoggerProvider
{
    private readonly ILoggerFactory _loggerFactory;

    public DefaultLoggerProvider(ILoggerFactory loggerFactory)
        => _loggerFactory = loggerFactory;

    public ILogger CreateLogger(string categoryName)
        => _loggerFactory.CreateLogger(categoryName);

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing) { }
}

