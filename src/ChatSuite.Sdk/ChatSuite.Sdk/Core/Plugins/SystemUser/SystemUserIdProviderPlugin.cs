namespace ChatSuite.Core.Sdk.Plugins.SystemUser;

internal class SystemUserIdProviderPlugin(ILoggerProvider loggerProvider, IValidator<MessageBase> messageValidator) : Plugin<MessageBase, string>(loggerProvider), IInputValidator
{
	internal const char Delimiter = '|';

	public SystemUserIdProviderPlugin(IValidator<MessageBase> messageValidator) : this(null!, messageValidator) { }

	public List<string> RuleSets => [];

	public ValidationResult Validate() => messageValidator.Validate(Input!);

	protected override Task ExecuteAsync(Response<string> response, CancellationToken cancellationToken)
	{
		response.Result = $"{Input!.Metadata!.SpaceId}{Delimiter}{Input!.Metadata!.Suite}{Delimiter}{Input!.User}".ToBase64();
		return Task.CompletedTask;
	}
}