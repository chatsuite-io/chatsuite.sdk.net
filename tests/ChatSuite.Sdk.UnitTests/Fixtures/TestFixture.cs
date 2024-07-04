using ChatSuite.Sdk.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit.Microsoft.DependencyInjection;

namespace ChatSuite.UnitTests.Fixtures;

public class TestFixture : TestBedFixture
{
	protected override void AddServices(IServiceCollection services, IConfiguration? configuration) => services.AddUserIdProviders();
	protected override ValueTask DisposeAsyncCore() => new();
	protected override IEnumerable<TestAppSettings> GetTestAppSettings() => [];
}
