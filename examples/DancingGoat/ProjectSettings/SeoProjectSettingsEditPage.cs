using Kentico.Xperience.Admin.Base;
using Kentico.Xperience.Admin.Base.Forms;
using Kentico.Xperience.Admin.Base.Forms.Internal;
using XperienceCommunity.ProjectSettings;
using DancingGoat.ProjectSettings;
using XperienceCommunity.ProjectSettings.Pages;

[assembly: UIPage(parentType: typeof(ProjectSettingsApplication),
    slug: SeoProjectSettings.UIPAGE_SLUG,
    uiPageType: typeof(SeoProjectSettingsEditPage),
    name: SeoProjectSettings.UIPAGE_NAME,
    templateName: TemplateNames.EDIT,
    order: 100)]

namespace DancingGoat.ProjectSettings;

public class SeoProjectSettingsEditPage(
        IFormItemCollectionProvider formItemCollectionProvider,
        IFormDataBinder formDataBinder)
    : GlobalSettingsEditPage<SeoProjectSettings>(formItemCollectionProvider, formDataBinder);
