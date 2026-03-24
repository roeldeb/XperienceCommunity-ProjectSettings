using System.Text.Json;

using CMS.ContentEngine;
using CMS.Core;
using CMS.DataEngine;

using Kentico.Xperience.Admin.Base;
using Kentico.Xperience.Admin.Base.Forms;

using XperienceCommunity.ProjectSettings.Classes;

namespace XperienceCommunity.ProjectSettings.Pages;

public class ChannelSettingsEditPage<T>(Kentico.Xperience.Admin.Base.Forms.Internal.IFormItemCollectionProvider formItemCollectionProvider,
                             IFormDataBinder formDataBinder,
                             IInfoProvider<ChannelInfo> channelInfoProvider) : ModelEditPage<T>(formItemCollectionProvider, formDataBinder)
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

    [PageParameter(typeof(IntPageModelBinder))]
    public int ObjectId { get; set; }

    public override Task ConfigurePage()
    {
        PageConfiguration.SubmitConfiguration.Label = "Save";

        var channel = channelInfoProvider.Get(ObjectId);
        if (channel?.ChannelType != ChannelType.Website)
        {
            PageConfiguration.SubmitConfiguration.Visible = false;
            PageConfiguration.EditMode = FormEditMode.Disabled;
            PageConfiguration.Headline = "Channel custom settings is not supported.";

            PageConfiguration.Callouts =
            [
                new CalloutConfiguration
                {
                        Headline = "NOT SUPPORTED!",
                        Content = "<p>Channel custom settings is supported only for <strong>Website</strong> channels!</p>",
                        ContentAsHtml = true,
                        Type = CalloutType.FriendlyWarning
                    }
            ];
        }
        else
        {
            PopulateData();

            var callouts = new List<CalloutConfiguration>();

            bool overridesGlobalSettings = CheckIfSettingOverridesGlobal();
            if (overridesGlobalSettings)
            {
                if (currentSetting is { ProjectSettingsID: > 0 })
                {
                    var callout = new CalloutConfiguration
                    {
                        Headline = "Warning",
                        Content = $"These settings override <a href=\"/admin/project-settings/{Model.SettingsSlug}\">global settings</a>",
                        ContentAsHtml = true,
                        Type = CalloutType.FriendlyWarning,
                        ActionButton = new CalloutCommandButtonConfiguration
                        {
                            ClickCommandName = nameof(ResetToDefault),
                            Text = "Reset to default"
                        }
                    };

                    callouts.Add(callout);
                }
                else
                {
                    PageConfiguration.EditMode = FormEditMode.Disabled;

                    var callout = new CalloutConfiguration
                    {
                        Headline = "Warning",
                        Content = $"There are <a href=\"/admin/project-settings/{Model.SettingsSlug}\">global settings</a> available. Do you want to override these settings?",
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
            }

            if (!string.IsNullOrEmpty(Model.SettingsExplanation))
            {
                callouts.Add(new()
                {
                    Headline = "Information",
                    Content = Model.SettingsExplanation,
                    ContentAsHtml = true,
                    Type = CalloutType.QuickTip
                }
                );
            }

            if (callouts.Count > 0)
            {
                PageConfiguration.Callouts = callouts;
            }

        }

        return base.ConfigurePage();
    }

    [PageCommand]
    public async Task<ICommandResponse> LoadData()
    {
        await ConfigurePage();

        return Response();
    }

    [PageCommand]
    public async Task<ICommandResponse> ResetToDefault()
    {
        try
        {
            if (currentSetting is { ProjectSettingsID: > 0 })
            {
                await ProjectSettingsInfo.Provider.DeleteAsync(currentSetting);

                return Response().UseCommand("LoadData")
                    .AddSuccessMessage("Channel-specific settings removed. Using global defaults.");
            }

            return Response()
                .AddWarningMessage("Already using global defaults.");
        }
        catch (Exception ex)
        {
            EventLogService.LogException(nameof(ChannelSettingsEditPage<T>), nameof(ResetToDefault), ex);

            return Response()
                .AddErrorMessage("Unable to reset settings. Check eventlog for more details!");
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
                    .AddSuccessMessage("Channel-specific settings created.");
            }

            return Response()
                .AddErrorMessage("Channel-specific settings already exist.");
        }
        catch (Exception ex)
        {
            EventLogService.LogException(nameof(ChannelSettingsEditPage<T>), nameof(ResetToDefault), ex);

            return Response()
                .AddErrorMessage("Unable to create channel-specific settings. Check eventlog for more details!");
        }
    }

    private bool CheckIfSettingOverridesGlobal() => ProjectSettingsInfo.Provider.Get()
            .WhereEquals(nameof(ProjectSettingsInfo.ProjectSettingsName), Model.SettingsName)
            .WhereEquals(nameof(ProjectSettingsInfo.ProjectSettingsChannelID), 0)
            .WhereNotEquals(nameof(ProjectSettingsInfo.ProjectSettingsValue), JsonSerializer.Serialize(Model))
            .Any();

    protected override async Task<ICommandResponse> ProcessFormData(T model, ICollection<IFormItem> formItems)
    {
        try
        {
            await SaveDataAsync();
        }
        catch (Exception ex)
        {
            EventLogService.LogException(nameof(ChannelSettingsEditPage<T>), nameof(ProcessFormData), ex);

            return GetErrorResponse("Unable to save data. Check eventlog for more details!");
        }

        return GetSuccessResponse("Settings saved successfully!");
    }

    private async Task SaveDataAsync()
    {
        if (Model != null)
        {
            var setting = ProjectSettingsInfo.Provider.Get()
                    .WhereEquals(nameof(ProjectSettingsInfo.ProjectSettingsChannelID), ObjectId)
                    .WhereEquals(nameof(ProjectSettingsInfo.ProjectSettingsName), Model.SettingsName)
                    .FirstOrDefault()
                ?? new ProjectSettingsInfo
                {
                    ProjectSettingsGUID = Guid.NewGuid(),
                    ProjectSettingsChannelID = ObjectId,
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
                .WhereEquals(nameof(ProjectSettingsInfo.ProjectSettingsChannelID), ObjectId)
                .WhereEquals(nameof(ProjectSettingsInfo.ProjectSettingsName), Model.SettingsName)
                .FirstOrDefault()
            ?? new ProjectSettingsInfo
            {
                ProjectSettingsGUID = Guid.NewGuid(),
                ProjectSettingsChannelID = ObjectId,
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
