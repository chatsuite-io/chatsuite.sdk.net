using ChatSuite.Sdk.Core.Security;
using System.Globalization;

namespace ChatSuite.Sdk.Plugin.Security;

public sealed record EntraIdTokenAcquisitionSettings : EntraIdTokenSettings
{
	public string Instance { get; set; } = string.Empty;
	public string ClientId { get; set; } = string.Empty;
	public string Authority => string.Format(CultureInfo.InvariantCulture, Instance, Tenant);
}