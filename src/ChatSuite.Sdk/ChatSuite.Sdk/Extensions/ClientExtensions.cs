using ChatSuite.Sdk.Connection;

namespace ChatSuite.Sdk.Extensions;

public static class ClientExtensions
{
	public static async Task<IClient?> BuildAsync(this IPlugin<ConnectionParameters, IClient?> plugin, ConnectionParameters connectionParameters, Action<Response<IClient?>>? onError)
	{
		plugin.Input = connectionParameters;
		var response = await plugin.RunAsync(CancellationToken.None);
		if(!response.DenotesSuccess() && onError is not null)
		{
			onError(response);
		}
		return response.Result;
	}

	public static IClient RegisterEvent(this IClient client, IEvent @event)
	{
		client.On(@event);
		return client;
	}
}