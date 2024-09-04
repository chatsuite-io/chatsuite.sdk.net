using ChatSuite.Sdk.Security.Encryption;
using System.Collections.Concurrent;

namespace ChatSuite.Sdk.Connection.Events;

internal class EncryptionKeyRegistry : IRegistry<CipherKeys>
{
	private readonly ConcurrentDictionary<string, CipherKeys> _encryptionKeys = new();

	public CipherKeys? this[string key]
	{
		get
		{
			_encryptionKeys.TryGetValue(key, out var cipherKeys);
			return cipherKeys;
		}
		set
		{
			ArgumentNullException.ThrowIfNull(value, nameof(value));
			if (_encryptionKeys.TryGetValue(key, out var oldCipherKeys))
			{
				_encryptionKeys.TryUpdate(key, value, oldCipherKeys!);
			}
			else
			{
				_encryptionKeys.TryAdd(key, value);
			}
		}
	}

	public int Count => _encryptionKeys.Count;
}