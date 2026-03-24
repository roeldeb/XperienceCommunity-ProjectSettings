using CMS.Base;
using CMS.Core;
using CMS.DataEngine;

using Microsoft.Extensions.DependencyInjection;

using XperienceCommunity.ProjectSettings.Admin;

[assembly: CMS.RegisterModule(typeof(SettingsModule))]
namespace XperienceCommunity.ProjectSettings.Admin;

internal class SettingsModule() : Module(nameof(SettingsModule))
{
    private ISettingsModuleInstaller? installer;

    protected override void OnInit(ModuleInitParameters parameters)
    {
        base.OnInit(parameters);

        var services = parameters.Services;

        installer = services.GetService<ISettingsModuleInstaller>();

        if (installer != null)
        {
            ApplicationEvents.Initialized.Execute += InitializeModule;
        }
    }

    private void InitializeModule(object? sender, EventArgs e) => installer?.Install();
}
