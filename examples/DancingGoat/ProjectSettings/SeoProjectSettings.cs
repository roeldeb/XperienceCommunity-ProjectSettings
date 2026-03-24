using Kentico.Xperience.Admin.Base.FormAnnotations;
using Kentico.Xperience.Admin.Websites.FormAnnotations;

using XperienceCommunity.ProjectSettings.Classes;

namespace DancingGoat.ProjectSettings;

public class SeoProjectSettings : IProjectSettingsType
{
    // ReSharper disable once InconsistentNaming
    public const string UIPAGE_SLUG = "edit-seo";
    // ReSharper disable once InconsistentNaming
    public const string UIPAGE_NAME = "SEO Settings";

    public string SettingsSlug => UIPAGE_SLUG;
    public string SettingsName => "Seo.Settings";
    public string SettingsDisplayName => "SEO Settings";
    public string SettingsExplanation => "Setup your <strong>SEO</strong> here.";

    [CheckBoxComponent(Label = "Show Robots", Order = 101, ExplanationText = "Do you want to show some robots?")]
    public virtual bool ShowRobots { get; set; } = false;

    [UrlSelectorComponent(Label = "Favicon", Order = 102)]
    public virtual string Favicon { get; set; } = string.Empty;
}
