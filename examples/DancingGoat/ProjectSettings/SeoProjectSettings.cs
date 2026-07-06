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
    public string SettingsExplanation => "Configure how this channel appears in search engines and on social media. " +
        "These values are used as defaults for pages that don't specify their own metadata.";

    [TextInputComponent(Label = "Meta title suffix", Order = 101,
        ExplanationText = "Appended to every page title, e.g. \"Espresso Blend | Dancing Goat\".")]
    public virtual string MetaTitleSuffix { get; set; } = " | Dancing Goat";

    [TextAreaComponent(Label = "Default meta description", Order = 102, MaxRowsNumber = 3,
        ExplanationText = "Fallback description (~160 characters) for pages without their own.")]
    public virtual string DefaultMetaDescription { get; set; } =
        "Freshly roasted, ethically sourced coffee delivered to your door. Discover the Dancing Goat difference.";

    [DropDownComponent(Label = "Search engine indexing", Order = 103,
        ExplanationText = "Controls the robots meta tag emitted on every page.",
        Options = "index,follow;Index and follow (public site)\nnoindex,nofollow;Block indexing (staging)")]
    public virtual string RobotsDirective { get; set; } = "index,follow";

    [UrlSelectorComponent(Label = "Default social share image", Order = 104,
        ExplanationText = "Open Graph / Twitter card image used when a page has none (1200x630 recommended).")]
    public virtual string DefaultOpenGraphImage { get; set; } = string.Empty;
}
