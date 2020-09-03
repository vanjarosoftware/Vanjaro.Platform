using DotNetNuke.Common.Lists;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Profile;
using DotNetNuke.Services.Localization;
using System.Linq;

namespace Vanjaro.UXManager.Extensions.Block.Profile.Entities
{
    public class ProfileProperties
    {
        ListController listController = new ListController();
        public ProfilePropertyDefinition ProfilePropertyDefinition { get; set; }
        private string _PropertyValue;
        public string PropertyValue
        {
            get
            {
                string ControlType = Managers.ProfileManager.GetControlType(this.ProfilePropertyDefinition.DataType);
                if (!string.IsNullOrEmpty(this.ProfilePropertyDefinition.PropertyValue))
                {
                    if (ControlType == "List")
                    {
                        var ListEntries = listController.GetListEntryInfoItems(this.ProfilePropertyDefinition.PropertyName, "", PortalSettings.Current.PortalId).ToList();
                        if (ListEntries != null && ListEntries.Count > 0)
                        {
                            if (ListEntries.Where(x => !string.IsNullOrEmpty(x.Value) && x.Value == this.ProfilePropertyDefinition.PropertyValue).FirstOrDefault() != null)
                                _PropertyValue = ListEntries.Where(x => !string.IsNullOrEmpty(x.Value) && x.Value == this.ProfilePropertyDefinition.PropertyValue).Select(x => new { x.Text }).FirstOrDefault().Text;
                        }
                    }
                    else if (ControlType == "Country" || ControlType == "Region")
                    {
                        int EntryId = 0;
                        int.TryParse(this.ProfilePropertyDefinition.PropertyValue, out EntryId);
                        _PropertyValue = EntryId > 0 ? listController.GetListEntryInfo(EntryId).Text : string.Empty;
                    }
                    else if (ControlType == "TrueFalse")
                    {
                        _PropertyValue = this.ProfilePropertyDefinition.PropertyValue.ToLower() == "true" ? "Yes" : "No";
                    }
                    else
                        _PropertyValue = this.ProfilePropertyDefinition.PropertyValue;
                }
                return _PropertyValue;
            }
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
    }
}


