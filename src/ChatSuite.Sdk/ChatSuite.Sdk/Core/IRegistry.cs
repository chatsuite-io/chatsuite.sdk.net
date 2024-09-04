using ChatSuite.Sdk.Security.Encryption;

namespace ChatSuite.Sdk.Core;

public interface IRegistry<T>
{
	T? this[string key] { get; set; }
	int Count { get; }
}
