namespace ChatSuite.Sdk.Security;

public interface IAccessTokenProvider
{
	Task<string?> GetAccessTokenAsync(CancellationToken cancellationToken);
}
