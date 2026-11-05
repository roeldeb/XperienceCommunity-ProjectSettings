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
    private IReadOnlyModeProvider? readOnlyModeProvider;

    protected override void OnInit(ModuleInitParameters parameters)
    {
        base.OnInit(parameters);

        var services = parameters.Services;

        installer = services.GetService<ISettingsModuleInstaller>();
        readOnlyModeProvider = services.GetService<IReadOnlyModeProvider>();

        if (installer != null)
        {
            ApplicationEvents.Initialized.Execute += InitializeModule;
        }
    }

    private void InitializeModule(object? sender, EventArgs e)
    {
        // Read-only (zero-downtime) deployments serve a frozen database snapshot where every
        // write throws ReadOnlyModeException. Skip the installation; it runs once the updated
        // instance starts in normal (writable) mode.
        if (readOnlyModeProvider?.IsReadOnly == true)
        {
            return;
        }

        installer?.Install();
    }
}
