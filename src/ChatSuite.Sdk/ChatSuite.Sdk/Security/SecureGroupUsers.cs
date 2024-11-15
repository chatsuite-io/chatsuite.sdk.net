namespace ChatSuite.Sdk.Security;

public record SecureGroupUsers
{
	public string? GroupName { get; set; }
	public List<string>? Users { get; set; }
}