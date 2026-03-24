namespace XperienceCommunity.ProjectSettings.Classes;

public interface IProjectSettingsType
{
    /// <summary>
    /// When settings are used both as global and channel-specific, this slug is used in the
    /// friendly warning when channel-specific settings are present and differ from global settings.
    /// </summary>
    public string SettingsSlug { get; }
    public string SettingsName { get; }
    public string SettingsDisplayName { get; }
    public string SettingsExplanation { get; }
}
