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
			var userConnectedEvent = new UserConnected(testOutputHelper);
			var userDisconnectedEvent = new UserDisconnected(testOutputHelper);
			_client!.Closed += ex =>
			{
				testOutputHelper.WriteLine($"Exception: {ex?.Message}");
				return Task.CompletedTask;
			};
			_client!
				.RegisterEvent(userConnectedEvent)
				.RegisterEvent(userDisconnectedEvent);
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

	private class UserConnected(ITestOutputHelper testOutputHelper) : IEvent
	{
		public string? Target => TargetEvent.OnUserConnected.ToString();
		public bool Connected { get; private set; }

		public event Action<object>? OnResultReady;

		public Task Handle(object argument)
		{
			ArgumentNullException.ThrowIfNullOrEmpty(Target, nameof(Target));
			testOutputHelper.WriteLine("@argument", argument);
			Connected = true;
			OnResultReady?.Invoke(Connected);
			return Task.CompletedTask;
		}
	}

	private class UserDisconnected(ITestOutputHelper testOutputHelper) : IEvent
	{
		public string? Target => TargetEvent.OnUserDisconnected.ToString();
		public bool Disconnected { get; private set; }

		public event Action<object>? OnResultReady;

		public Task Handle(object argument)
		{
			ArgumentNullException.ThrowIfNullOrEmpty(Target, nameof(Target));
			testOutputHelper.WriteLine("@argument", argument);
			Disconnected = true;
			OnResultReady?.Invoke(Disconnected);
			return Task.CompletedTask;
		}
	}
}
