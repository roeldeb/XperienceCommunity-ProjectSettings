using CMS.Membership;

using Kentico.Xperience.Admin.Base;
using Kentico.Xperience.Admin.Base.UIPages;

using XperienceCommunity.ProjectSettings;

[assembly: UIApplication(
    identifier: ProjectSettingsApplication.Identifier,
    type: typeof(ProjectSettingsApplication),
    slug: "project-settings",
    name: "Project settings",
    category: BaseApplicationCategories.CONFIGURATION,
    icon: Icons.BoxCogwheel,
    templateName: TemplateNames.SECTION_LAYOUT)]

namespace XperienceCommunity.ProjectSettings;

[UIPermission(SystemPermissions.VIEW)]
[UIPermission(SystemPermissions.CREATE)]
[UIPermission(SystemPermissions.DELETE)]
[UIPermission(SystemPermissions.UPDATE)]
public class ProjectSettingsApplication : ApplicationPage
{
    public const string Identifier = "XperienceCommunity.ProjectSettings.ProjectSettingsApplication";
}
