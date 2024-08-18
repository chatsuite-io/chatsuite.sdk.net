using ChatSuite.Sdk.Security.Encryption;

namespace ChatSuite.Sdk.Connection.Events;

internal interface IEncryptionKeyTracker
{
	CipherKeys? this[string key] { get; set; }
}
