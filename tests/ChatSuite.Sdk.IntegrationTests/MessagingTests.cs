using ChatSuite.Sdk.Core;
using ChatSuite.Sdk.Core.Message;
using ChatSuite.Sdk.IntegrationTests.Framework;

namespace ChatSuite.Sdk.IntegrationTests;

public class MessagingTests(ITestOutputHelper testOutputHelper, ReliableConnectionFixture fixture) : TestBed<ReliableConnectionFixture>(testOutputHelper, fixture)
{
	[Fact]
	public async Task SendMessageFromUserToUserAsync()
	{
		const string Space = "space101";
		const string Suite = "connectiontest101";
		await using var client1 = await _fixture.GetClientAsync(_testOutputHelper, sustainInFixture: false, new ConnectionParameters
		{
			Id = Guid.NewGuid().ToString(),
			User = "userA",
			Metadata = new()
			{
				ClientId = Guid.NewGuid().ToString(),
				Suite = Suite,
				SpaceId = Space
			}
		}, new UserMessageRecieved(_testOutputHelper));
		var user2connection = new ConnectionParameters
		{
			Id = Guid.NewGuid().ToString(),
			User = "userB",
			Metadata = new()
			{
				ClientId = Guid.NewGuid().ToString(),
				Suite = Suite,
				SpaceId = Space
			}
		};
		var @event = new UserMessageRecieved(_testOutputHelper);
		await using var client2 = await _fixture.GetClientAsync(_testOutputHelper,sustainInFixture: false, user2connection, @event);
		await client1!.ConnectAsync(CancellationToken.None);
		await client2!.ConnectAsync(CancellationToken.None);
		var sent = await client1.SendMessageToUserAsync("userB", Newtonsoft.Json.JsonConvert.SerializeObject(new ChatMessage { Id = Guid.NewGuid().ToString(), Body = ["This is a test"] }), CancellationToken.None);
		Assert.True(sent);
		var recieved = await @event.WaitAsync(() => @event.Recieved && sent, CancellationToken.None);
		Assert.True(recieved);
	}

	//Note: this test may exhibit some timing issues and therefore wemay want to run it manually with breakpoints
	[Fact]
	public async Task SendMessageFromUserToGroupAsync()
	{
		const string Space = "space102";
		const string Suite = "connectiontest102";
		var user1connection = new ConnectionParameters
		{
			Id = Guid.NewGuid().ToString(),
			User = "userA",
			Metadata = new()
			{
				ClientId = Guid.NewGuid().ToString(),
				Suite = Suite,
				SpaceId = Space
			}
		};
		await using var client1 = await _fixture.GetClientAsync(_testOutputHelper, sustainInFixture: false, user1connection);
		var user2connection = new ConnectionParameters
		{
			Id = Guid.NewGuid().ToString(),
			User = "userB",
			Metadata = new()
			{
				ClientId = Guid.NewGuid().ToString(),
				Suite = Suite,
				SpaceId = Space
			}
		};
		var user2event = new GroupMessageRecieved(_testOutputHelper);
		await using var client2 = await _fixture.GetClientAsync(_testOutputHelper, sustainInFixture: false, user2connection, user2event);
		var user3connection = new ConnectionParameters
		{
			Id = Guid.NewGuid().ToString(),
			User = "userC",
			Metadata = new()
			{
				ClientId = Guid.NewGuid().ToString(),
				Suite = Suite,
				SpaceId = Space
			}
		};
		var user3event = new GroupMessageRecieved(_testOutputHelper);
		await using var client3 = await _fixture.GetClientAsync(_testOutputHelper, sustainInFixture: false, user3connection, user3event);
		await client1!.ConnectAsync(CancellationToken.None);
		await client2!.ConnectAsync(CancellationToken.None);
		await client3!.ConnectAsync(CancellationToken.None);
		await Task.Delay(1000);
		var sent = await client1.SendMessageToGroupAsync(Newtonsoft.Json.JsonConvert.SerializeObject(new ChatMessage { Body = ["This is a group test"] }), CancellationToken.None);
		Assert.True(sent);
		var recieved1 = await user2event.WaitAsync(() => user2event.Recieved && sent, CancellationToken.None);
		var recieved2 = await user3event.WaitAsync(() => user3event.Recieved && sent, CancellationToken.None);
		Assert.True(recieved1 && recieved2);
	}

	[Fact]
	public async Task SendMessageFromUserToGroupWithOfflineUsersAsync()
	{
		//Note: Running this test requires the observation of the storage account's table manually
		const string Space = "space1025";
		const string Suite = "connectiontest1025";
		var user1connection = new ConnectionParameters
		{
			Id = Guid.NewGuid().ToString(),
			User = "userA",
			Metadata = new()
			{
				ClientId = Guid.NewGuid().ToString(),
				Suite = Suite,
				SpaceId = Space
			}
		};
		await using var client1 = await _fixture.GetClientAsync(_testOutputHelper, sustainInFixture: false, user1connection);
		var user2connection = new ConnectionParameters
		{
			Id = Guid.NewGuid().ToString(),
			User = "userB",
			Metadata = new()
			{
				ClientId = Guid.NewGuid().ToString(),
				Suite = Suite,
				SpaceId = Space
			}
		};
		await using var client2 = await _fixture.GetClientAsync(_testOutputHelper, sustainInFixture: false, user2connection);
		await client1!.ConnectAsync(CancellationToken.None);
		await client2!.ConnectAsync(CancellationToken.None);
		await client2!.StopAsync(CancellationToken.None);
		await Task.Delay(5000);
		var sent = await client1.SendMessageToGroupAsync(Newtonsoft.Json.JsonConvert.SerializeObject(new ChatMessage { Body = ["This is a group test for offline users"] }), CancellationToken.None);
		Assert.True(sent);
	}

	[Fact]
	public async Task SendUserStatusToGroupAsync()
	{
		const string Space = "space103";
		const string Suite = "connectiontest103";
		var user1connection = new ConnectionParameters
		{
			Id = Guid.NewGuid().ToString(),
			User = "userA",
			Metadata = new()
			{
				ClientId = Guid.NewGuid().ToString(),
				Suite = Suite,
				SpaceId = Space
			}
		};
		await using var client1 = await _fixture.GetClientAsync(_testOutputHelper, sustainInFixture: false, user1connection);
		var user2connection = new ConnectionParameters
		{
			Id = Guid.NewGuid().ToString(),
			User = "userB",
			Metadata = new()
			{
				ClientId = Guid.NewGuid().ToString(),
				Suite = Suite,
				SpaceId = Space
			}
		};
		var user2event = new StatusReportRecieved(_testOutputHelper);
		await using var client2 = await _fixture.GetClientAsync(_testOutputHelper, sustainInFixture: false, user2connection, user2event);
		var user3connection = new ConnectionParameters
		{
			Id = Guid.NewGuid().ToString(),
			User = "userC",
			Metadata = new()
			{
				ClientId = Guid.NewGuid().ToString(),
				Suite = Suite,
				SpaceId = Space
			}
		};
		var user3event = new StatusReportRecieved(_testOutputHelper);
		await using var client3 = await _fixture.GetClientAsync(_testOutputHelper, sustainInFixture: false, user3connection, user3event);
		await client1!.ConnectAsync(CancellationToken.None);
		await client2!.ConnectAsync(CancellationToken.None);
		await client3!.ConnectAsync(CancellationToken.None);
		await Task.Delay(1000);
		var reported = await client1!.ReportStatusToGroupAsync(new StatusDetails { Description = "Running Tests", Title = "Testing" }, CancellationToken.None);
		Assert.True(reported);
		var recieved1 = await user2event.WaitAsync(() => user2event.Recieved && reported, CancellationToken.None);
		var recieved2 = await user3event.WaitAsync(() => user3event.Recieved && reported, CancellationToken.None);
		Assert.True(recieved1 && recieved2);
	}

	[Fact]
	public async Task SendUserStatusToUserAsync()
	{
		const string Space = "space104";
		const string Suite = "connectiontest104";
		var user1connection = new ConnectionParameters
		{
			Id = Guid.NewGuid().ToString(),
			User = "userA",
			Metadata = new()
			{
				ClientId = Guid.NewGuid().ToString(),
				Suite = Suite,
				SpaceId = Space
			}
		};
		await using var client1 = await _fixture.GetClientAsync(_testOutputHelper, sustainInFixture: false, user1connection);
		var user2connection = new ConnectionParameters
		{
			Id = Guid.NewGuid().ToString(),
			User = "userB",
			Metadata = new()
			{
				ClientId = Guid.NewGuid().ToString(),
				Suite = Suite,
				SpaceId = Space
			}
		};
		var user2event = new StatusReportRecieved(_testOutputHelper);
		await using var client2 = await _fixture.GetClientAsync(_testOutputHelper, sustainInFixture: false, user2connection, user2event);		
		await client1!.ConnectAsync(CancellationToken.None);
		await client2!.ConnectAsync(CancellationToken.None);
		await Task.Delay(1000);
		var reported = await client1!.ReportStatusToUserAsync(user2connection.User/*the target username*/, new StatusDetails { Description = "Running Tests", Title = "Testing" }, CancellationToken.None);
		Assert.True(reported);
		Assert.True(await user2event.WaitAsync(() => user2event.Recieved && reported, CancellationToken.None));
	}

	private class UserMessageRecieved(ITestOutputHelper testOutputHelper) : TestEvent(testOutputHelper)
	{
		public override string? Target => TargetEvent.MessageDeliveredToUser.ToString();
		public bool Recieved { get; private set; } = false;

		public override Task Handle(object argument)
		{
			Recieved = true;
			return base.Handle(argument);
		}
	}

	private class GroupMessageRecieved(ITestOutputHelper testOutputHelper) : UserMessageRecieved(testOutputHelper)
	{
		public override string? Target => TargetEvent.MessageDeliveredToGroup.ToString();
	}

	private class StatusReportRecieved(ITestOutputHelper testOutputHelper) : UserMessageRecieved(testOutputHelper)
	{
		public override string? Target => TargetEvent.UserStatusReported.ToString();
	}
}
