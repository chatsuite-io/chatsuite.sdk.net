namespace ChatSuite.Core.Sdk.Plugins.SystemUser;

internal class MessageMetadataValidator : AbstractValidator<Metadata>
{
	public MessageMetadataValidator()
	{
		RuleFor(metadata => metadata.SpaceId).NotEmpty().MaximumLength(48);
		RuleFor(metadata => metadata.Suite).NotEmpty().MaximumLength(48);
	}
}