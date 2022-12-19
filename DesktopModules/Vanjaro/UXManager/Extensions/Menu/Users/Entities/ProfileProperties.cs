using DotNetNuke.Common.Lists;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Profile;
using DotNetNuke.Services.Localization;
using System.Collections.Generic;
using static Vanjaro.UXManager.Extensions.Menu.Users.Managers;

namespace Vanjaro.UXManager.Extensions.Menu.Users.Entities
{
    public class ProfileProperties
    {
        public CopyofProfilePropertyDefinition ProfilePropertyDefinition { get; set; }
        public IEnumerable<ListEntryInfo> ListEntries { get; set; }
        private string _ControlType;

        public string ControlType
        {
            get
            {
                if (ProfilePropertyDefinition != null)
                    _ControlType = UserManager.GetControlType(ProfilePropertyDefinition.DataType);
                return _ControlType;
            }
            set => _ControlType = value;
        }
        public string PropertyNameString
        {
            get
            {
                string _PropertyNameString = string.Empty;
                if (ProfilePropertyDefinition != null)
                    _PropertyNameString = Localization.GetString("ProfileProperties_" + ProfilePropertyDefinition.PropertyName, Components.Constants.DnnUserProfileResourcesFile, PortalSettings.Current.CultureCode);
                return !string.IsNullOrEmpty(_PropertyNameString) ? _PropertyNameString : ProfilePropertyDefinition.PropertyName;
            }
        }
        public string PropertyHelpString
        {
            get
            {
                string _PropertyHelpString = string.Empty;
                if (ProfilePropertyDefinition != null)
                    _PropertyHelpString = Localization.GetString("ProfileProperties_" + ProfilePropertyDefinition.PropertyName + ".Help", Components.Constants.DnnUserProfileResourcesFile, PortalSettings.Current.CultureCode);
                return !string.IsNullOrEmpty(_PropertyHelpString) ? _PropertyHelpString : ProfilePropertyDefinition.PropertyName;
            }
        }
        public string PropertyRequiredString
        {
            get
            {
                string _PropertyRequiredString = string.Empty;
                if (ProfilePropertyDefinition != null)
                    _PropertyRequiredString = Localization.GetString("ProfileProperties_" + ProfilePropertyDefinition.PropertyName + ".Required", Components.Constants.DnnUserProfileResourcesFile, PortalSettings.Current.CultureCode) ?? "";
                return _PropertyRequiredString;
            }
        }
        public string PropertyValidationString
        {
            get
            {
                string _PropertyValidationString = string.Empty;
                if (ProfilePropertyDefinition != null)
                    _PropertyValidationString = Localization.GetString("ProfileProperties_" + ProfilePropertyDefinition.PropertyName + ".Validation", Components.Constants.DnnUserProfileResourcesFile, PortalSettings.Current.CultureCode) ?? "";
                return _PropertyValidationString;
            }
        }
    }
}


