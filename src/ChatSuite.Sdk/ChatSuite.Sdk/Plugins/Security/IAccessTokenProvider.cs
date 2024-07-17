namespace ChatSuite.Sdk.Plugins.Security;

public interface IAccessTokenProvider
{
	Task<string?> GetAccessTokenAsync(CancellationToken cancellationToken);
}
