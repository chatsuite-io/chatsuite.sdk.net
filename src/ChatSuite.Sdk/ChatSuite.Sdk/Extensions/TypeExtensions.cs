using Newtonsoft.Json;

namespace ChatSuite.Sdk.Extensions;

public static class TypeExtensions
{
	public static T? Clone<T>(this T source)
	{
		if (source is null)
		{
			return default;
		}
		var deserializeSettings = new JsonSerializerSettings { ObjectCreationHandling = ObjectCreationHandling.Replace };
		return JsonConvert.DeserializeObject<T>(JsonConvert.SerializeObject(source), deserializeSettings);
	}

	public static string? Serialize<T>(this T obj) => JsonConvert.SerializeObject(obj);
	public static T? Deserializ<T>(this string serializedObject) => JsonConvert.DeserializeObject<T>(serializedObject);
}
