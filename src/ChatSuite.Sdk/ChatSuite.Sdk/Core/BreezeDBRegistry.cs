using DBreeze;
using System.Diagnostics.CodeAnalysis;

namespace ChatSuite.Sdk.Core;

public abstract class BreezeDBRegistry<TVal> : IRegistry<TVal>
	where TVal : class
{
	private static readonly DBreezeEngine? _engine = new("CipherKeysRegistry");

	//protected BreezeDBRegistry() => _engine = new DBreezeEngine(GetDatabaseName());

	public TVal? this[string key]
	{
		get => Get(key);
		set => Save(key, value ?? throw new ArgumentNullException("The value is null."));
	}

	public ulong Count
	{
		get
		{
			using var transaction = _engine?.GetTransaction();
			return transaction?.Count(GetCollectionName()) ?? 0;
		}
	}

	public void Recycle()
	{
		using var transaction = _engine?.GetTransaction();
		transaction?.RemoveAllKeys(GetCollectionName(), false);
		transaction?.Commit();
	}

	protected bool Save(string key, [DisallowNull] TVal model)
	{
		using var transaction = _engine?.GetTransaction();
		try
		{
			transaction?.Insert(GetCollectionName(), key, Newtonsoft.Json.JsonConvert.SerializeObject(model));
			transaction?.Commit();
			return transaction is not null;
		}
		catch
		{
			transaction?.Rollback();
		}
		return false;
	}

	protected TVal? Get(string key)
	{
		using var transaction = _engine?.GetTransaction();
		var value = transaction?
			.SelectForward<string, string>(GetCollectionName())
			.Select(r => r)
			.Where(r => r.Key == key)
			.FirstOrDefault();
		return value is not null ? Newtonsoft.Json.JsonConvert.DeserializeObject<TVal>(value.Value) : default;
	}

	protected abstract string GetCollectionName();
}
