namespace ChatSuite.Sdk.Core;

public enum TargetEvent
{
	None,
	OnUserConnected,
	OnUserDisconnected,
	MessageDeliveredToUser,
	MessageDeliveredToGroup,
	UserStatusReported,
	AcquireEncryptionPublicKey
}
