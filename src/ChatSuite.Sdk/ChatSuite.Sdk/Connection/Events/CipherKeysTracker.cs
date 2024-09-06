using ChatSuite.Sdk.Security.Encryption;
using System.Diagnostics.CodeAnalysis;

namespace ChatSuite.Sdk.Connection.Events;

public record CipherKeysTracker
{
	public string? OtherPartySystemUserId { get; set; }
	public string? PublicKey { get; set; }
	public string? PrivateKey { get; set; }
	public static CipherKeysTracker Create(string otherPartySystemUserId, [DisallowNull]CipherKeys cipherKeys) => new()
	{
		OtherPartySystemUserId = otherPartySystemUserId,
		PublicKey = cipherKeys.PublicKey,
		PrivateKey = cipherKeys.PrivateKey
	};
}