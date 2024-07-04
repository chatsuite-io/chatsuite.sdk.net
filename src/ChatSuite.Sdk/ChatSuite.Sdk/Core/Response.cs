namespace ChatSuite.Sdk.Core;

public record Response
{
	public Status Status { get; set; }	
	public IEnumerable<ValidationError>? ValidationErrors { get; set; }
	public IEnumerable<Error>? Errors { get; set; }

	public sealed record ValidationError
	{
		public string? InvalidProperty { get; internal set; }
		public string? ErrorMessage { get; internal set; }
	}

	public sealed record Error
	{
		public ErrorCode ErrorCode { get; set; } = ErrorCode.Generic;
		public string? Message { get; set; }
		public Exception? Exception { get; set; }
	}
}

public sealed record Response<TResult> : Response
{
	public TResult? Result { get; set; }
}

public enum ErrorCode : byte
{
	Generic,
	Infrastructure
}