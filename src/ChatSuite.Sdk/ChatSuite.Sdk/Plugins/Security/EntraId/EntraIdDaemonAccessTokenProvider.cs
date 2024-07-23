using Microsoft.Extensions.Options;

namespace ChatSuite.Sdk.Plugins.Security.EntraId;

internal class EntraIdDaemonAccessTokenProvider(
	IOptions<EntraIdDaemonTokenAcquisitionSettings> entraIDDaemonTokenAcquisitionSettings,
	IPlugin<EntraIdDaemonTokenAcquisitionSettings, string?> entraIDDaemonTokenAcquisitionPlugin) : IAccessTokenProvider
{
	public async Task<string?> GetAccessTokenAsync(CancellationToken cancellationToken)
	{
		entraIDDaemonTokenAcquisitionPlugin.Input = entraIDDaemonTokenAcquisitionSettings.Value;
		var response = await entraIDDaemonTokenAcquisitionPlugin.RunAsync(cancellationToken);
		return response?.Result;
	}
}
