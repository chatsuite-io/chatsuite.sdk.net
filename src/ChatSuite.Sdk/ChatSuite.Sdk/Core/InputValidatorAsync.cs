namespace ChatSuite.Sdk.Core;

public interface InputValidatorAsync
{
	Task<ValidationResult> ValidateAsync(CancellationToken cancellationToken);
}