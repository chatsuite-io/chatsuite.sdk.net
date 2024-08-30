using ChatSuite.Sdk.Core;
using ChatSuite.Sdk.IntegrationTests.Framework;
using Xunit.Microsoft.DependencyInjection.Attributes;

namespace ChatSuite.Sdk.IntegrationTests;

[TestCaseOrderer("Xunit.Microsoft.DependencyInjection.TestsOrder.TestPriorityOrderer", "Xunit.Microsoft.DependencyInjection")]
public class ConnectionTests(ITestOutputHelper testOutputHelper, ReliableConnectionFixture fixture) : TestBed<ReliableConnectionFixture>(testOutputHelper, fixture)
{
	[Fact, TestOrder(1)]
    public async Task ConnectionEstablishesAsync()
    {
		await using var client = await _fixture.GetClientAsync(_testOutputHelper);
		await client!.ConnectAsync(CancellationToken.None);
		Assert.True(client!.IsConnected());
    }

	[Fact, TestOrder(10)]
	public async Task TestConnectionEventsAsync()
	{
		var connection1params = new ConnectionParameters
		{
			Id = Guid.NewGuid().ToString(),
			User = "user1",
			Metadata = new()
			{
				ClientId = Guid.NewGuid().ToString(),
				Suite = "suite1",
				SpaceId = "space1"
			}
		};
		var connection2params = new ConnectionParameters
		{
			Id = Guid.NewGuid().ToString(),
			User = "user2",
			Metadata = new()
			{
				ClientId = Guid.NewGuid().ToString(),
				Suite = "suite1",
				SpaceId = "space1"
			}
		};
		var disconnection1 = new UserDisconnected(_testOutputHelper);
		var connection1 = new UserConnected(_testOutputHelper);
		await using var client1 = await _fixture.GetClientAsync(_testOutputHelper, sustainInFixture: false, connection1params, connection1, disconnection1);
		var disconnection2 = new UserDisconnected(_testOutputHelper);
		var connection2 = new UserConnected(_testOutputHelper);
		await using var client2 = await _fixture.GetClientAsync(_testOutputHelper, sustainInFixture: false, connection2params, connection2, disconnection2);
		await client1!.ConnectAsync(CancellationToken.None);
		await client2!.ConnectAsync(CancellationToken.None);
		var connected1 = await connection1.WaitAsync(() => connection1.Connected, CancellationToken.None);
		var connected2 = await connection2.WaitAsync(() => connection2.Connected, CancellationToken.None);
		Assert.True(connected1);
		Assert.True(connected2);
		//await client1!.StopAsync(CancellationToken.None);
		//await client2!.StopAsync(CancellationToken.None);
		//var disconnected1 = await disconnection1.WaitAsync(() => disconnection1.Disconnected, CancellationToken.None);
		//var disconnected2 = await disconnection2.WaitAsync(() => disconnection2.Disconnected, CancellationToken.None);
		//Assert.True(disconnected1);
		//Assert.True(disconnected2);
	}

	private class UserConnected(ITestOutputHelper testOutputHelper) : TestEvent(testOutputHelper)
	{
		public override string? Target => TargetEvent.OnUserConnected.ToString();
		public bool Connected { get; private set; }

		public override Task HandleAsync(object argument)
		{
			Connected = true;
			return base.HandleAsync(argument);
		}
	}

	private class UserDisconnected(ITestOutputHelper testOutputHelper) : TestEvent(testOutputHelper)
	{
		public override string? Target => TargetEvent.OnUserDisconnected.ToString();
		public bool Disconnected { get; private set; }

		public override Task HandleAsync(object argument)
		{			
			Disconnected = true;
			return base.HandleAsync(argument);
		}
	}
}