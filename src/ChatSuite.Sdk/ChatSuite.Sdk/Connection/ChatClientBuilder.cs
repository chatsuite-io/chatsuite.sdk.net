using ChatSuite.Sdk.Plugins.Security;

namespace ChatSuite.Sdk.Connection;

internal class ChatClientBuilder(
	IPlugin<MessageBase, string> systemUserIdProvider,
	IAccessTokenProvider accessTokenProvider,
	IValidator<ConnectionParameters> connectionParametesrValidator) : Plugin<ConnectionParameters, IClient?>, IInputValidator
{
	public List<string> RuleSets => [];

	public ValidationResult Validate() => connectionParametesrValidator.Validate(Input!);

	protected override async Task ExecuteAsync(Response<IClient?> response, CancellationToken cancellationToken)
	{
		Client? client = null;
		systemUserIdProvider.Input = Input;
		var systemUserId = await systemUserIdProvider.RunAsync(cancellationToken);
		if (systemUserId.DenotesSuccess())
		{
			try
			{
				client = new Client
				{
					ConnectionParameters = Input!,
					AccessTokenProvider = () => accessTokenProvider.GetAccessTokenAsync(cancellationToken),
					SystemUserId = systemUserId.Result!
				};
				client.Build();
			}
			catch (Exception ex)
			{
				response.Errors = [new Response.Error() { Exception = ex, Message = ex.Message }];
				response.Status = Status.Fail;
			}
		}
		else
		{
			response.Errors = systemUserId.Errors;
			response.ValidationErrors = systemUserId.ValidationErrors;
			response.Status = systemUserId.Status;
		}
		response.Result = client;
	}
}

