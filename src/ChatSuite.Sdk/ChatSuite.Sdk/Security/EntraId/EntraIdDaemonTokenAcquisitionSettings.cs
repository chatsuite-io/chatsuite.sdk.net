using System.Globalization;

namespace ChatSuite.Sdk.Security.EntraId;

public sealed record EntraIdDaemonTokenAcquisitionSettings : EntraIdTokenSettings
{
	public string Instance { get; set; } = string.Empty;
	public string ClientId { get; set; } = string.Empty;
	public string Authority => string.Format(CultureInfo.InvariantCulture, Instance, Tenant);
}