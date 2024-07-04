using ChatSuite.Core.Sdk.Plugins.SystemUser;

namespace ChatSuite.Sdk.Extensions;

public static class UserPluginExtensions
{
	public static async Task<string?> GetSignalRGroupNameAsync(this IPlugin<string, (string? spaceId, string? suite, string? username)> userIdDecomposer)
	{
		var elements = await userIdDecomposer.RunAsync(CancellationToken.None);
		return elements.DenotesSuccess() ? (elements.Result.spaceId!, elements.Result.suite!).GetSignalRGroupName() : null;
	}

	public static async Task<string?> GetUserSystemUserNameAsync(
		this IPlugin<MessageBase, string> systemUserIdProvider,
		IPlugin<string, (string? spaceId, string? suite, string? username)> userIdDecomposer,
		string username)
	{
		var elements = await userIdDecomposer.RunAsync(CancellationToken.None);
		if(elements.DenotesSuccess())
		{
			systemUserIdProvider.Input = new ChatMessage
			{
				User = username,
				Metadata = new()
				{
					SpaceId = elements.Result.spaceId!,
					Suite = elements.Result.suite!
				}
			};
			var recipientSystemUsername = await systemUserIdProvider.RunAsync(CancellationToken.None);
			if(recipientSystemUsername.DenotesSuccess())
			{
				return recipientSystemUsername.Result;
			}
		}
		return null;
	}

	public static string GetSignalRGroupName(this (string spaceId, string suite) groupSpecs) => $"{groupSpecs.spaceId}{SystemUserIdProviderPlugin.Delimiter}{groupSpecs.suite}".ToBase64();

	public static (string? spaceId, string? suite) GetSpaceAndSuite(this string groupName)
	{
		var delimitedGroupName = groupName.FromBase64();
		if (delimitedGroupName is not null)
		{
			var split = delimitedGroupName.Split(SystemUserIdProviderPlugin.Delimiter);
			return (split[0], split[1]);
		}
		return (null, null);
	}
}
