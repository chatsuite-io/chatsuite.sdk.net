using Xunit.Microsoft.DependencyInjection.Attributes;

namespace ChatSuite.Sdk.IntegrationTests;

[TestCaseOrderer("Xunit.Microsoft.DependencyInjection.TestsOrder.TestPriorityOrderer", "Xunit.Microsoft.DependencyInjection")]
[Collection("Connection Tests")]
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
		var connected1 = false;
		var connected2 = false;
		var disconnected1 = false;
		await using var client1 = await _fixture.GetClientAsync(_testOutputHelper, sustainInFixture: false, connection1params);
		await using var client2 = await _fixture.GetClientAsync(_testOutputHelper, sustainInFixture: false, connection2params);
		var connected1Event = client1!.AcquireUserConnectedEvent();
		var connected2Event = client2!.AcquireUserConnectedEvent();
		var disconnected1Event = client1!.AcquireUserConnectedEvent();
		connected1Event.OnResultReady += async (o) => connected1 = true;
		connected1Event.OnResultReady += async (o) => connected2 = true;
		disconnected1Event.OnResultReady += async (o) => disconnected1 = true;
		var token1 = new CancellationTokenSource();
		var token2 = new CancellationTokenSource();
		var token3 = new CancellationTokenSource();
		var token4 = new CancellationTokenSource();
		await client1!.ConnectAsync(CancellationToken.None);
		await client2!.ConnectAsync(CancellationToken.None);
		token1.CancelAfter(10000);
		while (!token1.IsCancellationRequested && !connected1){ }
		token2.CancelAfter(10000);
		while (!token2.IsCancellationRequested && !connected2){ }
		Assert.True(connected1);
		Assert.True(connected2);
		token3.CancelAfter(15000);
		token4.CancelAfter(15000);
		await client1!.StopAsync(CancellationToken.None);
		await client2!.StopAsync(CancellationToken.None);
		while (!token3.IsCancellationRequested && !disconnected1){ }
		Assert.True(disconnected1);
	}
}