namespace ChatSuite.Sdk.Core;

public interface IInputValidator
{
	List<string> RuleSets { get; }
	ValidationResult Validate();
}
