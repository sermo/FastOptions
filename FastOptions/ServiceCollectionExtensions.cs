using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace FastOptions;

public static class ServiceCollectionExtensions
{
	/// <summary>
	/// Removes the existing registration of <see cref="IOptionsSnapshot{TOptions}"/> from the service collection and adds the <see cref="FastOptionsSnapshot{TOptions}"/> implementation.
	/// <see href="https://github.com/dotnet/runtime/issues/53793">Background available here.</see>
	/// </summary>
	/// <param name="services">The <see cref="IServiceCollection"/> to add the service to.</param>
	/// <returns>A reference to this instance after the operation has completed.</returns>
	public static IServiceCollection AddFastOptions(this IServiceCollection services)
	{
		services.AddOptions();

		var descriptor = services.Single(descriptor => descriptor.ServiceType == typeof(IOptionsSnapshot<>));

		if (descriptor.ImplementationType == null || descriptor.ImplementationType != typeof(OptionsManager<>))
		{
			throw new InvalidOperationException("Service collection has an unknown descriptor for type IOptionsSnapshot<>");
		}

		if (!services.Remove(descriptor))
		{
			throw new InvalidOperationException("Unable to remove existing IOptionsSnapshot<> registration.");
		}

		services.Add(new ServiceDescriptor(descriptor.ImplementationType, descriptor.ImplementationType, descriptor.Lifetime));

		services.AddScoped(typeof(IOptionsSnapshot<>), typeof(FastOptionsSnapshot<>));

		return services;
	}
}