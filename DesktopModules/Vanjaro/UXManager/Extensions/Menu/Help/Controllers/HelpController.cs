using DotNetNuke.Entities.Users;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Vanjaro.Common.ASPNET.WebAPI;
using Vanjaro.Common.Engines.UIEngine;


namespace Vanjaro.UXManager.Extensions.Menu.Help.Controllers
{
    [AuthorizeAccessRoles(AccessRoles = "host")]
    public class HelpController : UIEngineController
    {

#if RELEASE        
        private static string VanjaroAzureURL = "https://vanjaroplatform.blob.core.windows.net/platform/support/videos.html";
        private static string OriginURL = "https://vanjaroplatform.blob";
#else        
        private static string VanjaroAzureURL = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Authority + "/desktopmodules/vanjaro/uxmanager/extensions/menu/help/resources/help/videos.html";
        private static string OriginURL = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Authority;
#endif

        internal static List<IUIData> GetData(UserInfo userInfo, string identifier, Dictionary<string, string> parameters)
        {
            Dictionary<string, IUIData> Settings = new Dictionary<string, IUIData>();
            Settings.Add("AuthenticatedURL", new UIData { Name = "AuthenticatedURL", Value = VanjaroAzureURL });
            Settings.Add("OriginURL", new UIData { Name = "OriginURL", Value = OriginURL });
            return Settings.Values.ToList();
        }

        public override string AccessRoles()
        {
            return Factories.AppFactory.GetAccessRoles(UserInfo);
        }
    }
}