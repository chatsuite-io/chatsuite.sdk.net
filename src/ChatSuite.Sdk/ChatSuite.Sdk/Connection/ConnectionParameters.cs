namespace ChatSuite.Sdk.Connection;

public record ConnectionParameters : MessageBase
{
	public string? SecretKey { internal get; set; }
	public string? Endpoint { internal get; set; }
}
