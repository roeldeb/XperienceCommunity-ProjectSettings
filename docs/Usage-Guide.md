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
  - [Components](#components)
  - [Selectors](#selectors)
  - [WebPageSelectorComponent Example](#webpageselectorcomponent-example)
- [Generate Code with GitHub Copilot](#generate-code-with-github-copilot)
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
using CMS.ContentEngine;
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

    [ContentItemSelectorComponent(
        ImageContent.CONTENT_TYPE_NAME,
        Label = "Logo",
        MinimumItems = 0,
        MaximumItems = 1,
        Order = 4)]
    public virtual IEnumerable<ContentItemReference> LogoImage { get; set; } = [];
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

The stored value is cached per settings type and channel. The cache is cleared automatically whenever a
settings record is saved or deleted through the admin UI (or the Kentico API), also on other web farm
instances. Deleting a website channel deletes its channel-specific settings.

## Admin Navigation

- **Global Settings:** Settings → Project Settings → [Your Settings Page]
- **Channel Settings:** Settings → Project Settings → Channel Settings → [Select Channel] → [Your Settings Page]

### Admin UI Features

Both admin pages base their state purely on whether a settings **record** exists — not on
whether the stored values happen to match the developer defaults. This means the editable form
only appears once you have explicitly created an override, and the callouts always describe
which values are currently in effect. Creating or resetting an override reloads the page, so the
form and callouts update immediately without a manual browser refresh.

**Global Settings Page:**

- When no global record exists, the form is **disabled** and previews the developer defaults. A
  "Using developer defaults" callout with a **Create override** button lets you store an initial
  record so the settings become editable.
- When a global record exists, a **Reset to developer defaults** page action deletes the record
  (with a destructive confirmation dialog) so the system falls back to the developer defaults.
- Lists the channels that have their own settings (as links). When creating global settings, it
  notes that those channels already override the global values and will not be affected.

**Channel Settings Page:**

- When no channel record exists, the form is **disabled** and previews the effective values — the
  global settings when they exist, otherwise the developer defaults. A **Create override** button
  seeds a new channel record from those effective values, so creating an override does not change
  the values already in effect.
- When a channel record exists, a **Reset** page action removes it (with a destructive
  confirmation dialog). It is labeled **Reset to global settings** when global settings exist,
  or **Reset to developer defaults** when they do not.
- An "Overriding global settings" callout links back to the global settings page when the channel
  record overrides global values.
- Only supports Website channels.

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

### Components

| Attribute | C# Type | Description |
|-----------|---------|-------------|
| `[TextInputComponent]` | `string` | Single-line text input |
| `[TextAreaComponent]` | `string` | Multi-line text area |
| `[NumberInputComponent]` | `int?` | Integer number input |
| `[DecimalNumberInputComponent]` | `decimal?` | Decimal number input |
| `[CheckBoxComponent]` | `bool` | Boolean checkbox |
| `[DropDownComponent]` | `string` | Dropdown selection (single option) |
| `[RadioGroupComponent]` | `string` | Radio button group (single selection) |
| `[RichTextEditorComponent]` | `string` | Rich text editor (HTML) |
| `[CodeEditorComponent]` | `string` | Code editor with syntax highlighting |
| `[PasswordComponent]` | `string` | Password input (obscured text) |
| `[DateInputComponent]` | `DateTime?` | Date selector |
| `[DateTimeInputComponent]` | `DateTime?` | Date and time selector |
| `[ExtensionSelectorComponent]` | `string` | Allowed file extensions selector |

### Selectors

| Attribute | C# Type | Description |
|-----------|---------|-------------|
| `[ContentItemSelectorComponent]` | `IEnumerable<ContentItemReference>` | Content item selector (pages, images, reusable content) |
| `[WebPageSelectorComponent]` | `IEnumerable<WebPageRelatedItem>` | Web page selector (pages from website channel content tree) |
| `[UrlSelectorComponent]` | `string` | URL selector (external URL or page from content tree) |
| `[TagSelectorComponent]` | `IEnumerable<TagReference>` | Taxonomy tag selector |
| `[ObjectSelectorComponent]` | `IEnumerable<ObjectRelatedItem>` | Database object selector (users, etc.) |
| `[GeneralSelectorComponent]` | `IEnumerable<string>` | Custom data selector with search |
| `[EmailSelectorComponent]` | `IEnumerable<EmailRelatedItem>` | Email selector (from email channels) |
| `[FormSelectorComponent]` | `IEnumerable<ObjectRelatedItem>` | Form selector |
| `[SmartFolderSelectorComponent]` | `SmartFolderReference` | Smart folder selector |
| `[ContentFolderSelectorComponent]` | `int` | Content folder selector |

> **Namespace notes:**
> - Most attributes: `Kentico.Xperience.Admin.Base.FormAnnotations`
> - `WebPageSelectorComponent`: `Kentico.Xperience.Admin.Websites.FormAnnotations` (`WebPageRelatedItem` is in `CMS.Websites`)
> - `EmailSelectorComponent`: `Kentico.Xperience.Admin.DigitalMarketing.FormAnnotations`
> - `TagSelectorComponent` / `ContentItemSelectorComponent`: `Kentico.Xperience.Admin.Base.FormAnnotations` or `Kentico.Xperience.Admin.Content.FormAnnotations`

### WebPageSelectorComponent Example

The `WebPageSelectorComponent` enables users to select pages from a website channel's content tree. It returns `IEnumerable<WebPageRelatedItem>` objects identified by page GUIDs.

**Configuration properties:**
- **TreePath** – limits selection to a subtree (e.g., `"/Articles"`)
- **MaximumPages** – max selectable pages (default: 1, use 0 for unlimited)
- **Sortable** – enables ordering of selected pages
- **ItemModifierType** – a type implementing `IWebPagePanelItemModifier` to disable specific pages from selection

```csharp
using CMS.Websites;
using Kentico.Xperience.Admin.Websites;
using Kentico.Xperience.Admin.Websites.FormAnnotations;

[WebPageSelectorComponent(
    TreePath = "/Articles",
    MaximumPages = 5,
    // Built-in modifier that prevents selection of folders
    // and pages whose content type is not included in routing
    ItemModifierType = typeof(WebPagesWithUrlWebPagePanelItemModifier),
    Label = "Pages",
    Order = 1)]
public virtual IEnumerable<WebPageRelatedItem> Pages { get; set; } = new List<WebPageRelatedItem>();
```

> **Note:** The `WebPageSelectorComponent` attribute is in the `Kentico.Xperience.Admin.Websites.FormAnnotations` namespace (not `Kentico.Xperience.Admin.Base.FormAnnotations`).

## Generate Code with GitHub Copilot

Use the included [GitHub Copilot prompt](./../.github/prompts/create-project-settings.prompt.md) to quickly scaffold new settings. Copy it to your project's `.github/prompts/` directory or use it directly in Copilot Chat.

### Example Copilot Request

```
Create project settings using XperienceCommunity.ProjectSettings with:

Settings Name: Newsletter
Properties:
- FromEmail (string, TextInput, default: "newsletter@example.com")
- FromName (string, TextInput, default: "Newsletter")
- EnableDoubleOptIn (bool, CheckBox, default: true)
- WelcomeMessage (string, RichText, default: "")

Target: Both global and channel settings
```

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
