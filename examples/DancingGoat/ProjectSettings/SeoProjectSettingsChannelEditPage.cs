using CMS.ContentEngine;
using CMS.DataEngine;

using Kentico.Xperience.Admin.Base;
using Kentico.Xperience.Admin.Base.Forms;
using Kentico.Xperience.Admin.Base.Forms.Internal;

using XperienceCommunity.ProjectSettings.Pages;

using DancingGoat.ProjectSettings;

[assembly: UIPage(
    parentType: typeof(ProjectSettingsChannelEditSection),
    slug: SeoProjectSettings.UIPAGE_SLUG,
    uiPageType: typeof(SeoProjectSettingsChannelEditPage),
    name: SeoProjectSettings.UIPAGE_NAME,
    templateName: TemplateNames.EDIT,
    order: UIPageOrder.NoOrder)]

namespace DancingGoat.ProjectSettings;

public class SeoProjectSettingsChannelEditPage(
        IFormItemCollectionProvider formItemCollectionProvider,
        IFormDataBinder formDataBinder,
        IInfoProvider<ChannelInfo> channelInfoProvider)
    : ChannelSettingsEditPage<SeoProjectSettings>(formItemCollectionProvider, formDataBinder, channelInfoProvider);
