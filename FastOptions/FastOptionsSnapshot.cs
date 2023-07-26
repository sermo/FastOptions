using System.Collections.Concurrent;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace FastOptions;

/// <summary>
/// Represents a fast implementation of <see cref="IOptionsSnapshot{TOptions}"/>.
/// </summary>
/// <typeparam name="TOptions">The options type.</typeparam>
public class FastOptionsSnapshot<TOptions> : IOptionsSnapshot<TOptions> where TOptions : class
{
	private readonly IServiceProvider serviceProvider;
	private readonly IOptionsMonitor<TOptions>? monitor;
	private readonly ConcurrentDictionary<string, TOptions> namedValuesDictionary = new ConcurrentDictionary<string, TOptions>();

	public FastOptionsSnapshot(IServiceProvider serviceProvider)
	{
		this.serviceProvider = serviceProvider;

		try
		{
			monitor = serviceProvider.GetService(typeof(IOptionsMonitor<TOptions>)) as IOptionsMonitor<TOptions>;
		}
		catch (InvalidOperationException)
		{
			// Swallow the exception and continue without the monitor.
			// This means that the type contains at least one scoped option and we'll need to fall back to OptionsManager (slow) later.
		}
	}

	/// <inheritdoc />
	public TOptions Value => Get(null);

	/// <inheritdoc />
	public TOptions Get(string? name)
	{
		var key = name ?? Options.DefaultName;

		var value =  monitor?.Get(name) ?? ((OptionsManager<TOptions>) serviceProvider.GetRequiredService(typeof(OptionsManager<TOptions>))).Get(name);

		namedValuesDictionary.TryAdd(key, value);

		return namedValuesDictionary[key];
	}
}