using Dnn.PersonaBar.Library.Common;
using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Vanjaro.Common.Utilities;

namespace Vanjaro.UXManager.Extensions.Block.Profile.Entities
{
    public class Profile
    {
        DotNetNuke.Entities.Users.UserInfo _userInfo = null;
        Dictionary<string, List<Entities.ProfileProperties>> _Fields = null;
        public bool IsAuthenticated { get; set; }
        public bool IsAllowEdit { get { return PortalSettings.Current.UserId != -1 && PortalSettings.Current.UserId == UserInfo.UserID ? true : false; } }
        public UserInfo UserInfo
        {
            get
            {
                if (_userInfo == null)
                {
                    int _UserId = Null.NullInteger;
                    if (HttpContext.Current.Request.QueryString["userid"] != null)
                        // Use Int32.MaxValue as invalid UserId
                        _UserId = Int32.TryParse(HttpContext.Current.Request.QueryString["userid"], out _UserId) ? _UserId : Int32.MaxValue;
                    if (_UserId > 0)
                        _userInfo = UserController.Instance.GetUserById(PortalSettings.Current.PortalId, _UserId);
                    if (_userInfo == null)
                        _userInfo = UserController.Instance.GetCurrentUserInfo();
                }
                return _userInfo;
            }
        }
        public string PhotoURL => UserInfo.Profile.PhotoURL.Contains("no_avatar.gif") ? Vanjaro.Common.Utilities.UserUtils.GetProfileImage(PortalSettings.Current.PortalId, UserInfo.UserID, UserInfo.Email) : Utilities.GetProfileAvatar(UserInfo.UserID);
        public Dictionary<string, List<Entities.ProfileProperties>> Fields
        {
            get
            {
                if (_Fields == null)
                {
                    _Fields = new Dictionary<string, List<ProfileProperties>>();
                    foreach (var d in Managers.ProfileManager.GetLocalizedCategories(UserInfo.Profile.ProfileProperties, this.UserInfo))
                    {
                        _Fields.Add(d.Value, Managers.ProfileManager.GetProfileFields(UserInfo).Where(x => x.ProfilePropertyDefinition.PropertyCategory == d.Key && !string.IsNullOrEmpty(x.PropertyValue) && Managers.ProfileManager.IsExistsDataType(x.ProfilePropertyDefinition.DataType)).ToList());
                    }
                }
                return _Fields;
            }
        }
        public string UserExtensionURL => ServiceProvider.NavigationManager.NavigateURL("", "mid=0", "icp=true", "guid=fa7ca744-1677-40ef-86b2-ca409c5c6ed3#!/setting?uid=" + UserInfo.UserID);
    }
}