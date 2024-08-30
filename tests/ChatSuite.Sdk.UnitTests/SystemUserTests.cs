using ChatSuite.Sdk.Extensions;
using ChatSuite.Sdk.UnitTests.Fixtures;

namespace ChatSuite.Sdk.UnitTests;

public class SystemUserTests(ITestOutputHelper testOutputHelper, TestFixture fixture) : TestBed<TestFixture>(testOutputHelper, fixture)
{
	private readonly IPlugin<MessageBase, string> _systemUserIdProvider = fixture.GetService<IPlugin<MessageBase, string>>(testOutputHelper)!;
	private readonly IPlugin<string, (string? spaceId, string? suite, string? reciever)> _systemUserIdDecomposer = fixture.GetService<IPlugin<string, (string? spaceId, string? suite, string? reciever)>>(testOutputHelper)!;

	[Fact]
	public async Task SystemUserIsNotEmptyAsync()
	{
		_systemUserIdProvider.Input = new ChatMessage
		{
			User = "myName",
			Metadata = new()
			{
				Suite = "mySuite",
				SpaceId = "myWorkspace"
			}
		};
		var response = await _systemUserIdProvider.RunAsync(CancellationToken.None);
		Assert.NotNull(response.Result);
		Assert.NotEmpty(response.Result);
	}

	[Fact]
	public async Task SystemUserIdDecompositionNotEmptyAsync()
	{
		_systemUserIdDecomposer.Input = "Segment1|Segment2|Segment3".ToBase64();
		var response = await _systemUserIdDecomposer.RunAsync(CancellationToken.None);
		Assert.NotNull(response.Result.spaceId);
		Assert.NotNull(response.Result.suite);
		Assert.NotNull(response.Result.reciever);
	}

	[Fact]
	public void ConvertToFromBase64()
	{
		const string PlainText = "This is a test plain text";
		var base64 = PlainText.ToBase64();
		Assert.NotEmpty(base64);
		var plainText = base64.FromBase64();
		Assert.NotEmpty(plainText);
		Assert.Equal(PlainText, plainText);
	}
}