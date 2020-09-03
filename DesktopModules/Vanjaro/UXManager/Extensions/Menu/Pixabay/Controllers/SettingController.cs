using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using DotNetNuke.Web.Api;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using Vanjaro.Common.ASPNET.WebAPI;
using Vanjaro.Common.Engines.UIEngine;

namespace Vanjaro.UXManager.Extensions.Menu.Pixabay.Controllers
{
    [ValidateAntiForgeryToken]
    [AuthorizeAccessRoles(AccessRoles = "admin")]
    public class SettingController : UIEngineController
    {
        internal static List<IUIData> GetData(int portalId, UserInfo userInfo)
        {
            Dictionary<string, IUIData> Settings = new Dictionary<string, IUIData>();
            string apikey = PortalController.GetEncryptedString("Vanjaro.Integration.Pixabay", portalId, Config.GetDecryptionkey());
            bool HasApiKey = false;
            if (!string.IsNullOrEmpty(apikey))
            {
                HasApiKey = true;
            }

            Settings.Add("ApiKey", new UIData { Name = "ApiKey", Value = apikey });
            Settings.Add("HasApiKey", new UIData { Name = "HasApiKey", Options = HasApiKey });
            return Settings.Values.ToList();
        }

        [HttpPost]
        public bool Save(dynamic Data)
        {
            if (Vanjaro.Core.Providers.Pixabay.IsValid(Data.ToString()))
            {
                PortalController.UpdateEncryptedString(PortalSettings.PortalId, "Vanjaro.Integration.Pixabay", Data.ToString(), Config.GetDecryptionkey());
                return true;
            }
            return false;
        }

        [HttpGet]
        public string Delete()
        {
            PortalController.UpdateEncryptedString(PortalSettings.PortalId, "Vanjaro.Integration.Pixabay", string.Empty, Config.GetDecryptionkey());
            return string.Empty;
        }
        public override string AccessRoles()
        {
            return Factories.AppFactory.GetAccessRoles(UserInfo);
        }
    }
}