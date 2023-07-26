using FastOptions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace FastOptions.Tests;

public class FastOptionsSnapshotTest
{
	[Fact]
	public void ConfigurationCachingTest()
	{
		var config = new ConfigurationBuilder().AddInMemoryCollection().Build();
		var services = new ServiceCollection()
			.AddOptions()
			.AddSingleton<IConfigureOptions<TestOptions>, TestConfigure>()
			.AddSingleton<IOptionsChangeTokenSource<TestOptions>>(new ConfigurationChangeTokenSource<TestOptions>(Options.DefaultName, config))
			.AddSingleton<IOptionsChangeTokenSource<TestOptions>>(new ConfigurationChangeTokenSource<TestOptions>("1", config))
			.AddFastOptions()
			.BuildServiceProvider();

		var factory = services.GetRequiredService<IServiceScopeFactory>();
		TestOptions? options;
		TestOptions? namedOne;
		using (var scope = factory.CreateScope())
		{
			options = scope.ServiceProvider.GetRequiredService<IOptionsSnapshot<TestOptions>>().Value;
			Assert.Equal(options, scope.ServiceProvider.GetRequiredService<IOptionsSnapshot<TestOptions>>().Value);
			Assert.Equal(1, TestConfigure.ConfigureCount);
			namedOne = scope.ServiceProvider.GetRequiredService<IOptionsSnapshot<TestOptions>>().Get("1");
			Assert.Equal(namedOne, scope.ServiceProvider.GetRequiredService<IOptionsSnapshot<TestOptions>>().Get("1"));
			Assert.Equal(2, TestConfigure.ConfigureCount);
		}
		Assert.Equal(1, TestConfigure.CtorCount);
		using (var scope = factory.CreateScope())
		{
			var options2 = scope.ServiceProvider.GetRequiredService<IOptionsSnapshot<TestOptions>>().Value;
			Assert.Equal(options, options2);
			Assert.Equal(2, TestConfigure.ConfigureCount);
			var namedOne2 = scope.ServiceProvider.GetRequiredService<IOptionsSnapshot<TestOptions>>().Get("1");
			Assert.Equal(namedOne2, namedOne);
			Assert.Equal(2, TestConfigure.ConfigureCount);
		}
		Assert.Equal(1, TestConfigure.CtorCount);
	}

	[Fact]
	public void ConfigurationFallbackTest()
	{
		var config = new ConfigurationBuilder()
			.AddInMemoryCollection()
			.Build();

		var serviceCollectionWithoutScopedConfiguration = new ServiceCollection()
			.Configure<TestOptions>(config.GetSection("TestOptions"))
			.AddScoped<IConfigureOptions<TestOptions>>(s => new TestConfigure(1))
			.AddFastOptions();

		using (var services = serviceCollectionWithoutScopedConfiguration.BuildServiceProvider())
		{
			var options = services.GetRequiredService<IOptionsSnapshot<TestOptions>>().Value;
				Assert.Equal(1, options.Id);
		}

		var serviceCollectionWithScopedConfiguration = serviceCollectionWithoutScopedConfiguration
			.AddScoped<IConfigureOptions<TestOptions>>(s => new TestConfigure(2));

		using (var services = serviceCollectionWithScopedConfiguration.BuildServiceProvider())
		{
			var factory = services.GetRequiredService<IServiceScopeFactory>();
			using (var scope = factory.CreateScope())
			{
				var options = scope.ServiceProvider.GetRequiredService<IOptionsSnapshot<TestOptions>>().Value;
				Assert.Equal(2, options.Id);
			}
		}

	}

	private class TestOptions
	{
		public int Id { get; set; } = 0;
	}

	private class TestConfigure : IConfigureNamedOptions<TestOptions>
	{
		private readonly int? id;
		public static int ConfigureCount;
		public static int CtorCount;

		public TestConfigure()
		{
			CtorCount++;
		}

		public TestConfigure(int id) : this()
		{
			this.id = id;
		}

		public void Configure(string? name, TestOptions options)
		{
			ConfigureCount++;

			if (id.HasValue)
			{
				options.Id = id.Value;
			}
		}

		public void Configure(TestOptions options) => Configure(Options.DefaultName, options);
	}
}