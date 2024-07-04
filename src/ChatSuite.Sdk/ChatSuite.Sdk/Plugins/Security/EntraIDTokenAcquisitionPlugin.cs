using Microsoft.Identity.Client;

namespace ChatSuite.Sdk.Plugin.Security;

public class EntraIDTokenAcquisitionPlugin : Plugin<EntraIdTokenAcquisitionSettings, string?>, IInputValidator
{
	public List<string> RuleSets => [];

	public ValidationResult Validate() => new EntraIDTokenAcquisitionPluginValidator().Validate(this);

	protected override async Task ExecuteAsync(Response<string?> response, CancellationToken cancellationToken)
	{
		response.Status = Status.Success;
		var app = ConfidentialClientApplicationBuilder.Create(Input!.ClientId)
			.WithClientSecret(Input!.ClientSecret)
			.WithAuthority(new Uri(Input!.Authority))
			.Build();
		var scopes = new string[] { Input!.Scope };
		try
		{
			var result = await app.AcquireTokenForClient(scopes).ExecuteAsync(cancellationToken);
			response.Result = result.AccessToken;
		}
		catch (MsalServiceException mex) when (mex.Message.Contains("AADSTS70011"))
		{
			response.Status = Status.Error;
			response.Errors = [new Response.Error{Message = mex.Message}];
		}
		catch (Exception ex)
		{
			response.Status = Status.Fail;
			response.Errors = [new Response.Error { Message = ex.Message }];
		}
	}
}
