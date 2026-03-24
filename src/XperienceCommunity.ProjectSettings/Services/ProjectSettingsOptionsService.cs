using System.Text.Json;

using CMS.Websites.Routing;

using Microsoft.Extensions.Options;

using XperienceCommunity.ProjectSettings.Classes;

namespace XperienceCommunity.ProjectSettings.Services;

public interface IProjectSettingsOptionsService
{
    public T GetSettings<T>(int channelId) where T : class, IProjectSettingsType, new();
}

public class ProjectSettingsOptionsService : IProjectSettingsOptionsService
{
    public T GetSettings<T>(int channelId) where T : class, IProjectSettingsType, new()
    {
        var settingsType = new T();
        string settingsName = settingsType.SettingsName;

        // Try to get settings for the specific channel first
        var setting = ProjectSettingsInfo.Provider.Get()
            .WhereEquals(nameof(ProjectSettingsInfo.ProjectSettingsChannelID), channelId)
            .WhereEquals(nameof(ProjectSettingsInfo.ProjectSettingsName), settingsName)
            .TopN(1)
            .FirstOrDefault();

        // If not found, try to get global settings (channelId = 0)
        setting ??= ProjectSettingsInfo.Provider.Get()
            .WhereEquals(nameof(ProjectSettingsInfo.ProjectSettingsChannelID), 0)
            .WhereEquals(nameof(ProjectSettingsInfo.ProjectSettingsName), settingsName)
            .TopN(1)
            .FirstOrDefault();

        // If no settings found, return default instance
        if (setting == null || string.IsNullOrWhiteSpace(setting.ProjectSettingsValue))
        {
            return new T();
        }

        try
        {
            return JsonSerializer.Deserialize<T>(setting.ProjectSettingsValue) ?? new T();
        }
        catch
        {
            return new T();
        }
    }
}

/// <summary>
/// Options wrapper that retrieves channel settings from the database based on the current channel context.
/// </summary>
public class ProjectSettingsOptions<T>(IProjectSettingsOptionsService settingsService, IWebsiteChannelContext channelContext) : IOptions<T>
    where T : class, IProjectSettingsType, new()
{
    private T? cachedValue;
    private int cachedChannelId = -1;

    public T Value
    {
        get
        {
            int currentChannelId = channelContext.WebsiteChannelID;

            // Cache the value per channel to avoid repeated database queries within the same request
            if (cachedValue == null || cachedChannelId != currentChannelId)
            {
                cachedValue = settingsService.GetSettings<T>(currentChannelId);
                cachedChannelId = currentChannelId;
            }

            return cachedValue;
        }
    }
}
