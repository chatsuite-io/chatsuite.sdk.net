using ChatSuite.Sdk.Security.Encryption;
using System.Collections.Concurrent;

namespace ChatSuite.Sdk.Connection.Events;

internal class EncryptionKeyTracker : IEncryptionKeyTracker
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
			if (_encryptionKeys.TryGetValue(key, out _))
			{
				_encryptionKeys.Remove(key, out var _);
			}
			_encryptionKeys.TryAdd(key, value ?? throw new ArgumentNullException());
		}
	}
}