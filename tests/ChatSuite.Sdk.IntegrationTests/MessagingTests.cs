using ChatSuite.Sdk.Core.Message;
using Xunit.Microsoft.DependencyInjection.Attributes;

namespace ChatSuite.Sdk.IntegrationTests;

[TestCaseOrderer("Xunit.Microsoft.DependencyInjection.TestsOrder.TestPriorityOrderer", "Xunit.Microsoft.DependencyInjection")]
[Collection("Messaging Tests")]
public class MessagingTests(ITestOutputHelper testOutputHelper, ReliableConnectionFixture fixture) : TestBed<ReliableConnectionFixture>(testOutputHelper, fixture)
{
	[Fact, TestOrder(1)]
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
		});
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
		await using var client2 = await _fixture.GetClientAsync(_testOutputHelper,sustainInFixture: false, user2connection);
		var received = false;
		client2!.AcquireUserMessageDeliveredEvent().OnResultReady += async obj => { received = true; };
		await client1!.ConnectAsync(CancellationToken.None);
		await client2!.ConnectAsync(CancellationToken.None);
		var sent = await client1.SendMessageToUserAsync("userB", new ChatMessage { Id = Guid.NewGuid().ToString(), Body = ["This is a test"] }, CancellationToken.None);
		Assert.True(sent);
		var timeoutToken = new CancellationTokenSource();
		timeoutToken.CancelAfter(TimeSpan.FromSeconds(5));
		while (!timeoutToken.IsCancellationRequested && !received){ }
		Assert.True(received);
	}

	//Note: this test may exhibit some timing issues and therefore we may want to run it manually with breakpoints
	[Fact, TestOrder(10)]
	public async Task SendMessageFromUserToGroupAsync()
	{
		const string Space = "space102";
		const string Suite = "connectiontest102";
		var user1connection = new ConnectionParameters
		{
			Id = Guid.NewGuid().ToString(),
			User = "userA00",
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
			User = "userB00",
			Metadata = new()
			{
				ClientId = Guid.NewGuid().ToString(),
				Suite = Suite,
				SpaceId = Space
			}
		};
		await using var client2 = await _fixture.GetClientAsync(_testOutputHelper, sustainInFixture: false, user2connection);
		var user3connection = new ConnectionParameters
		{
			Id = Guid.NewGuid().ToString(),
			User = "userC00",
			Metadata = new()
			{
				ClientId = Guid.NewGuid().ToString(),
				Suite = Suite,
				SpaceId = Space
			}
		};
		await using var client3 = await _fixture.GetClientAsync(_testOutputHelper, sustainInFixture: false, user3connection);
		var received1 = false;
		var received2 = false;
		client2!.AcquireGroupMessageDeliveredEvent().OnResultReady += async obj => { received1 = true; };
		client3!.AcquireGroupMessageDeliveredEvent().OnResultReady += async obj => { received2 = true; };
		await client1!.ConnectAsync(CancellationToken.None);
		await client2!.ConnectAsync(CancellationToken.None);
		await client3!.ConnectAsync(CancellationToken.None);
		await Task.Delay(1000);
		var sent = await client1.SendMessageToGroupAsync(new ChatMessage { Body = ["This is a group test"] }, CancellationToken.None);
		Assert.True(sent);	
		var timeoutToken1 = new CancellationTokenSource();
		timeoutToken1.CancelAfter(TimeSpan.FromSeconds(15));
		while (!timeoutToken1.IsCancellationRequested && !received1){ }
		var timeoutToken2 = new CancellationTokenSource();
		timeoutToken2.CancelAfter(TimeSpan.FromSeconds(15));
		while (!timeoutToken2.IsCancellationRequested && !received2){ }
		Assert.True(received1 && received2);
	}

	[Fact, TestOrder(20)]
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
		var sent = await client1.SendMessageToGroupAsync(new ChatMessage { Body = ["This is a group test for offline users"] }, CancellationToken.None);
		Assert.True(sent);
	}

	[Fact, TestOrder(30)]
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
		await using var client2 = await _fixture.GetClientAsync(_testOutputHelper, sustainInFixture: false, user2connection);
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
		await using var client3 = await _fixture.GetClientAsync(_testOutputHelper, sustainInFixture: false, user3connection);
		var received1 = false;
		var received2 = false;
		client2!.AcquireStatusReportReceivedEvent().OnResultReady += async obj => { received1 = true; };
		client3!.AcquireStatusReportReceivedEvent().OnResultReady += async obj => { received2 = true; };
		await client1!.ConnectAsync(CancellationToken.None);
		await client2!.ConnectAsync(CancellationToken.None);
		await client3!.ConnectAsync(CancellationToken.None);
		await Task.Delay(1000);
		var reported = await client1!.ReportStatusToGroupAsync(new StatusDetails { Description = "Running Tests", Title = "Testing" }, CancellationToken.None);
		var timeoutToken1 = new CancellationTokenSource();
		timeoutToken1.CancelAfter(TimeSpan.FromSeconds(10));
		while (!timeoutToken1.IsCancellationRequested && !received1){ }
		var timeoutToken2 = new CancellationTokenSource();
		timeoutToken2.CancelAfter(TimeSpan.FromSeconds(10));
		while (!timeoutToken2.IsCancellationRequested && !received2){ }
		Assert.True(received1 && received2);
		Assert.True(reported);
		Assert.True(received1 && received2);
	}

	[Fact, TestOrder(40)]
	public async Task SendUserStatusToUserAsync()
	{
		const string Space = "space104";
		const string Suite = "connectiontest104";
		var user1connection = new ConnectionParameters
		{
			Id = Guid.NewGuid().ToString(),
			User = "user1A",
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
			User = "user1B",
			Metadata = new()
			{
				ClientId = Guid.NewGuid().ToString(),
				Suite = Suite,
				SpaceId = Space
			}
		};
		await using var client2 = await _fixture.GetClientAsync(_testOutputHelper, sustainInFixture: false, user2connection);
		var received = false;
		client2!.AcquireStatusReportReceivedEvent().OnResultReady += async obj => { received = true; };
		await client1!.ConnectAsync(CancellationToken.None);
		await client2!.ConnectAsync(CancellationToken.None);
		await Task.Delay(1000);
		var reported = await client1!.ReportStatusToUserAsync(user2connection.User/*the target username*/, new StatusDetails { Description = "Running Tests", Title = "Testing" }, CancellationToken.None);
		var timeoutToken = new CancellationTokenSource();
		timeoutToken.CancelAfter(TimeSpan.FromSeconds(10));
		while (!timeoutToken.IsCancellationRequested && !received){ }
		Assert.True(reported);
		Assert.True(received);
	}
}
