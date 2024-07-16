using ChatSuite.Sdk.Plugin.Security;
using Microsoft.Extensions.Options;

namespace ChatSuite.Sdk.Plugins.Security;

internal class EntraIDDaemonAccessTokenProvider(
	IOptions<EntraIDDaemonTokenAcquisitionSettings> entraIDDaemonTokenAcquisitionSettings,
	IPlugin<EntraIDDaemonTokenAcquisitionSettings, string?> entraIDDaemonTokenAcquisitionPlugin) : IAccessTokenProvider
{
	public async Task<string?> GetAccessTokenAsync(CancellationToken cancellationToken)
	{
		entraIDDaemonTokenAcquisitionPlugin.Input = entraIDDaemonTokenAcquisitionSettings.Value;
		var response = await entraIDDaemonTokenAcquisitionPlugin.RunAsync(cancellationToken);
		return response?.Result;
	}
}
