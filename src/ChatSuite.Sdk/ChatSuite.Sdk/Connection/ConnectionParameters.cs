using System.Diagnostics.CodeAnalysis;

namespace ChatSuite.Sdk.Connection;

public record ConnectionParameters : MessageBase
{
	public static ConnectionParameters Instantiate(
		[DisallowNull] string user,
		[DisallowNull] string suite,
		[DisallowNull] string spaceId,
		string? endPoint = null,
		string? endPointSecret = null,
		string? clientId = null) => new()
		{
			Id = Guid.NewGuid().ToString(),
			User = user,
			Endpoint = endPoint,
			SecretKey = endPointSecret,
			Metadata = new()
			{
				ClientId = clientId ?? Guid.NewGuid().ToString(),
				Suite = suite,
				SpaceId = spaceId
			}
		};
	public string? SecretKey { internal get; set; }
	public string? Endpoint { internal get; set; }
}
