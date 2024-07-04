namespace ChatSuite.Sdk.Extensions;

public static class ResponseExtensions
{
	public static bool DenotesSuccess(this Response response) => response.Status == Status.Success;
	public static bool DenotesFailures(this Response response) => response.Status == Status.Fail;
	public static bool DenotesInvalidity(this Response response) => response.Status == Status.Invalid;
	public static bool DenotesError(this Response response) => response.Status == Status.Error;
}
