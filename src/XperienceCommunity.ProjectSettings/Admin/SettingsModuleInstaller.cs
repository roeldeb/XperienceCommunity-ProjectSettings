using CMS.ContentEngine;
using CMS.DataEngine;
using CMS.FormEngine;
using CMS.Modules;

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

        var formItem = new FormFieldInfo
        {
            Name = nameof(ProjectSettingsInfo.ProjectSettingsChannelID),
            Visible = false,
            DataType = FieldDataType.Integer,
            Enabled = true,
            ReferenceToObjectType = ChannelInfo.OBJECT_TYPE,
            ReferenceType = ObjectDependencyEnum.Required
        };
        formInfo.AddFormItem(formItem);

        formItem = new FormFieldInfo
        {
            Name = nameof(ProjectSettingsInfo.ProjectSettingsGUID),
            DataType = FieldDataType.Guid,
            Enabled = true,
            AllowEmpty = false
        };
        formInfo.AddFormItem(formItem);

        formItem = new FormFieldInfo
        {
            Name = nameof(ProjectSettingsInfo.ProjectSettingsName),
            DataType = FieldDataType.Text,
            Size = 200,
            Enabled = true,
            AllowEmpty = false
        };
        formInfo.AddFormItem(formItem);

        formItem = new FormFieldInfo
        {
            Name = nameof(ProjectSettingsInfo.ProjectSettingsDisplayName),
            DataType = FieldDataType.Text,
            Size = 200,
            Enabled = true,
            AllowEmpty = false
        };
        formInfo.AddFormItem(formItem);

        formItem = new FormFieldInfo
        {
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
            existingForm.CombineWithForm(form, new());
            info.ClassFormDefinition = existingForm.GetXmlDefinition();
        }
        else
        {
            info.ClassFormDefinition = form.GetXmlDefinition();
        }
    }

}
