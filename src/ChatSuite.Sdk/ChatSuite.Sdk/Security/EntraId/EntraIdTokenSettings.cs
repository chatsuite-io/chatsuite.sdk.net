namespace ChatSuite.Sdk.Security.EntraId;

public abstract record EntraIdTokenSettings
{
	public string Tenant { get; set; } = string.Empty;
	public string ClientSecret { get; set; } = string.Empty;
	public string Scope { get; set; } = string.Empty;
}
