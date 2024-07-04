namespace ChatSuite.Sdk.Plugin.Security;

internal class EntraIDTokenAcquisitionPluginValidator : AbstractValidator<EntraIDTokenAcquisitionPlugin>
{
	public EntraIDTokenAcquisitionPluginValidator()
	{
		RuleFor(plugin => plugin.Input).NotEmpty();
		When(plugin => plugin.Input is not null, () =>
		{
			RuleFor(plugin => plugin.Input!.Authority).NotEmpty();
			RuleFor(plugin => plugin.Input!.ClientId).NotEmpty();
			RuleFor(plugin => plugin.Input!.ClientSecret).NotEmpty();
			RuleFor(plugin => plugin.Input!.Tenant).NotEmpty();
		});
	}
}