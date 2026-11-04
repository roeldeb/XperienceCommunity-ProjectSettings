using System.Text.Json;

using CMS.DataEngine;
using CMS.Helpers;
using CMS.Websites.Routing;

using Microsoft.Extensions.Options;

using XperienceCommunity.ProjectSettings.Classes;

namespace XperienceCommunity.ProjectSettings.Services;

public interface IProjectSettingsOptionsService
{
    public T GetSettings<T>(int channelId) where T : class, IProjectSettingsType, new();
}

public class ProjectSettingsOptionsService(
    IInfoProvider<ProjectSettingsInfo> projectSettingsProvider,
    IProgressiveCache progressiveCache) : IProjectSettingsOptionsService
{
    private const int CacheMinutes = 60;

    // Touched by Kentico whenever a ProjectSettingsInfo is created, updated or deleted
    // (TouchCacheDependencies on the type info); touches are synchronised across web farm instances.
    private static readonly string cacheDependencyKey = $"{ProjectSettingsInfo.OBJECT_TYPE}|all";

    public T GetSettings<T>(int channelId) where T : class, IProjectSettingsType, new()
    {
        string settingsName = new T().SettingsName;

        // The stored JSON is cached rather than the deserialized object: settings classes are mutable,
        // so every caller gets its own instance.
        string? settingsValue = progressiveCache.Load(
            cacheSettings =>
            {
                cacheSettings.CacheDependency = CacheHelper.GetCacheDependency(cacheDependencyKey);
                return LoadSettingsValue(settingsName, channelId);
            },
            new CacheSettings(CacheMinutes, nameof(ProjectSettingsOptionsService), settingsName, channelId));

        // If no settings found, return default instance
        if (string.IsNullOrWhiteSpace(settingsValue))
        {
            return new T();
        }

        try
        {
            return JsonSerializer.Deserialize<T>(settingsValue) ?? new T();
        }
        catch (JsonException)
        {
            return new T();
        }
    }

    /// <summary>
    /// Returns the stored value for the channel when it exists, otherwise the global value (channel 0).
    /// </summary>
    private string? LoadSettingsValue(string settingsName, int channelId) =>
        projectSettingsProvider.Get()
            .WhereEquals(nameof(ProjectSettingsInfo.ProjectSettingsName), settingsName)
            .WhereIn(nameof(ProjectSettingsInfo.ProjectSettingsChannelID), new[] { channelId, 0 })
            .OrderByDescending(nameof(ProjectSettingsInfo.ProjectSettingsChannelID))
            .TopN(1)
            .FirstOrDefault()?.ProjectSettingsValue;
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
