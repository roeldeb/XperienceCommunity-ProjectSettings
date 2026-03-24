using Microsoft.Extensions.Options;

namespace DancingGoat.ProjectSettings;

/// <summary>
/// Example demonstrating how to use channel settings with IOptions pattern
/// </summary>
public class ProjectSettingsUsageExample
{
    private readonly IOptions<SeoProjectSettings> _seoSettings;

    /// <summary>
    /// Constructor injection of settings via IOptions
    /// Settings are automatically retrieved from the database based on the current channel context
    /// </summary>
    public ProjectSettingsUsageExample(IOptions<SeoProjectSettings> seoSettings)
    {
        _seoSettings = seoSettings;
    }

    public void UseSettings()
    {
        // Access settings via the Value property
        var settings = _seoSettings.Value;

        // Use the settings
        var showRobots = settings.ShowRobots;
        var favicon = settings.Favicon;

        // Settings are cached per request and per channel
    }
}

/// <summary>
/// Example of how to register channel settings in Program.cs or Startup.cs
/// </summary>
public static class ServiceRegistrationExample
{
    public static void Example()
    {
        // In Program.cs or Startup.cs:

        // builder.Services.AddProjectSettings<SeoProjectSettings>();

        // This registers:
        // 1. The base IChannelSettingsOptionsService (singleton)
        // 2. IOptions<SeoProjectSettings> that retrieves settings from database

        // Now you can inject IOptions<SeoProjectSettings> anywhere in your application
        // The settings will be automatically retrieved based on the current channel context
    }
}
