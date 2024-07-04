namespace ChatSuite.Core.Sdk.Plugins.SystemUser;

internal class SystemUserIdProviderPluginValidator : AbstractValidator<MessageBase>
{
	public SystemUserIdProviderPluginValidator(IValidator<Metadata> metadataValidator)
	{
		RuleFor(message => message).NotNull();
		RuleFor(message => message.User).NotEmpty().MaximumLength(126);
		RuleFor(message => message.Metadata).NotNull();
		RuleFor(message => message.Metadata!)
			.SetValidator(metadataValidator)
			.Unless(message => message.Metadata is null);
	}
}
