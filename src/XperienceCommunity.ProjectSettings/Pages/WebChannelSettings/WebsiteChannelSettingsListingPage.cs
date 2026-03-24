using CMS.ContentEngine;
using CMS.DataEngine;
using CMS.Websites;

using Kentico.Xperience.Admin.Base;

using XperienceCommunity.ProjectSettings;
using XperienceCommunity.ProjectSettings.Pages.WebChannelSettings;

[assembly: UIPage(
    parentType: typeof(ProjectSettingsApplication),
    slug: "channel-settings",
    uiPageType: typeof(WebsiteChannelSettingsListingPage),
    name: "Channel settings",
    templateName: TemplateNames.LISTING,
    order: UIPageOrder.NoOrder)]

namespace XperienceCommunity.ProjectSettings.Pages.WebChannelSettings;

internal class WebsiteChannelSettingsListingPage(IInfoProvider<ChannelInfo> channelInfoProvider) : ListingPage
{
    protected override string ObjectType => WebsiteChannelInfo.OBJECT_TYPE;

    public override async Task ConfigurePage()
    {
        PageConfiguration.ColumnConfigurations
            .AddColumn(nameof(WebsiteChannelInfo.WebsiteChannelChannelID), "Channel", formatter: (value, _) => GetChannelName(Convert.ToInt32(value)));

        PageConfiguration.AddEditRowAction<ProjectSettingsChannelEditSection>();

        await base.ConfigurePage();
    }

    private string? GetChannelName(int id) => channelInfoProvider.Get(id)?.ChannelDisplayName;
}
