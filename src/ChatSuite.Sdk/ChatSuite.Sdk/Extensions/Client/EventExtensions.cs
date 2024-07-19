namespace ChatSuite.Sdk.Extensions.Client;

public static class EventExtensions
{
	public static TargetEvent GetTargetEvent(this PartyStatus partyStatus) => partyStatus switch
	{
		PartyStatus.Unknown => throw new ApplicationException(),
		PartyStatus.JoinedTheGroup or PartyStatus.LeftTheGroup => throw new NotSupportedException(),
		PartyStatus.Connected => TargetEvent.OnUserConnected,
		PartyStatus.Disconnected => TargetEvent.OnUserDisconnected,
		PartyStatus.StatusReported => TargetEvent.UserStatusReported,
		_ => throw new NotImplementedException(),
	};
}
