using ChatSuite.Sdk.Core;
using ChatSuite.Sdk.Extensions.Client;
using ChatSuite.Sdk.Extensions.DependencyInjection;
using ChatSuite.Sdk.IntegrationTests.Settings;
using Microsoft.Extensions.Configuration;
using Xunit.Microsoft.DependencyInjection;

namespace ChatSuite.Sdk.IntegrationTests.Fixtures;

public class ReliableConnectionFixture : TestBedFixture
{
	private IClient? _client;

	public Dictionary<string, object> Data { get; } = [];

	public async Task<IClient?> GetClientAsync(ITestOutputHelper testOutputHelper, bool sustainInFixture = true, ConnectionParameters? connectionParameters = null, params IEvent[] events)
	{
		if (_client is null || !sustainInFixture)
		{
			var chatClientBuilder = GetService<IPlugin<ConnectionParameters, IClient?>>(testOutputHelper)!;
			var connectionSettings = GetService<IOptions<ConnectionSettings>>(testOutputHelper)!;
			var connection = connectionParameters ?? ConnectionParameters.Instantiate(
				"userA",
				"testSuite",
				"testSpaceId");
			connection.Endpoint = connectionSettings.Value.Endpoint;
			connection.SecretKey = connectionSettings.Value.SecretKey;
			_client = await chatClientBuilder.BuildAsync(connection, error => { });
			_client!.Closed += ex =>
			{
				testOutputHelper.WriteLine($"Exception: {ex?.Message}");
				return Task.CompletedTask;
			};
			foreach (var @event in events)
			{
				_client!.RegisterEvent(@event);
			}
		}
		return _client;
	}

	protected override void AddServices(IServiceCollection services, IConfiguration? configuration) => services
		.Configure<ConnectionSettings>(configuration!.GetSection(nameof(ConnectionSettings)))
		.AddChatSuiteClient()
		.AddEntraIdDaemonAccessTokenProvider(configuration)
		.AddEncryptionPlugins()
		.AddDecryptionPlugins()
		.AddEncryptionKeyRegistry();

	protected override ValueTask DisposeAsyncCore() => _client?.DisposeAsync() ?? new();

	protected override IEnumerable<TestAppSettings> GetTestAppSettings()
	{
		yield return new() { Filename = "appsettings.json", IsOptional = false };
	}

	protected override void AddUserSecrets(IConfigurationBuilder configurationBuilder) => configurationBuilder.AddUserSecrets<ReliableConnectionFixture>();
}
