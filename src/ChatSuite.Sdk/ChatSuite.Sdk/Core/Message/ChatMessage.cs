namespace ChatSuite.Sdk.Core.Message;

public record ChatMessage : MessageBase
{
    public IEnumerable<object>? Body { get; set; }
	public override string ToString() => Newtonsoft.Json.JsonConvert.SerializeObject(this);
}
