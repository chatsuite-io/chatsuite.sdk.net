namespace ChatSuite.Sdk.Connection.Events;

internal class SecureGroupUsersDelivered(IRegistry<SecureGroupUsers> registry) : IEvent
{
	public string? Target => TargetEvent.GroupUsersDelivered.ToString();

	public event Action<Task<object>>? OnResultReady;
	public event Action<string>? OnErrored;

	public Task HandleAsync(object argument)
	{
		var rawText = ((System.Text.Json.JsonElement)argument).GetRawText() ?? string.Empty;
		try
		{
			var secureGroupUsers = Newtonsoft.Json.JsonConvert.DeserializeObject<SecureGroupUsers>(rawText) ?? throw new ApplicationException($"The expected payload must be a {nameof(SecureGroupUsers)}.");
			OnResultReady?.Invoke(Task.FromResult((object)secureGroupUsers));
			registry[secureGroupUsers.GroupName!] = secureGroupUsers;
		}
		catch (Exception ex)
		{
			OnErrored?.Invoke(ex.Message);
		}
		return Task.CompletedTask;
	}
}
