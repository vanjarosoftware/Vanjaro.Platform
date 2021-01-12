using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using DotNetNuke.Web.Api;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Web.Http;
using Vanjaro.Common.ASPNET.WebAPI;
using Vanjaro.Common.Engines.UIEngine;
using Vanjaro.Core.Entities.Theme;
using Vanjaro.UXManager.Extensions.Apps.ThemeBuilder.Factories;
using Vanjaro.UXManager.Library.Common;
using static Vanjaro.Core.Managers;

namespace Vanjaro.UXManager.Extensions.Apps.ThemeBuilder.Controllers
{
    [AuthorizeAccessRoles(AccessRoles = "admin")]
    public class SettingsController : UIEngineController
    {
        internal static List<IUIData> GetAllData(string identifier, Dictionary<string, string> parameters)
        {
            Dictionary<string, IUIData> Settings = new Dictionary<string, IUIData>();
            string ThemeName = string.Empty;
            string Category = string.Empty;
            string Guid = string.Empty;
            if (parameters.Count > 0)
            {
                if (parameters.ContainsKey("themename"))
                {
                    ThemeName = parameters["themename"];
                }

                if (parameters.ContainsKey("cat"))
                {
                    Category = parameters["cat"];
                }

                if (parameters.ContainsKey("guid"))
                {
                    Guid = parameters["guid"];
                }
            }
            Settings.Add("MarkUp", new UIData { Name = "MarkUp", Value = Core.Managers.ThemeManager.GetMarkUp(identifier, Guid) });
            Settings.Add("Category", new UIData { Name = "Category", Value = Category });
            Settings.Add("Guid", new UIData { Name = "Guid", Value = Guid });
            Settings.Add("ThemeName", new UIData { Name = "ThemeName", Value = ThemeName });
            if (identifier.ToLower() == "setting_manage")
            {
                bool DeveloperMode = true;
                ThemeEditorWrapper themeEditorWrapper = Core.Managers.ThemeManager.GetThemeEditors(PortalSettings.Current.PortalId, Guid);
                if (themeEditorWrapper != null)
                {
                    DeveloperMode = themeEditorWrapper.DeveloperMode;
                }

                Settings.Add("DeveloperMode", new UIData { Name = "DeveloperMode", Value = DeveloperMode.ToString().ToLower() });
                Settings.Add("Fonts", new UIData { Name = "Fonts", Options = Core.Managers.ThemeManager.GetFonts(PortalSettings.Current.PortalId, Guid) });
                Settings.Add("Font", new UIData { Name = "Font", Options = new ThemeFont() });
            }
            return Settings.Values.ToList();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public dynamic Delete(string Category, string Guid)
        {
            dynamic result = new ExpandoObject();
            result.IsSuccess = false;
            if (Core.Managers.ThemeManager.Delete(Guid, Category, null))
            {
                result.ManageMarkup = Core.Managers.ThemeManager.GetMarkUp(AppFactory.Identifier.setting_manage.ToString(), Guid);
                result.IsSuccess = true;
            }
            return result;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public dynamic Delete(string Category, string SubCategory, string Guid)
        {
            dynamic result = new ExpandoObject();
            result.IsSuccess = false;
            if (Core.Managers.ThemeManager.Delete(Guid, Category, SubCategory))
            {
                result.ManageMarkup = Core.Managers.ThemeManager.GetMarkUp(AppFactory.Identifier.setting_manage.ToString(), Guid);
                result.IsSuccess = true;
            }
            return result;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Save(string Guid, ThemeEditorData Data)
        {
            ActionResult actionResult = new ActionResult();
            try
            {
                Core.Managers.ThemeManager.Save(Guid, Data.ThemeEditorValues);
                Core.Managers.ThemeManager.ProcessScss(PortalSettings.Current.PortalId, true);
                actionResult.Data = Core.Managers.ThemeManager.GetMarkUp(AppFactory.Identifier.setting_settings.ToString(), Guid);
                actionResult.IsSuccess = true;
            }
            catch (Exception ex)
            {
                ExceptionManager.LogException(ex);
                actionResult.Message = ex.Message;
                actionResult.Data = Core.Managers.ThemeManager.GetMarkUp(AppFactory.Identifier.setting_settings.ToString(), Guid);
                actionResult.IsSuccess = false;
            }
            return actionResult;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UpdateFont(string Guid, ThemeFont Data)
        {
            ActionResult actionResult = new ActionResult();
            dynamic result = new ExpandoObject();
            try
            {
                Core.Managers.ThemeManager.UpdateFonts(Guid, Data);
                result.Fonts = Core.Managers.ThemeManager.GetFonts(PortalSettings.Current.PortalId, Guid);
            }
            catch (Exception)
            {

            }
            actionResult.Data = result;
            return actionResult;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Deletefont(string Guid, ThemeFont Data)
        {
            ActionResult actionResult = new ActionResult();
            dynamic result = new ExpandoObject();
            try
            {
                Core.Managers.ThemeManager.DeleteFonts(Guid, Data);
                result.Fonts = Core.Managers.ThemeManager.GetFonts(PortalSettings.Current.PortalId, Guid);
            }
            catch (Exception)
            {

            }
            actionResult.Data = result;
            return actionResult;
        }

        [HttpGet]
        [ValidateAntiForgeryToken]
        public List<ThemeFont> GetFonts(string Guid)
        {
            return Core.Managers.ThemeManager.GetFonts(PortalSettings.Current.PortalId, Guid);
        }

        public override string AccessRoles()
        {
            return AppFactory.GetAccessRoles(UserInfo);
        }
    }
}