# Create Project Settings

Generate strongly-typed project settings using the XperienceCommunity.ProjectSettings package.

## Required Information

Before generating code, ensure you have the following information. If any is missing, ask the user:

1. **Settings Name** (e.g., "Seo", "Email", "Analytics")
2. **Settings Properties** - List of properties with:
   - Property name
   - Property type (string, bool, int, IEnumerable<ContentItemReference>, etc.)
   - Form component (TextInput, TextArea, CheckBox, NumberInput, ContentItemSelector, etc.)
   - Label text
   - Default value
3. **Target Scopes** - Which admin pages to create:
   - [ ] Global Settings
   - [ ] Channel Settings
   - [ ] Both

---

## Code Generation Instructions

### 1. Settings Class

Create in: `{LogicProject}/Settings/{SettingsName}Settings.cs`

```csharp
using CMS.ContentEngine;
using Kentico.Xperience.Admin.Base.FormAnnotations;
using XperienceCommunity.ProjectSettings.Classes;

namespace {LogicProjectNamespace}.Settings;

public class {SettingsName}Settings : IProjectSettingsType
{
    public const string PAGE_SLUG = "{settings-slug}";

    public string SettingsSlug => PAGE_SLUG;
    public string SettingsName => "{SettingsName}.Settings";
    public string SettingsDisplayName => "{Settings Display Name}";
    public string SettingsExplanation => "{Description of these settings}";

    // Add properties here with appropriate form components:
    // [TextInputComponent(Label = "...", Order = 1)]
    // [TextAreaComponent(Label = "...", Order = 2)]
    // [CheckBoxComponent(Label = "...", Order = 3)]
    // [NumberInputComponent(Label = "...", Order = 4)]
    // [ContentItemSelectorComponent(ContentTypeName, Label = "...", MinimumItems = 0, MaximumItems = 1, Order = 5)]
    // [WebPageSelectorComponent(TreePath = "...", MaximumPages = 5, Label = "...", Order = 6)]
}
```

### 2. Global Settings Page (if needed)

Create in: `{WebProject}/UISettings/{SettingsName}GlobalSettingsPage.cs`

```csharp
using Kentico.Xperience.Admin.Base;
using Kentico.Xperience.Admin.Base.Forms;
using Kentico.Xperience.Admin.Base.Forms.Internal;
using XperienceCommunity.ProjectSettings;
using XperienceCommunity.ProjectSettings.Pages;
using {LogicProjectNamespace}.Settings;
using {WebProjectNamespace}.UISettings;

[assembly: UIPage(
    parentType: typeof(ProjectSettingsApplication),
    slug: {SettingsName}Settings.PAGE_SLUG,
    uiPageType: typeof({SettingsName}GlobalSettingsPage),
    name: "{Settings Display Name}",
    templateName: TemplateNames.EDIT,
    order: {order})]

namespace {WebProjectNamespace}.UISettings;

public class {SettingsName}GlobalSettingsPage(
    IFormItemCollectionProvider formItemCollectionProvider,
    IFormDataBinder formDataBinder)
    : GlobalSettingsEditPage<{SettingsName}Settings>(formItemCollectionProvider, formDataBinder)
{
}
```

### 3. Channel Settings Page (if needed)

Create in: `{WebProject}/UISettings/{SettingsName}ChannelSettingsPage.cs`

```csharp
using CMS.ContentEngine;
using CMS.DataEngine;
using Kentico.Xperience.Admin.Base;
using Kentico.Xperience.Admin.Base.Forms;
using Kentico.Xperience.Admin.Base.Forms.Internal;
using XperienceCommunity.ProjectSettings.Pages;
using {LogicProjectNamespace}.Settings;
using {WebProjectNamespace}.UISettings;

[assembly: UIPage(
    parentType: typeof(ProjectSettingsChannelEditSection),
    slug: {SettingsName}Settings.PAGE_SLUG,
    uiPageType: typeof({SettingsName}ChannelSettingsPage),
    name: "{Settings Display Name}",
    templateName: TemplateNames.EDIT,
    order: {order})]

namespace {WebProjectNamespace}.UISettings;

public class {SettingsName}ChannelSettingsPage(
    IFormItemCollectionProvider formItemCollectionProvider,
    IFormDataBinder formDataBinder,
    IInfoProvider<ChannelInfo> channelInfoProvider)
    : ChannelSettingsEditPage<{SettingsName}Settings>(formItemCollectionProvider, formDataBinder, channelInfoProvider)
{
}
```

### 4. Service Registration

Add to `Program.cs`:

```csharp
builder.Services.AddProjectSettings<{SettingsName}Settings>();
```

---

## Component Reference

### Components

| Component | Type | Example |
|-----------|------|---------|
| `[TextInputComponent]` | `string` | `public virtual string Title { get; set; } = "";` |
| `[TextAreaComponent]` | `string` | `public virtual string Description { get; set; } = "";` |
| `[CheckBoxComponent]` | `bool` | `public virtual bool IsEnabled { get; set; } = true;` |
| `[NumberInputComponent]` | `int?` | `public virtual int? MaxItems { get; set; } = 10;` |
| `[DecimalNumberInputComponent]` | `decimal?` | `public virtual decimal? Price { get; set; } = 0.0m;` |
| `[DropDownComponent]` | `string` | `public virtual string SelectedOption { get; set; } = "";` |
| `[RadioGroupComponent]` | `string` | `public virtual string Layout { get; set; } = "";` |
| `[RichTextEditorComponent]` | `string` | `public virtual string Content { get; set; } = "";` |
| `[CodeEditorComponent]` | `string` | `public virtual string CustomScript { get; set; } = "";` |
| `[PasswordComponent]` | `string` | `public virtual string ApiKey { get; set; } = "";` |
| `[DateInputComponent]` | `DateTime?` | `public virtual DateTime? StartDate { get; set; } = null;` |
| `[DateTimeInputComponent]` | `DateTime?` | `public virtual DateTime? ScheduledAt { get; set; } = null;` |
| `[ExtensionSelectorComponent]` | `string` | `public virtual string AllowedExtensions { get; set; } = "";` |

### Selectors

| Component | Type | Example |
|-----------|------|---------|
| `[ContentItemSelectorComponent]` | `IEnumerable<ContentItemReference>` | `public virtual IEnumerable<ContentItemReference> Images { get; set; } = [];` |
| `[WebPageSelectorComponent]` | `IEnumerable<WebPageRelatedItem>` | `public virtual IEnumerable<WebPageRelatedItem> Pages { get; set; } = new List<WebPageRelatedItem>();` |
| `[UrlSelectorComponent]` | `string` | `public virtual string Url { get; set; } = "";` |
| `[TagSelectorComponent]` | `IEnumerable<TagReference>` | `public virtual IEnumerable<TagReference> Tags { get; set; } = [];` |
| `[ObjectSelectorComponent]` | `IEnumerable<ObjectRelatedItem>` | `public virtual IEnumerable<ObjectRelatedItem> Users { get; set; } = new List<ObjectRelatedItem>();` |
| `[GeneralSelectorComponent]` | `IEnumerable<string>` | `public virtual IEnumerable<string> Options { get; set; } = [];` |
| `[EmailSelectorComponent]` | `IEnumerable<EmailRelatedItem>` | `public virtual IEnumerable<EmailRelatedItem> Emails { get; set; } = new List<EmailRelatedItem>();` |
| `[FormSelectorComponent]` | `IEnumerable<ObjectRelatedItem>` | `public virtual IEnumerable<ObjectRelatedItem> SelectedForm { get; set; } = new List<ObjectRelatedItem>();` |
| `[SmartFolderSelectorComponent]` | `SmartFolderReference` | `public virtual SmartFolderReference Folder { get; set; }` |
| `[ContentFolderSelectorComponent]` | `int` | `public virtual int FolderId { get; set; }` |

> **Namespace notes:**
> - Most attributes: `Kentico.Xperience.Admin.Base.FormAnnotations`
> - `WebPageSelectorComponent`: `Kentico.Xperience.Admin.Websites.FormAnnotations` (`WebPageRelatedItem` is in `CMS.Websites`)
> - `EmailSelectorComponent`: `Kentico.Xperience.Admin.DigitalMarketing.FormAnnotations`
> - `TagSelectorComponent` / `ContentItemSelectorComponent`: `Kentico.Xperience.Admin.Base.FormAnnotations` or `Kentico.Xperience.Admin.Content.FormAnnotations`

---

## Example Request

> Create settings for "Analytics" with:
> - TrackingId (string, TextInput, default: "")
> - EnableTracking (bool, CheckBox, default: false)
> - ExcludedPaths (string, TextArea, default: "/admin\n/api")
>
> Target: Both global and channel settings

---

## Checklist

- [ ] Settings class created with `IProjectSettingsType` interface
- [ ] All properties marked as `virtual`
- [ ] All properties have default values
- [ ] PAGE_SLUG constant defined and used consistently
- [ ] Global settings page created (if requested)
- [ ] Channel settings page created (if requested)
- [ ] Both pages use the same slug (if both are created)
- [ ] `AddProjectSettings<T>()` added to Program.cs
- [ ] Order values set appropriately for admin navigation
