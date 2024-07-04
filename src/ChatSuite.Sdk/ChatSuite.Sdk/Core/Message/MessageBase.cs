namespace ChatSuite.Sdk.Core.Message;

public abstract record MessageBase
{
    public string? Id { get; set; }
    public string? User { get; set; }
    public Metadata? Metadata { get; set; }
}
