using ChatSuite.Sdk.Connection.Events;
using ChatSuite.Sdk.Core;
using ChatSuite.Sdk.Core.Message;
using ChatSuite.Sdk.Extensions;
using ChatSuite.Sdk.Security.Encryption;
using ChatSuite.Sdk.Security.EntraId;
using Xunit.Microsoft.DependencyInjection.Attributes;

namespace ChatSuite.Sdk.IntegrationTests;

[TestCaseOrderer("Xunit.Microsoft.DependencyInjection.TestsOrder.TestPriorityOrderer", "Xunit.Microsoft.DependencyInjection")]
public class SecurityTests(ITestOutputHelper testOutputHelper, ReliableConnectionFixture fixture) : TestBed<ReliableConnectionFixture>(testOutputHelper, fixture)
{
	private const string TextToEncrypt = "Muskoka, once discovered, never forgotten!";
	private const string PubPrivateDictionaryKey = "key";
	private const string SystemUserId1 = "systemuserid1";
	private const string SystemUserId2 = "systemuserid2";

	[Fact, TestOrder(1)]
	public async Task AcquireDaemonJwtTokenAsync()
	{
		var settings = _fixture.GetService<IOptions<EntraIdDaemonTokenAcquisitionSettings>>(_testOutputHelper)!;
		var plugin = _fixture.GetService<IPlugin<EntraIdDaemonTokenAcquisitionSettings, string?>>(_testOutputHelper)!;
		plugin.Input = settings.Value;
		var token = await plugin.RunAsync(CancellationToken.None);
		Assert.NotNull(token?.Result);
		Assert.False(string.IsNullOrEmpty(token?.Result));
	}

	[Fact, TestOrder(10)]
	public async Task GetEncryptionKeysAsync()
	{
		var encryptionKeyGeneratorPlugin = _fixture.GetService<IPlugin<int, CipherKeys>>(_testOutputHelper)!;
		var keys = await encryptionKeyGeneratorPlugin.RunAsync(CancellationToken.None);
		Assert.NotNull(keys);
		Assert.True(keys.Result?.PublicKey.Length > 0);
		Assert.True(keys.Result?.PrivateKey.Length > 0);
		Assert.False(keys.Result?.PublicKey.Equals(keys.Result.PrivateKey));
		_fixture.Data[PubPrivateDictionaryKey] = keys.Result!;
	}

	[Fact, TestOrder(20)]
	public async Task EncryptAsync()
	{
		var cipherKeys = (CipherKeys)_fixture.Data[PubPrivateDictionaryKey];
		var encryptionPlugin = _fixture.GetKeyedService<IPlugin<(string encryptionPublicKey, string stringToEncrypt), string>>(Extensions.DependencyInjection.DependencyInjectionExtensions.EncryptionPluginKey, _testOutputHelper)!;
		encryptionPlugin.Input = (cipherKeys.PublicKey, TextToEncrypt);
		var encryptionResult = await encryptionPlugin.RunAsync(CancellationToken.None);
		Assert.True(encryptionResult.DenotesSuccess());
		Assert.NotEmpty(encryptionResult.Result!);
		_fixture.Data[nameof(TextToEncrypt)] = encryptionResult.Result!;
	}

	[Fact, TestOrder(30)]
	public async Task DecryptAsync()
	{
		var cipherKeys = (CipherKeys)_fixture.Data[PubPrivateDictionaryKey];
		var decryptionPlugin = _fixture.GetKeyedService<IPlugin<(string encryptionPrivateKey, string encryptedString), string>>(Extensions.DependencyInjection.DependencyInjectionExtensions.DecryptionPluginKey, _testOutputHelper)!;
		decryptionPlugin.Input = (cipherKeys.PrivateKey, (string)_fixture.Data[nameof(TextToEncrypt)]);
		var decryptionResult = await decryptionPlugin.RunAsync(CancellationToken.None);
		Assert.True(decryptionResult.DenotesSuccess());
		Assert.NotEmpty(decryptionResult.Result!);
		Assert.True(decryptionResult.Result == TextToEncrypt);
	}

	[Fact, TestOrder(20)]
	public void TestEncryptionKeyAddToRegistry()
	{
		var cipherKeys = (CipherKeys)_fixture.Data[PubPrivateDictionaryKey];
		var registry = _fixture.GetService<IRegistry<CipherKeysTracker>>(_testOutputHelper)!;
		registry.Recycle();
		registry[SystemUserId1] = CipherKeysTracker.Create(SystemUserId1, cipherKeys);
		Assert.Equal(cipherKeys.PublicKey, registry[SystemUserId1]!.PublicKey);
		Assert.Equal(cipherKeys.PrivateKey, registry[SystemUserId1]!.PrivateKey);
	}

	[Fact, TestOrder(25)]
	public void TestEncryptionKeyUpdateRegistry()
	{
		var cipherKeys = ((CipherKeys)_fixture.Data[PubPrivateDictionaryKey]) with { PublicKey = "pubkey1" };
		var registry = _fixture.GetService<IRegistry<CipherKeysTracker>>(_testOutputHelper)!;
		registry.Recycle();
		registry[SystemUserId1] = CipherKeysTracker.Create(SystemUserId1, cipherKeys);
		Assert.Equal(cipherKeys.PublicKey, registry[SystemUserId1]!.PublicKey);
		Assert.Equal(cipherKeys.PrivateKey, registry[SystemUserId1]!.PrivateKey);
		Assert.Equal(1ul, registry.Count);
	}

	[Fact, TestOrder(26)]
	public void TestEncryptionKeyAddToRegistryAgain()
	{
		var cipherKeys = (CipherKeys)_fixture.Data[PubPrivateDictionaryKey] with { PublicKey = "pubkey2" };
		var registry = _fixture.GetService<IRegistry<CipherKeysTracker>>(_testOutputHelper)!;
		registry[SystemUserId2] = CipherKeysTracker.Create(SystemUserId1, cipherKeys);
		Assert.Equal(cipherKeys.PublicKey, registry[SystemUserId2]!.PublicKey);
		Assert.Equal(cipherKeys.PrivateKey, registry[SystemUserId2]!.PrivateKey);
		Assert.Equal(2ul, registry.Count);
		Assert.NotEqual(registry[SystemUserId2], registry[SystemUserId1]);
	}
#if DEBUG
	[Fact, TestOrder(30)]
	public async Task RequestPublicKeyAsync()
	{
		const string Space = "space102";
		const string Suite = "connectiontest102";
		await using var client1 = await _fixture.GetClientAsync(_testOutputHelper, sustainInFixture: false, new ConnectionParameters
		{
			Id = Guid.NewGuid().ToString(),
			User = "userA",
			Metadata = new()
			{
				ClientId = Guid.NewGuid().ToString(),
				Suite = Suite,
				SpaceId = Space
			}
		});
		var user2connection = new ConnectionParameters
		{
			Id = Guid.NewGuid().ToString(),
			User = "userB",
			Metadata = new()
			{
				ClientId = Guid.NewGuid().ToString(),
				Suite = Suite,
				SpaceId = Space
			}
		};
		await using var client2 = await _fixture.GetClientAsync(_testOutputHelper, sustainInFixture: false, user2connection);
		await client1!.ConnectAsync(CancellationToken.None);
		await client2!.ConnectAsync(CancellationToken.None);
		var client2PublicKey = await client1!.RequestPublicKeyAsync(user2connection.User, CancellationToken.None);
		Assert.NotNull(client2PublicKey);
		Assert.NotEmpty(client2PublicKey);
	}

	[Fact, TestOrder(30)]
	public async Task GetOnlineStatusAsync()
	{
		const string Space = "space102";
		const string Suite = "connectiontest102";
		await using var client1 = await _fixture.GetClientAsync(_testOutputHelper, sustainInFixture: false, new ConnectionParameters
		{
			Id = Guid.NewGuid().ToString(),
			User = "userAA",
			Metadata = new()
			{
				ClientId = Guid.NewGuid().ToString(),
				Suite = Suite,
				SpaceId = Space
			}
		});
		var user2connection = new ConnectionParameters
		{
			Id = Guid.NewGuid().ToString(),
			User = "userBB",
			Metadata = new()
			{
				ClientId = Guid.NewGuid().ToString(),
				Suite = Suite,
				SpaceId = Space
			}
		};
		await using var client2 = await _fixture.GetClientAsync(_testOutputHelper, sustainInFixture: false, user2connection);
		await client1!.ConnectAsync(CancellationToken.None);
		await client2!.ConnectAsync(CancellationToken.None);
		var isOnline = await client1!.IsUserOnlineAsync(user2connection.User, CancellationToken.None);
		Assert.True(isOnline);
	}
#endif

	[Fact, TestOrder(40)]
	public async Task SendEncryptedMessageAsync()
	{
		const string OtherUser = "userBBB";
		const string Space = "space102";
		const string Suite = "connectiontest102";
		await using var client1 = await _fixture.GetClientAsync(_testOutputHelper, sustainInFixture: false, new ConnectionParameters
		{
			Id = Guid.NewGuid().ToString(),
			User = "userAAA",
			Metadata = new()
			{
				ClientId = Guid.NewGuid().ToString(),
				Suite = Suite,
				SpaceId = Space
			}
		});
		var user2connection = new ConnectionParameters
		{
			Id = Guid.NewGuid().ToString(),
			User = OtherUser,
			Metadata = new()
			{
				ClientId = Guid.NewGuid().ToString(),
				Suite = Suite,
				SpaceId = Space
			}
		};
		await using var client2 = await _fixture.GetClientAsync(_testOutputHelper, sustainInFixture: false, user2connection);
		await client1!.ConnectAsync(CancellationToken.None);
		await client2!.ConnectAsync(CancellationToken.None);
		const string MessageToSend = "This is a secure test message.";
		var messageReceived = false;
		var cancellationTokenSource = new CancellationTokenSource();
		cancellationTokenSource.CancelAfter(15000);
		client2!.GetMessageDeliveredEvent().OnResultReady += async o =>
		{
			var result = await o;
			messageReceived = result is not null;
		};
		var sent = await client1.SendEncryptedMessageToUserAsync(OtherUser, new ChatMessage { Id = Guid.NewGuid().ToString(), Body = [MessageToSend] }, CancellationToken.None);
		Assert.True(sent);
		while (!messageReceived && !cancellationTokenSource.IsCancellationRequested){ }
		Assert.True(messageReceived);
	}

	[Fact, TestOrder(50)]
	public void SaveCipherKeys()
	{
		const string Key = "mykey";
		using var registry = _fixture.GetService<IRegistry<CipherKeysTracker>>(_testOutputHelper)!;
		registry.Recycle();
		registry[Key] = new()
		{
			OtherPartySystemUserId = Guid.NewGuid().ToString(),
			PrivateKey = "privateKey",
			PublicKey = "publickey"
		};
		Assert.Equal(1ul, registry.Count);
		var record = registry[Key];
		Assert.NotNull(record);
	}
}
