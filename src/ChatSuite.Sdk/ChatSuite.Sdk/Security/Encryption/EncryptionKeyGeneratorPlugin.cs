namespace ChatSuite.Sdk.Security.Encryption;

internal class EncryptionKeyGeneratorPlugin : Plugin<int, CipherKeys>
{
	public const int DwKeySize = 2048;
	protected override Task ExecuteAsync(Response<CipherKeys> response, CancellationToken cancellationToken)
	{
		response.Status = Status.Success;
		try
		{
			Input = DwKeySize;
			using var rsa = new RSACryptoServiceProvider(Input);
			response.Result = new(Convert.ToBase64String(rsa.ExportCspBlob(false)), Convert.ToBase64String(rsa.ExportCspBlob(true)));
		}
		catch(CryptographicException ex)
		{
			response.Status = Status.Fail;
			response.Errors =
			[
				new()
				{
					Exception = ex,
					Message = ex.Message,
					ErrorCode = ErrorCode.Generic
				}
			];
		}
		return Task.CompletedTask;
	}
}
