namespace ChatSuite.Sdk.Extensions;

public static class StringExtensions
{
	public static string ToBase64(this string plainString) => Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(plainString));
	public static string FromBase64(this string base64String) => System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(base64String));
	public static PartyStatus GetPartyStatus(this string status) => (PartyStatus)Enum.Parse(typeof(PartyStatus), status);
}