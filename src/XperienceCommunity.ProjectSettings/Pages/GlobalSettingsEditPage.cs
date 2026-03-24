using System.Data;
using System.Text.Json;

using CMS.ContentEngine;
using CMS.Core;
using CMS.Websites;

using Kentico.Xperience.Admin.Base;
using Kentico.Xperience.Admin.Base.Forms;

using XperienceCommunity.ProjectSettings.Classes;

namespace XperienceCommunity.ProjectSettings.Pages;

public class GlobalSettingsEditPage<T>(
    Kentico.Xperience.Admin.Base.Forms.Internal.IFormItemCollectionProvider formItemCollectionProvider,
    IFormDataBinder formDataBinder) : ModelEditPage<T>(formItemCollectionProvider, formDataBinder)
                             where T : class, IProjectSettingsType, new()
{

    private T model = new();
    private ProjectSettingsInfo? currentSetting;

    protected override T Model
    {
        get
        {
            model ??= new T();

            return model;
        }
    }

    public override Task ConfigurePage()
    {
        PageConfiguration.SubmitConfiguration.Label = "Save";

        PopulateData();

        var callouts = new List<CalloutConfiguration>();

        if (currentSetting is { ProjectSettingsID: 0 })
        {
            PageConfiguration.EditMode = FormEditMode.Disabled;

            var callout = new CalloutConfiguration
            {
                Headline = "Warning",
                Content = "System is using developer defaults, do you want to change these settings?",
                ContentAsHtml = true,
                Type = CalloutType.FriendlyWarning,
                ActionButton = new CalloutCommandButtonConfiguration
                {
                    ClickCommandName = nameof(CreateOverride),
                    Text = "Create override"
                }
            };

            callouts.Add(callout);
        }
        else
        {
            var overrideChannelNames = GetOverrideChannels();
            if (overrideChannelNames.Count > 0)
            {
                callouts.Add(new()
                {
                    Headline = "Warning",
                    Content = $"There are overrides for these settings in channel(s): {GetChannelSettingLinks(overrideChannelNames)}",
                    ContentAsHtml = true,
                    Type = CalloutType.FriendlyWarning
                });
            }
        }
        if (!string.IsNullOrEmpty(Model.SettingsExplanation))
        {
            callouts.Add(new()
            {
                Headline = "Information",
                Content = Model.SettingsExplanation,
                ContentAsHtml = true,
                Type = CalloutType.QuickTip
            });
        }

        if (callouts.Count > 0)
        {
            PageConfiguration.Callouts = callouts;
        }

        return base.ConfigurePage();

        string GetChannelSettingLinks(Dictionary<int, string> overrideChannelNames)
        {
            var links = overrideChannelNames.Select(kv =>
                          $"<a href=\"/admin/project-settings/channel-settings/{kv.Key}/{Model.SettingsSlug}\">{kv.Value}</a>");
            return string.Join(", ", links);
        }
    }

    [PageCommand]
    public async Task<ICommandResponse> CreateOverride()
    {
        try
        {
            if (currentSetting is { ProjectSettingsID: 0 })
            {
                await ProjectSettingsInfo.Provider.SetAsync(currentSetting);

                return Response()
                    .AddSuccessMessage("Project settings created.");
            }

            return Response()
                .AddErrorMessage("Project settings already exist.");
        }
        catch (Exception ex)
        {
            EventLogService.LogException(nameof(ChannelSettingsEditPage<T>), nameof(CreateOverride), ex);

            return Response()
                .AddErrorMessage("Unable to create project settings. Check eventlog for more details!");
        }
    }

    private Dictionary<int, string> GetOverrideChannels()
    {
        var channelIdsQuery = ProjectSettingsInfo.Provider.Get()
            .WhereEquals(nameof(ProjectSettingsInfo.ProjectSettingsName), Model.SettingsName)
            .WhereGreaterThan(nameof(ProjectSettingsInfo.ProjectSettingsChannelID), 0)
            .WhereNotEquals(nameof(ProjectSettingsInfo.ProjectSettingsValue), JsonSerializer.Serialize(Model))
            .AsSingleColumn(nameof(ProjectSettingsInfo.ProjectSettingsChannelID));

        var channelNames = ChannelInfo.Provider.Get()
            .Source(s => s.Join<WebsiteChannelInfo>(
                nameof(ChannelInfo.ChannelID),
                nameof(WebsiteChannelInfo.WebsiteChannelChannelID)))
            .WhereIn(nameof(WebsiteChannelInfo.WebsiteChannelID), channelIdsQuery)
            .Columns(nameof(WebsiteChannelInfo.WebsiteChannelID), nameof(ChannelInfo.ChannelDisplayName))
            .Result;

        var res = channelNames.Tables[0].AsEnumerable()
            .ToDictionary(
                row => row.Field<int>(nameof(WebsiteChannelInfo.WebsiteChannelID)),
                row => row.Field<string>(nameof(ChannelInfo.ChannelDisplayName)) ?? string.Empty
            );

        return res;
    }

    protected override async Task<ICommandResponse> ProcessFormData(T model, ICollection<IFormItem> formItems)
    {
        try
        {
            await SaveDataAsync();
        }
        catch (Exception ex)
        {
            EventLogService.LogException(nameof(GlobalSettingsEditPage<T>), nameof(ProcessFormData), ex);

            return GetErrorResponse("Unable to save data. Check eventlog for more details!");
        }

        return GetSuccessResponse("Settings saved successfully!");
    }

    private async Task SaveDataAsync()
    {
        if (Model != null)
        {
            var setting = ProjectSettingsInfo.Provider.Get()
                    .WhereEquals(nameof(ProjectSettingsInfo.ProjectSettingsName), Model.SettingsName)
                    .WhereEquals(nameof(ProjectSettingsInfo.ProjectSettingsChannelID), 0)
                    .FirstOrDefault()
                ?? new ProjectSettingsInfo
                {
                    ProjectSettingsChannelID = 0,
                    ProjectSettingsGUID = Guid.NewGuid(),
                    ProjectSettingsName = Model.SettingsName,
                    ProjectSettingsDisplayName = Model.SettingsDisplayName
                };
            setting.ProjectSettingsValue = JsonSerializer.Serialize(Model);

            await ProjectSettingsInfo.Provider.SetAsync(setting);

            // Update the current setting reference after save
            currentSetting = setting;
        }
    }

    private void PopulateData()
    {
        if (Model != null)
        {
            currentSetting = ProjectSettingsInfo.Provider.Get()
                     .WhereEquals(nameof(ProjectSettingsInfo.ProjectSettingsName), Model.SettingsName)
                     .WhereEquals(nameof(ProjectSettingsInfo.ProjectSettingsChannelID), 0)
                     .FirstOrDefault()
                 ?? new ProjectSettingsInfo
                 {
                     ProjectSettingsGUID = Guid.NewGuid(),
                     ProjectSettingsChannelID = 0,
                     ProjectSettingsName = Model.SettingsName,
                     ProjectSettingsDisplayName = Model.SettingsDisplayName
                 };

            model = !string.IsNullOrWhiteSpace(currentSetting.ProjectSettingsValue)
                ? JsonSerializer.Deserialize<T>(currentSetting.ProjectSettingsValue) ?? new T()
                : new T();

        }
    }

    private ICommandResponse<FormSubmissionResult> GetErrorResponse(params string[] messages)
    {
        var response = ResponseFrom(new FormSubmissionResult(FormSubmissionStatus.Error));

        foreach (string message in messages)
        {
            response.AddErrorMessage(message);
        }

        return response;
    }

    private ICommandResponse<FormSubmissionResult> GetSuccessResponse(string message)
    {
        var response = ResponseFrom(new FormSubmissionResult(FormSubmissionStatus.ValidationSuccess));

        response.AddSuccessMessage(message);

        return response;
    }

}
