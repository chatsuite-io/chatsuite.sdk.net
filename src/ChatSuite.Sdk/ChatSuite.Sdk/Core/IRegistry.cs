namespace ChatSuite.Sdk.Core;

public interface IRegistry<T>
{
	T? this[string key] { get; set; }
	ulong Count { get; }
	void Recycle();
}
