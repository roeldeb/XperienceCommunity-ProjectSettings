using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

using XperienceCommunity.ProjectSettings.Admin;
using XperienceCommunity.ProjectSettings.Classes;
using XperienceCommunity.ProjectSettings.Services;

namespace XperienceCommunity.ProjectSettings;

public static class ProjectSettingsServiceCollectionExtensions
{
    /// <summary>
    /// Registers the core project settings services including the module installer and options service.
    /// This method is idempotent and can be called multiple times safely.
    /// </summary>
    internal static IServiceCollection AddProjectSettingsOptions(this IServiceCollection services)
    {
        // Register the module installer for database table creation (idempotent)
        services.TryAddSingleton<ISettingsModuleInstaller, SettingsModuleInstaller>();

        // Register the settings options service (idempotent)
        services.TryAddSingleton<IProjectSettingsOptionsService, ProjectSettingsOptionsService>();

        return services;
    }

    /// <summary>
    /// Registers a specific project settings type as IOptions{T}.
    /// Settings are retrieved from the database based on the current channel context.
    /// </summary>
    /// <typeparam name="T">The settings type that implements IProjectSettingsType</typeparam>
    public static IServiceCollection AddProjectSettings<T>(this IServiceCollection services)
        where T : class, IProjectSettingsType, new()
    {
        // Ensure the base services are registered (includes module installer)
        services.AddProjectSettingsOptions();

        // Register the options wrapper for this specific type
        // Must be Scoped because it depends on IWebsiteChannelContext (scoped per request)
        services.AddScoped<IOptions<T>, ProjectSettingsOptions<T>>();

        return services;
    }
}
