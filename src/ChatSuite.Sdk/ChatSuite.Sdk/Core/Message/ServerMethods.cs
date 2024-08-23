namespace ChatSuite.Sdk.Core.Message;

public enum ServerMethods
{
	negotiate,
	OnConnected,
	OnDisconnected,
	SendMessageToGroup,
	SendMessageToUserInGroup,
	SendEncryptedMessageToUserInGroup,
	JoinUserToGroup,
	LeaveUserFromGroup,
	SendStatusToGroup,
	ReportUserStatusToUserInGroup,
	ReportUserStatusToGroup,
	GetOfflineUsers,
	AcquireEncryptionPublicKey,
	ShareEncryptionPublicKey
}