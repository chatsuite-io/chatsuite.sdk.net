using ChatSuite.Sdk.Security.Encryption;

namespace ChatSuite.Sdk.Connection.Events;

public interface IEncryptionKeyRegistry
{
	CipherKeys? this[string key] { get; set; }
	int Count { get; }
}
