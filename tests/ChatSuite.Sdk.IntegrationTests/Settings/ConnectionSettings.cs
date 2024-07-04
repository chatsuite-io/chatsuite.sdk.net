namespace ChatSuite.Sdk.IntegrationTests.Settings;

public record ConnectionSettings
{
	public string? SecretKey { get; set; }
	public string? Endpoint { get; set; }
}
