namespace ChatSuite.Sdk.Connection;

internal class ConnectionParametersValidator : AbstractValidator<ConnectionParameters>
{
	public ConnectionParametersValidator(IValidator<MessageBase> messageBaseValidator)
	{
		RuleFor(connectionParameters => connectionParameters).SetValidator(messageBaseValidator);
		RuleFor(connectionParameters => connectionParameters.SecretKey).NotEmpty().MaximumLength(128).MinimumLength(8);
		RuleFor(connectionParameters => connectionParameters.Endpoint).NotEmpty().MaximumLength(256);
		RuleFor(connectionParameters => connectionParameters).Must(connectionParameters => Uri.TryCreate(connectionParameters.Endpoint, new UriCreationOptions(), out var _));
	}
}