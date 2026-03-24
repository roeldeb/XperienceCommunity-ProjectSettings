using CMS.Websites;

using Kentico.Xperience.Admin.Base;

using XperienceCommunity.ProjectSettings.Pages;
using XperienceCommunity.ProjectSettings.Pages.WebChannelSettings;

[assembly: UIPage(
    parentType: typeof(WebsiteChannelSettingsListingPage),
    slug: PageParameterConstants.PARAMETERIZED_SLUG,
    uiPageType: typeof(ProjectSettingsChannelEditSection),
    name: "Edit",
    templateName: TemplateNames.SECTION_LAYOUT,
    order: 0)]


namespace XperienceCommunity.ProjectSettings.Pages;

public class ProjectSettingsChannelEditSection : EditSectionPage<WebsiteChannelInfo>;
