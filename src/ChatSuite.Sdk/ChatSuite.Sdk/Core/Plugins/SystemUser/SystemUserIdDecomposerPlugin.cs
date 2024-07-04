namespace ChatSuite.Core.Sdk.Plugins.SystemUser;

internal class SystemUserIdDecomposerPlugin(ILoggerProvider loggerProvider, [FromKeyedServices(nameof(SystemUserIdDecomposerPlugin))] IValidator<string> validator) : Plugin<string, (string? spaceId, string? suite, string? username)>(loggerProvider), IInputValidator
{
	public SystemUserIdDecomposerPlugin([FromKeyedServices(nameof(SystemUserIdDecomposerPlugin))] IValidator<string> validator) : this(null!, validator) { }

	public List<string> RuleSets => [];

	public ValidationResult Validate() => validator.Validate(Input!);

	protected override Task ExecuteAsync(Response<(string? spaceId, string? suite, string? username)> response, CancellationToken cancellationToken)
	{
		var splitData = Input!.FromBase64().Split(SystemUserIdProviderPlugin.Delimiter);
		response.Result = new(splitData[0], splitData[1], splitData[2]);
		return Task.CompletedTask;
	}
}
