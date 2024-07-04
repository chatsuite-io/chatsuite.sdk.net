namespace ChatSuite.Sdk.Core;

public abstract class Plugin<TInput, TResult>(ILoggerProvider loggerProvider) : IPlugin<TInput, TResult>
{
	protected ILogger? _logger;

	public Plugin() : this(null!) { }

	public TInput? Input { get; set; }

	public async Task<Response<TResult>> RunAsync(CancellationToken cancellationToken)
	{
		_logger = loggerProvider?.CreateLogger(GetType().Name);
		var response = new Response<TResult> { Status = Status.Success };
		try
		{
			if (this is IInputValidator requestValidator)
			{
				var validationResults = ReportInvalidNullRequest() ?? requestValidator.Validate();
				if (!validationResults.IsValid)
				{
					PopulateValidationError(validationResults);
					return response;
				}
			}
			if (this is InputValidatorAsync requestValidatorAsync)
			{
				var validationResults = ReportInvalidNullRequest() ?? await requestValidatorAsync.ValidateAsync(cancellationToken);
				if (!validationResults.IsValid)
				{
					PopulateValidationError(validationResults);
					return response;
				}
			}
			await ExecuteAsync(response, cancellationToken);
		}
		catch (Exception ex)
		{
			response.Errors = [new Response.Error() { Exception = ex, Message = ex.Message }];
			FailureReport(response);
			_logger?.LogCritical(ex, $"Unhandled exception: {ex.Message}");
		}
		return response;

		void PopulateValidationError(ValidationResult validationResults)
		{
			response.Status = Status.Invalid;
			response.ValidationErrors = validationResults.Errors.Select(error => new Response<TResult>.ValidationError { ErrorMessage = error.ErrorMessage, InvalidProperty = error.PropertyName });
		}
	}
	protected abstract Task ExecuteAsync(Response<TResult> response, CancellationToken cancellationToken);
	protected virtual void FailureReport(Response<TResult> response) => response.Status = Status.Fail;

	private ValidationResult? ReportInvalidNullRequest() => Input is null
		? new()
		{
			Errors =
			[
				new()
				{
					PropertyName = nameof(Input),
					ErrorMessage = "Request must not be null.",
					Severity = Severity.Error
				}
			]
		}
		: null;
}
