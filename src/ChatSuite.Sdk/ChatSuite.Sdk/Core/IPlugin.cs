namespace ChatSuite.Sdk.Core;

public interface IPlugin<in TInput, TResult>
{
	TInput? Input { set; }
	Task<Response<TResult>> RunAsync(CancellationToken cancellationToken);
}
