using DotNetNuke.Web.Api;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using Vanjaro.Common.ASPNET.WebAPI;
using Vanjaro.Common.Engines.UIEngine;
using Vanjaro.UXManager.Extensions.Menu.Languages.Managers;
using Vanjaro.UXManager.Library.Common;

namespace Vanjaro.UXManager.Extensions.Menu.Languages.Controllers
{
    [AuthorizeAccessRoles(AccessRoles = "host")]
    [ValidateAntiForgeryToken]
    public class AddController : UIEngineController
    {
        internal static List<IUIData> GetData(int portalId)
        {
            Dictionary<string, IUIData> Settings = new Dictionary<string, IUIData>();
            string language = string.Empty;
            List<LanguageRequest> Languages = LanguagesManager.GetAllLanguages(false);
            if (Languages.Count > 0)
            {
                language = Languages.FirstOrDefault().Code;
            }

            Settings.Add("Languages", new UIData { Name = "Languages", Options = Languages, OptionsValue = "Code", OptionsText = "DisplayName", Value = language });
            return Settings.Values.ToList();
        }
        [HttpGet]
        public ActionResult GetAll(bool IsNativeName)
        {
            ActionResult result = new ActionResult
            {
                Data = LanguagesManager.GetAllLanguages(IsNativeName),
                IsSuccess = true
            };
            return result;
        }

        [HttpGet]
        public ActionResult Update(string Code)
        {
            return LanguagesManager.Update(PortalSettings, UserInfo, Code);
        }
        public override string AccessRoles()
        {
            return Factories.AppFactory.GetAccessRoles(UserInfo);
        }
    }
}