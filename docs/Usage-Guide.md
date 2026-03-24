# Usage Guide

This guide provides full documentation for the **XperienceCommunity.ProjectSettings** library.

## Table of Contents

- [Installation](#installation)
- [Creating a Settings Class](#creating-a-settings-class)
- [Service Registration](#service-registration)
- [Creating Admin UI Pages](#creating-admin-ui-pages)
  - [Global Settings Page](#global-settings-page)
  - [Channel Settings Page](#channel-settings-page)
  - [Using Both Global and Channel Settings](#using-both-global-and-channel-settings)
- [Using Settings in Your Code](#using-settings-in-your-code)
- [Direct Service Access](#direct-service-access)
- [Settings Resolution](#settings-resolution)
- [Admin Navigation](#admin-navigation)
- [Key Types](#key-types)
- [Available Form Components](#available-form-components)
- [Database Structure](#database-structure)
- [Best Practices](#best-practices)
- [Troubleshooting](#troubleshooting)

---

## Installation

```powershell
dotnet add package XperienceCommunity.ProjectSettings
```

## Creating a Settings Class

Create a class that implements `IProjectSettingsType`:

```csharp
using XperienceCommunity.ProjectSettings.Classes;
using Kentico.Xperience.Admin.Base.FormAnnotations;

namespace MyProject.Settings;

public class SeoSettings : IProjectSettingsType
{
    public string SettingsSlug => "seo-settings";
    public string SettingsName => "Seo.Settings";
    public string SettingsDisplayName => "SEO Settings";
    public string SettingsExplanation => "Configure SEO settings for your website.";

    [TextInputComponent(Label = "Meta Title Suffix", Order = 1)]
    public virtual string MetaTitleSuffix { get; set; } = "";

    [TextAreaComponent(Label = "Default Meta Description", Order = 2)]
    public virtual string DefaultMetaDescription { get; set; } = "";

    [CheckBoxComponent(Label = "Enable Open Graph", Order = 3)]
    public virtual bool EnableOpenGraph { get; set; } = true;
}
```

## Service Registration

```csharp
using XperienceCommunity.ProjectSettings;

var builder = WebApplication.CreateBuilder(args);

// Register each settings type you want to use
builder.Services.AddProjectSettings<SeoSettings>();
builder.Services.AddProjectSettings<EmailSettings>();
builder.Services.AddProjectSettings<AnalyticsSettings>();

var app = builder.Build();
app.Run();
```

The `AddProjectSettings<T>()` method:

- Registers `IOptions<T>` for dependency injection
- Automatically registers the module installer (creates database table on first run)
- Automatically registers `IProjectSettingsOptionsService` for direct access
- Is idempotent — safe to call multiple times

## Creating Admin UI Pages

### Global Settings Page

Inherit from `GlobalSettingsEditPage<T>`:

```csharp
using Kentico.Xperience.Admin.Base;
using Kentico.Xperience.Admin.Base.Forms;
using Kentico.Xperience.Admin.Base.Forms.Internal;
using XperienceCommunity.ProjectSettings;
using XperienceCommunity.ProjectSettings.Pages;
using MyProject.Settings;
using MyProject.UISettings;

[assembly: UIPage(
    parentType: typeof(ProjectSettingsApplication),
    slug: "seo-settings",
    uiPageType: typeof(SeoGlobalSettingsPage),
    name: "SEO Settings",
    templateName: TemplateNames.EDIT,
    order: 1)]

namespace MyProject.UISettings;

public class SeoGlobalSettingsPage(
    IFormItemCollectionProvider formItemCollectionProvider,
    IFormDataBinder formDataBinder)
    : GlobalSettingsEditPage<SeoSettings>(formItemCollectionProvider, formDataBinder)
{
}
```

### Channel Settings Page

Inherit from `ChannelSettingsEditPage<T>`:

```csharp
using CMS.ContentEngine;
using CMS.DataEngine;
using Kentico.Xperience.Admin.Base;
using Kentico.Xperience.Admin.Base.Forms;
using Kentico.Xperience.Admin.Base.Forms.Internal;
using XperienceCommunity.ProjectSettings.Pages;
using MyProject.Settings;
using MyProject.UISettings;

[assembly: UIPage(
    parentType: typeof(ProjectSettingsChannelEditSection),
    slug: "seo-settings",
    uiPageType: typeof(SeoChannelSettingsPage),
    name: "SEO Settings",
    templateName: TemplateNames.EDIT,
    order: 1)]

namespace MyProject.UISettings;

public class SeoChannelSettingsPage(
    IFormItemCollectionProvider formItemCollectionProvider,
    IFormDataBinder formDataBinder,
    IInfoProvider<ChannelInfo> channelInfoProvider)
    : ChannelSettingsEditPage<SeoSettings>(formItemCollectionProvider, formDataBinder, channelInfoProvider)
{
}
```

### Using Both Global and Channel Settings

You can use the same settings class for both global and channel-specific admin pages. This enables global defaults with per-channel overrides.

**Important:** When creating both admin pages for the same settings class, use the **same `slug`** value in both `UIPage` attributes.

```csharp
public class EmailSettings : IProjectSettingsType
{
    public const string PAGE_SLUG = "email-settings";

    public string SettingsSlug => PAGE_SLUG;
    public string SettingsName => "Email.Settings";
    public string SettingsDisplayName => "Email Settings";
    public string SettingsExplanation => "Configure email settings.";

    [TextInputComponent(Label = "From Email", Order = 1)]
    public virtual string FromEmail { get; set; } = "noreply@example.com";
}
```

Create both admin pages using the same slug:

```csharp
// Global page
[assembly: UIPage(
    parentType: typeof(ProjectSettingsApplication),
    slug: EmailSettings.PAGE_SLUG,
    uiPageType: typeof(EmailGlobalSettingsPage),
    name: "Email Settings",
    templateName: TemplateNames.EDIT,
    order: 1)]

// Channel page
[assembly: UIPage(
    parentType: typeof(ProjectSettingsChannelEditSection),
    slug: EmailSettings.PAGE_SLUG,
    uiPageType: typeof(EmailChannelSettingsPage),
    name: "Email Settings",
    templateName: TemplateNames.EDIT,
    order: 1)]
```

Register once in Program.cs:

```csharp
builder.Services.AddProjectSettings<EmailSettings>();
```

## Using Settings in Your Code

```csharp
using Microsoft.Extensions.Options;

public class SeoService
{
    private readonly SeoSettings _settings;

    public SeoService(IOptions<SeoSettings> settings)
    {
        _settings = settings.Value;
    }

    public string GetPageTitle(string baseTitle)
    {
        return $"{baseTitle} {_settings.MetaTitleSuffix}";
    }
}
```

## Direct Service Access

For scenarios where you need more control (e.g., retrieving settings for a specific channel):

```csharp
using XperienceCommunity.ProjectSettings.Services;

public class MultiChannelService
{
    private readonly IProjectSettingsOptionsService _settingsService;

    public MultiChannelService(IProjectSettingsOptionsService settingsService)
    {
        _settingsService = settingsService;
    }

    public void ProcessChannel(int channelId)
    {
        // Get settings for a specific channel (with fallback to global)
        var settings = _settingsService.GetSettings<SeoSettings>(channelId);

        // Get global settings explicitly
        var globalSettings = _settingsService.GetSettings<SeoSettings>(0);
    }
}
```

## Settings Resolution

When you inject `IOptions<T>`, settings are resolved automatically based on the current website channel context:

1. **Channel-specific settings** are checked first (based on `IWebsiteChannelContext.WebsiteChannelID`)
2. **Global settings** (channelId = 0) are used as fallback
3. **Default values** from the class are used if nothing is configured

## Admin Navigation

- **Global Settings:** Settings → Project Settings → [Your Settings Page]
- **Channel Settings:** Settings → Project Settings → Channel Settings → [Select Channel] → [Your Settings Page]

### Admin UI Features

**Global Settings Page:**

- Shows warning when using developer defaults (no saved settings)
- "Create override" button to save initial settings
- Shows links to channels that have overrides

**Channel Settings Page:**

- Shows warning when overriding global settings
- "Reset to default" button to remove channel-specific settings
- "Create override" button to create channel-specific settings
- Only supports Website channels

## Key Types

| Type                                | Namespace                                     | Description                                 |
| ----------------------------------- | --------------------------------------------- | ------------------------------------------- |
| `IProjectSettingsType`              | `XperienceCommunity.ProjectSettings.Classes`  | Interface for settings classes              |
| `GlobalSettingsEditPage<T>`         | `XperienceCommunity.ProjectSettings.Pages`    | Base class for global settings admin pages  |
| `ChannelSettingsEditPage<T>`        | `XperienceCommunity.ProjectSettings.Pages`    | Base class for channel settings admin pages |
| `ProjectSettingsApplication`        | `XperienceCommunity.ProjectSettings`          | Parent for global settings pages            |
| `ProjectSettingsChannelEditSection` | `XperienceCommunity.ProjectSettings.Pages`    | Parent for channel settings pages           |
| `IProjectSettingsOptionsService`    | `XperienceCommunity.ProjectSettings.Services` | Service for direct settings access          |

## Available Form Components

| Attribute                        | Description                                      |
| -------------------------------- | ------------------------------------------------ |
| `[TextInputComponent]`           | Single-line text input                           |
| `[TextAreaComponent]`            | Multi-line text area                             |
| `[NumberInputComponent]`         | Numeric input                                    |
| `[CheckBoxComponent]`            | Boolean checkbox                                 |
| `[UrlSelectorComponent]`         | URL/asset selector                               |
| `[DropDownComponent]`            | Dropdown selection                               |
| `[RichTextComponent]`            | Rich text editor                                 |
| `[ContentItemSelectorComponent]` | Content item selector (images, reusable content) |

## Database Structure

Settings are stored in the `XperienceCommunity_ProjectSettings` table (created automatically on first application start):

| Column                       | Description                                      |
| ---------------------------- | ------------------------------------------------ |
| `ProjectSettingsChannelID`   | 0 for global settings, or the website channel ID |
| `ProjectSettingsName`        | Unique identifier (from `SettingsName`)          |
| `ProjectSettingsValue`       | JSON serialized settings object                  |
| `ProjectSettingsDisplayName` | Display name in admin UI                         |

## Best Practices

1. **Naming Convention:** Use descriptive, hierarchical names for `SettingsName` (e.g., `"Email.Smtp.Settings"`)
2. **Default Values:** Always provide sensible defaults in your settings classes
3. **SettingsSlug:** Must match the `slug` in your `UIPage` attribute for admin UI links to work
4. **Documentation:** Use `SettingsExplanation` to provide helpful context for administrators
5. **Virtual Properties:** Mark properties as `virtual` to support proxy generation if needed
6. **Shared Slugs:** When using both global and channel admin pages, use the same `slug` value

## Troubleshooting

### Settings Not Appearing in Admin

- Ensure your settings class implements `IProjectSettingsType`
- Verify the admin UI page has the correct `parentType` and base class
- Rebuild and restart the application

### Settings Returning Default Values

- Check that settings have been saved in the admin interface
- Verify `SettingsName` matches between your class and database

### Channel Settings Always Returning Global Values

- Verify channel-specific settings have been saved
- Check that `IWebsiteChannelContext` is resolving the current channel
