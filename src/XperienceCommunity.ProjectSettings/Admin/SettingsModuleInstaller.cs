using CMS.DataEngine;
using CMS.FormEngine;
using CMS.Modules;
using CMS.Websites;

using XperienceCommunity.ProjectSettings.Classes;

namespace XperienceCommunity.ProjectSettings.Admin;

internal interface ISettingsModuleInstaller
{
    public void Install();
}

internal class SettingsModuleInstaller(IInfoProvider<ResourceInfo> resourceInfoProvider) : ISettingsModuleInstaller
{
    public void Install()
    {
        var resourceInfo = InstallModule();

        InstallProjectSettingsConfigurationClass(resourceInfo);
    }

    private ResourceInfo InstallModule()
    {
        var resourceInfo = resourceInfoProvider.Get(SettingsConstants.ResourceConstants.ResourceName)
                           ?? new ResourceInfo();

        resourceInfo.ResourceDisplayName = SettingsConstants.ResourceConstants.ResourceDisplayName;
        resourceInfo.ResourceName = SettingsConstants.ResourceConstants.ResourceName;
        resourceInfo.ResourceDescription = SettingsConstants.ResourceConstants.ResourceDescription;
        resourceInfo.ResourceIsInDevelopment = SettingsConstants.ResourceConstants.ResourceIsInDevelopment;

        if (resourceInfo.HasChanged)
        {
            resourceInfoProvider.Set(resourceInfo);
        }

        return resourceInfo;
    }

    private static void InstallProjectSettingsConfigurationClass(ResourceInfo resourceInfo)
    {
        var info = DataClassInfoProvider.GetDataClassInfo(ProjectSettingsInfo.TYPEINFO.ObjectClassName) ??
                                      DataClassInfo.New(ProjectSettingsInfo.OBJECT_TYPE);

        info.ClassName = ProjectSettingsInfo.TYPEINFO.ObjectClassName;
        info.ClassTableName = ProjectSettingsInfo.TYPEINFO.ObjectClassName.Replace(".", "_");
        info.ClassDisplayName = "Channel settings";
        info.ClassResourceID = resourceInfo.ResourceID;
        info.ClassType = ClassType.OTHER;

        var formInfo = FormHelper.GetBasicFormDefinition(nameof(ProjectSettingsInfo.ProjectSettingsID));

        // Fixed field GUIDs: without them every FormFieldInfo gets a random GUID, so each
        // environment ends up with a different class definition (CI/CD churn).
        var idField = formInfo.GetFormField(nameof(ProjectSettingsInfo.ProjectSettingsID));
        idField.Guid = new Guid("8fb8ac7e-be69-42ff-8ad3-ffcc3475e00f");
        formInfo.UpdateFormField(nameof(ProjectSettingsInfo.ProjectSettingsID), idField);

        // The column holds a WebsiteChannelID (the channel pages and IWebsiteChannelContext work
        // with website channels), so the reference points at cms.websitechannel: deleting a
        // website channel then deletes its settings instead of leaving orphaned rows.
        var formItem = new FormFieldInfo
        {
            Guid = new Guid("58dc593d-c554-4a6c-a6f8-ae565ce16c6f"),
            Name = nameof(ProjectSettingsInfo.ProjectSettingsChannelID),
            Visible = false,
            DataType = FieldDataType.Integer,
            Enabled = true,
            ReferenceToObjectType = WebsiteChannelInfo.OBJECT_TYPE,
            ReferenceType = ObjectDependencyEnum.Required
        };
        formInfo.AddFormItem(formItem);

        formItem = new FormFieldInfo
        {
            Guid = new Guid("6bb2deb7-adf1-41ee-960b-3db615841e20"),
            Name = nameof(ProjectSettingsInfo.ProjectSettingsGUID),
            DataType = FieldDataType.Guid,
            Enabled = true,
            AllowEmpty = false
        };
        formInfo.AddFormItem(formItem);

        formItem = new FormFieldInfo
        {
            Guid = new Guid("7dfe2573-784b-471d-b8e6-4b96731f5658"),
            Name = nameof(ProjectSettingsInfo.ProjectSettingsName),
            DataType = FieldDataType.Text,
            Size = 200,
            Enabled = true,
            AllowEmpty = false
        };
        formInfo.AddFormItem(formItem);

        formItem = new FormFieldInfo
        {
            Guid = new Guid("793d0f96-9561-40e5-9d1e-fbcb11c9cfd8"),
            Name = nameof(ProjectSettingsInfo.ProjectSettingsDisplayName),
            DataType = FieldDataType.Text,
            Size = 200,
            Enabled = true,
            AllowEmpty = false
        };
        formInfo.AddFormItem(formItem);

        formItem = new FormFieldInfo
        {
            Guid = new Guid("787b9063-796d-4841-911b-505b70284535"),
            Name = nameof(ProjectSettingsInfo.ProjectSettingsValue),
            DataType = FieldDataType.LongText,
            Enabled = true,
            AllowEmpty = true
        };
        formInfo.AddFormItem(formItem);

        SetFormDefinition(info, formInfo);

        if (info.HasChanged)
        {
            DataClassInfoProvider.SetDataClassInfo(info);
        }
    }

    /// <summary>
    /// Ensure that the form is not upserted with any existing form
    /// </summary>
    /// <param name="info"></param>
    /// <param name="form"></param>
    private static void SetFormDefinition(DataClassInfo info, FormInfo form)
    {
        if (info.ClassID > 0)
        {
            var existingForm = new FormInfo(info.ClassFormDefinition);
            // OverwriteExisting heals environments installed before the fixed field GUIDs and the
            // website channel reference existed (random GUIDs / cms.channel in the class definition).
            existingForm.CombineWithForm(form, new() { OverwriteExisting = true });
            info.ClassFormDefinition = existingForm.GetXmlDefinition();
        }
        else
        {
            info.ClassFormDefinition = form.GetXmlDefinition();
        }
    }

}
