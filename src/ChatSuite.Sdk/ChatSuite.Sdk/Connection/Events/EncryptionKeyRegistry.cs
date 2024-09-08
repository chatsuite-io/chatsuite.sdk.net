namespace ChatSuite.Sdk.Connection.Events;

internal class EncryptionKeyRegistry : BreezeDBRegistry<CipherKeysTracker>
{
	protected override string GetDatabaseName() => "CipherKeysRegistry";
	protected override string GetCollectionName() => "CipherKeysCollection";
}
