namespace ChatSuite.Sdk.Connection.Events;

internal class SecureGroupUsersRegistry : BreezeDBRegistry<SecureGroupUsers>
{
	protected override string GetCollectionName() => $"{nameof(SecureGroupUsers)}Collection";
}
