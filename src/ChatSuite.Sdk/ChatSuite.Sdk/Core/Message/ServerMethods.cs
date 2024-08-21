namespace ChatSuite.Sdk.Core.Message;

public enum ServerMethods
{
	negotiate,
	OnConnected,
	OnDisconnected,
	SendMessageToGroup,
	SendMessageToUserInGroup,
	JoinUserToGroup,
	LeaveUserFromGroup,
	SendStatusToGroup,
	ReportUserStatusToUserInGroup,
	ReportUserStatusToGroup,
	GetOfflineUsers,
	AcquireEncryptionPublicKey,
	ShareEncryptionPublicKey
}