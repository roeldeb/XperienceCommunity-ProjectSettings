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

    // Resolved via the service locator instead of constructor injection
    // to keep the constructor signature backwards compatible for consumers
    private static IPageLinkGenerator PageLinkGenerator => Service.Resolve<IPageLinkGenerator>();

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
            ConfigureUnsupportedChannelPage();
        }
        else
        {
            ConfigureWebsiteChannelPage();
        }

        return base.ConfigurePage();
    }

    private void ConfigureUnsupportedChannelPage()
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

    private void ConfigureWebsiteChannelPage()
    {
        PopulateData();

        bool globalSettingsExist = GlobalSettingsExist();

        if (currentSetting is { ProjectSettingsID: > 0 })
        {
            AddResetAction(globalSettingsExist);
        }

        var callouts = BuildCallouts(globalSettingsExist);

        if (callouts.Count > 0)
        {
            PageConfiguration.Callouts = callouts;
        }
    }

    private void AddResetAction(bool globalSettingsExist)
    {
        string resetLabel = globalSettingsExist ? "Reset to global settings" : "Reset to developer defaults";

        PageConfiguration.AdditionalActions.Add(new ButtonEditTemplateActionComponent(ActionType.Command)
        {
            Label = resetLabel,
            Parameter = nameof(ResetToDefault),
            ButtonColor = ButtonColor.Secondary,
            Destructive = true,
            ConfirmationDialog = new ConfirmationDialog
            {
                Title = resetLabel,
                Detail = globalSettingsExist
                    ? "The channel-specific settings will be deleted and the global settings will apply."
                    : "The channel-specific settings will be deleted and the system will use the developer defaults.",
                Button = "Reset",
                Destructive = true
            }
        });
    }

    private List<CalloutConfiguration> BuildCallouts(bool globalSettingsExist)
    {
        var callouts = new List<CalloutConfiguration>();

        if (currentSetting is { ProjectSettingsID: 0 })
        {
            PageConfiguration.EditMode = FormEditMode.Disabled;
            callouts.Add(CreateMissingOverrideCallout(globalSettingsExist));
        }
        else if (globalSettingsExist)
        {
            callouts.Add(CreateOverridingGlobalSettingsCallout());
        }

        if (!string.IsNullOrEmpty(Model.SettingsExplanation))
        {
            callouts.Add(CreateExplanationCallout());
        }

        return callouts;
    }

    private CalloutConfiguration CreateMissingOverrideCallout(bool globalSettingsExist) => new()
    {
        Headline = globalSettingsExist ? "Using global settings" : "Using developer defaults",
        Content = globalSettingsExist
            ? $"This channel has no settings of its own — the <a href=\"/admin/project-settings/{Model.SettingsSlug}\">global settings</a> apply and are shown below. Create an override to use different values for this channel."
            : "This channel has no settings of its own and no global settings exist — the developer defaults apply and are shown below. Create an override to use different values for this channel.",
        ContentAsHtml = true,
        Type = CalloutType.FriendlyWarning,
        ActionButton = new CalloutCommandButtonConfiguration
        {
            ClickCommandName = nameof(CreateOverride),
            Text = "Create override"
        }
    };

    private CalloutConfiguration CreateOverridingGlobalSettingsCallout() => new()
    {
        Headline = "Overriding global settings",
        Content = $"These channel-specific settings override the <a href=\"/admin/project-settings/{Model.SettingsSlug}\">global settings</a>. Use \"Reset to global settings\" to return to the global values.",
        ContentAsHtml = true,
        Type = CalloutType.FriendlyWarning
    };

    private CalloutConfiguration CreateExplanationCallout() => new()
    {
        Headline = "Information",
        Content = Model.SettingsExplanation,
        ContentAsHtml = true,
        Type = CalloutType.QuickTip
    };

    [PageCommand]
    public async Task<ICommandResponse> ResetToDefault()
    {
        try
        {
            if (currentSetting is { ProjectSettingsID: > 0 })
            {
                await ProjectSettingsInfo.Provider.DeleteAsync(currentSetting);

                // Reload the current page so ConfigurePage reflects the removed override
                return NavigateTo(GetCurrentPagePath(), true);
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
                // Seed the override from the global settings (when available)
                // so creating it doesn't change the effective values
                currentSetting.ProjectSettingsValue = GetGlobalSettingsValue() ?? string.Empty;

                await ProjectSettingsInfo.Provider.SetAsync(currentSetting);

                // Reload the current page so ConfigurePage re-enables the form
                return NavigateTo(GetCurrentPagePath(), true);
            }

            return Response()
                .AddErrorMessage("Channel-specific settings already exist.");
        }
        catch (Exception ex)
        {
            EventLogService.LogException(nameof(ChannelSettingsEditPage<T>), nameof(CreateOverride), ex);

            return Response()
                .AddErrorMessage("Unable to create channel-specific settings. Check eventlog for more details!");
        }
    }

    private string GetCurrentPagePath() => PageLinkGenerator.GetPath(GetType(), new PageParameterValues
    {
        { typeof(ProjectSettingsChannelEditSection), ObjectId }
    });

    private bool GlobalSettingsExist() => ProjectSettingsInfo.Provider.Get()
            .WhereEquals(nameof(ProjectSettingsInfo.ProjectSettingsName), Model.SettingsName)
            .WhereEquals(nameof(ProjectSettingsInfo.ProjectSettingsChannelID), 0)
            .Any();

    private string? GetGlobalSettingsValue() => ProjectSettingsInfo.Provider.Get()
            .WhereEquals(nameof(ProjectSettingsInfo.ProjectSettingsName), Model.SettingsName)
            .WhereEquals(nameof(ProjectSettingsInfo.ProjectSettingsChannelID), 0)
            .FirstOrDefault()?.ProjectSettingsValue;

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

            // Without a channel record, the global settings (when available) are
            // the effective values, so show those in the disabled form
            string? settingsValue = currentSetting.ProjectSettingsID > 0
                ? currentSetting.ProjectSettingsValue
                : GetGlobalSettingsValue();

            model = !string.IsNullOrWhiteSpace(settingsValue)
                ? JsonSerializer.Deserialize<T>(settingsValue) ?? new T()
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
