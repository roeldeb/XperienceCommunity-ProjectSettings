using System;
using System.Data;
using System.Collections.Generic;
using System.Linq;

using CMS;
using CMS.DataEngine;
using CMS.Helpers;

using XperienceCommunity.ProjectSettings.Classes;

[assembly: RegisterObjectType(typeof(ProjectSettingsInfo), ProjectSettingsInfo.OBJECT_TYPE)]

namespace XperienceCommunity.ProjectSettings.Classes
{
    /// <summary>
    /// Data container class for <see cref="ProjectSettingsInfo"/>.
    /// </summary>
    public partial class ProjectSettingsInfo : AbstractInfo<ProjectSettingsInfo, IInfoProvider<ProjectSettingsInfo>>, IInfoWithId, IInfoWithGuid
    {
        /// <summary>
        /// Object type.
        /// </summary>
        public const string OBJECT_TYPE = "xperiencecommunity.projectsettings";


        /// <summary>
        /// Type information.
        /// </summary>
        public static readonly ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(IInfoProvider<ProjectSettingsInfo>), OBJECT_TYPE, "XperienceCommunity.ProjectSettings", "ProjectSettingsID", null, "ProjectSettingsGUID", null, "ProjectSettingsDisplayName", null, null, null)
        {
            TouchCacheDependencies = true,
            DependsOn = new List<ObjectDependency>()
            {
                new ObjectDependency("ProjectSettingsChannelID", "cms.websitechannel", ObjectDependencyEnum.Required),
            },
        };


        /// <summary>
        /// Channel settings key ID.
        /// </summary>
        [DatabaseField]
        public virtual int ProjectSettingsID
        {
            get => ValidationHelper.GetInteger(GetValue(nameof(ProjectSettingsID)), 0);
            set => SetValue(nameof(ProjectSettingsID), value);
        }


        /// <summary>
        /// Channel settings key channel ID.
        /// </summary>
        [DatabaseField]
        public virtual int ProjectSettingsChannelID
        {
            get => ValidationHelper.GetInteger(GetValue(nameof(ProjectSettingsChannelID)), 0);
            set => SetValue(nameof(ProjectSettingsChannelID), value);
        }


        /// <summary>
        /// Channel settings key GUID.
        /// </summary>
        [DatabaseField]
        public virtual Guid ProjectSettingsGUID
        {
            get => ValidationHelper.GetGuid(GetValue(nameof(ProjectSettingsGUID)), Guid.Empty);
            set => SetValue(nameof(ProjectSettingsGUID), value);
        }


        /// <summary>
        /// Channel settings key name.
        /// </summary>
        [DatabaseField]
        public virtual string ProjectSettingsName
        {
            get => ValidationHelper.GetString(GetValue(nameof(ProjectSettingsName)), String.Empty);
            set => SetValue(nameof(ProjectSettingsName), value);
        }


        /// <summary>
        /// Channel settings key display name.
        /// </summary>
        [DatabaseField]
        public virtual string ProjectSettingsDisplayName
        {
            get => ValidationHelper.GetString(GetValue(nameof(ProjectSettingsDisplayName)), String.Empty);
            set => SetValue(nameof(ProjectSettingsDisplayName), value);
        }


        /// <summary>
        /// Channel settings key value.
        /// </summary>
        [DatabaseField]
        public virtual string ProjectSettingsValue
        {
            get => ValidationHelper.GetString(GetValue(nameof(ProjectSettingsValue)), String.Empty);
            set => SetValue(nameof(ProjectSettingsValue), value, String.Empty);
        }


        /// <summary>
        /// Deletes the object using appropriate provider.
        /// </summary>
        protected override void DeleteObject()
        {
            Provider.Delete(this);
        }


        /// <summary>
        /// Updates the object using appropriate provider.
        /// </summary>
        protected override void SetObject()
        {
            Provider.Set(this);
        }


        /// <summary>
        /// Creates an empty instance of the <see cref="ProjectSettingsInfo"/> class.
        /// </summary>
        public ProjectSettingsInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Creates a new instances of the <see cref="ProjectSettingsInfo"/> class from the given <see cref="DataRow"/>.
        /// </summary>
        /// <param name="dr">DataRow with the object data.</param>
        public ProjectSettingsInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }
    }
}
