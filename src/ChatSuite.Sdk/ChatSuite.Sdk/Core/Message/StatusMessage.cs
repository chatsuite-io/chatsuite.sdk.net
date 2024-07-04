namespace ChatSuite.Sdk.Core.Message;

public record StatusMessage : MessageBase
{
	public StatusDetails? StatusDetails { get; set; }
	public override string ToString() => Newtonsoft.Json.JsonConvert.SerializeObject(this);
}
