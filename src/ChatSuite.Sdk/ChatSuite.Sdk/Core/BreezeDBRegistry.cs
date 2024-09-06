using DBreeze;
using System.Diagnostics.CodeAnalysis;

namespace ChatSuite.Sdk.Core;

public abstract class BreezeDBRegistry<TVal> : IRegistry<TVal>
	where TVal : class
{
	private DBreezeEngine? _engine;
	private bool disposedValue;

	protected BreezeDBRegistry() => _engine = new DBreezeEngine(GetDatabaseName());

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

	public void Dispose()
	{
		// Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
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
	protected abstract string GetDatabaseName();

	protected virtual void Dispose(bool disposing)
	{
		if (!disposedValue)
		{
			if (disposing)
			{
				_engine?.Dispose();
				_engine = null;
			}

			// TODO: free unmanaged resources (unmanaged objects) and override finalizer
			// TODO: set large fields to null
			disposedValue = true;
		}
	}

	// // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
	// ~Registry()
	// {
	//     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
	//     Dispose(disposing: false);
	// }
}
