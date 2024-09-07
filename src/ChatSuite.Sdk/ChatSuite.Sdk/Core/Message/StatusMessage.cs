namespace ChatSuite.Sdk.Core.Message;

public record StatusMessage : MessageBase
{
	public const string OnlineStatus = "online";
	public const string OfflineStatus = "offline";
	public StatusDetails? StatusDetails { get; set; }
	public override string ToString() => Newtonsoft.Json.JsonConvert.SerializeObject(this);
}
