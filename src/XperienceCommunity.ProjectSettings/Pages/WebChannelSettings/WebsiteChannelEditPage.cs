using CMS.Websites;

using Kentico.Xperience.Admin.Base;

using XperienceCommunity.ProjectSettings.Pages;
using XperienceCommunity.ProjectSettings.Pages.WebChannelSettings;

[assembly: UIPage(
    parentType: typeof(ProjectSettingsChannelEditSection),
    slug: "edit",
    uiPageType: typeof(WebsiteChannelEditPage),
    name: "General info",
    templateName: TemplateNames.OVERVIEW,
    order: UIPageOrder.First)]


namespace XperienceCommunity.ProjectSettings.Pages.WebChannelSettings;

internal class WebsiteChannelEditPage : OverviewPage<WebsiteChannelInfo>
{
    public WebsiteChannelEditPage() => PageConfiguration.Callouts =
        [
            new CalloutConfiguration
            {
                Headline = "Channel settings",
                Content = "If any channelsettings are available, you will find them here.",
                ContentAsHtml = true,
                Type = CalloutType.QuickTip
            }
        ];

    [PageParameter(typeof(IntPageModelBinder))]
    public override int ObjectId { get; set; }
}
