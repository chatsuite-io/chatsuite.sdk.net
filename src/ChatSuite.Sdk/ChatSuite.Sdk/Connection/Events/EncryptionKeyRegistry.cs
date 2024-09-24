namespace ChatSuite.Sdk.Connection.Events;

internal class EncryptionKeyRegistry : BreezeDBRegistry<CipherKeysTracker>
{
	protected override string GetCollectionName() => "CipherKeysCollection";
}
