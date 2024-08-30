namespace ChatSuite.Sdk.Core.Message;

public record Metadata
{
    public string? ConnectionId { get; set; }
    public string? SpaceId { get; set; }
    public string? Suite { get; set; }
    public string? ClientId { get; set; }
    public string? ResponseToMessageId { get; set; }
	public string? ForwardOfMessageId { get; set; }
	public bool IsMessageEncrypted { get; set; } = false;
}
