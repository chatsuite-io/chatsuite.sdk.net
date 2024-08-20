using ChatSuite.Sdk.Connection.Events;
using ChatSuite.Sdk.Core;
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
	const string RegistryKey1 = "systemuserid1";
	const string RegistryKey2 = "systemuserid2";

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
		var registry = _fixture.GetService<IEncryptionKeyRegistry>(testOutputHelper)!;
		registry[RegistryKey1] = cipherKeys;
		Assert.Equal(cipherKeys, registry[RegistryKey1]);
	}

	[Fact, TestOrder(25)]
	public void TestEncryptionKeyUpdateRegistry()
	{
		var cipherKeys = ((CipherKeys)_fixture.Data[PubPrivateDictionaryKey]) with { PublicKey = "pubkey1" };
		var registry = _fixture.GetService<IEncryptionKeyRegistry>(testOutputHelper)!;
		registry[RegistryKey1] = cipherKeys;
		Assert.Equal(cipherKeys, registry[RegistryKey1]);
		Assert.Equal(1, registry.Count);
	}

	[Fact, TestOrder(26)]
	public void TestEncryptionKeyAddToRegistryAgain()
	{
		var cipherKeys = (CipherKeys)_fixture.Data[PubPrivateDictionaryKey] with { PublicKey = "pubkey2" };
		var registry = _fixture.GetService<IEncryptionKeyRegistry>(testOutputHelper)!;
		registry[RegistryKey2] = cipherKeys;
		Assert.Equal(2, registry.Count);
		Assert.NotEqual(registry[RegistryKey2], registry[RegistryKey1]);
	}
}
