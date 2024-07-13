using ChatSuite.Sdk.Plugin.Security;

namespace ChatSuite.Sdk.Plugins.Security;

internal class EntraIDDaemonAccessTokenProvider(IPlugin<EntraIDDaemonTokenAcquisitionSettings, string?> entraIDDaemonTokenAcquisitionPlugin) : IAccessTokenProvider
{
	public EntraIDDaemonTokenAcquisitionSettings? EntraIDDaemonTokenAcquisitionSettings { private get; set; }

	public async Task<string?> GetAccessTokenAsync(CancellationToken cancellationToken)
	{
		ArgumentNullException.ThrowIfNull(EntraIDDaemonTokenAcquisitionSettings, nameof(EntraIDDaemonTokenAcquisitionSettings));
		entraIDDaemonTokenAcquisitionPlugin.Input = EntraIDDaemonTokenAcquisitionSettings;
		var response = await entraIDDaemonTokenAcquisitionPlugin.RunAsync(cancellationToken);
		return response?.Result;
	}
}
