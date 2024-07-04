namespace ChatSuite.Core.Sdk.Plugins.SystemUser;

internal class SystemUserIdValidator : AbstractValidator<string>
{
	public SystemUserIdValidator() => RuleFor(userId => userId).NotEmpty().MaximumLength(256);
}