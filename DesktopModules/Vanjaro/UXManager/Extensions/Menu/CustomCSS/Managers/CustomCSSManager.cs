using DotNetNuke.Common;
using DotNetNuke.Entities.Controllers;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Services.Localization;
using DotNetNuke.Web.Client;
using DotNetNuke.Web.Client.ClientResourceManagement;
using System;
using System.IO;
using Vanjaro.UXManager.Library.Common;

namespace Vanjaro.UXManager.Extensions.Menu.CustomCSS.Managers
{
    public class StyleSheetManager
    {
        internal static string LoadStyleSheet(int PortalID)
        {
            string activeLanguage = LocaleController.Instance.GetDefaultLocale(PortalID).Code;
            PortalInfo portal = PortalController.Instance.GetPortal(PortalID, activeLanguage);

            string uploadDirectory = "";
            string styleSheetContent = "";
            if (portal != null)
            {
                uploadDirectory = portal.HomeDirectoryMapPath;
            }

            //read CSS file
            if (File.Exists(uploadDirectory + "portal.css"))
            {
                using (StreamReader text = File.OpenText(uploadDirectory + "portal.css"))
                {
                    styleSheetContent = text.ReadToEnd();
                }
            }

            return styleSheetContent;
        }

        internal static ActionResult Update(int PortalID, string StyleSheetContent)
        {
            ActionResult actionResult = new ActionResult();
            try
            {
                string strUploadDirectory = string.Empty;
                string relativePath = string.Empty;

                PortalInfo objPortal = PortalController.Instance.GetPortal(PortalID);
                if (objPortal != null)
                {
                    strUploadDirectory = objPortal.HomeDirectoryMapPath;
                    relativePath = $"{Globals.ApplicationPath}/{objPortal.HomeDirectory}/portal.css";
                }

                //reset attributes
                if (File.Exists(strUploadDirectory + "portal.css"))
                {
                    File.SetAttributes(strUploadDirectory + "portal.css", FileAttributes.Normal);
                }

                //write CSS file
                if (!string.IsNullOrEmpty(StyleSheetContent.Replace("\n", "")))
                {
                    using (StreamWriter writer = File.CreateText(strUploadDirectory + "portal.css"))
                    {
                        writer.WriteLine(StyleSheetContent);
                    }
                }
                else if (File.Exists(strUploadDirectory + "portal.css"))
                {
                    File.Delete(strUploadDirectory + "portal.css");
                }

                //Clear client resource cache
                string overrideSetting = PortalController.GetPortalSetting(ClientResourceSettings.OverrideDefaultSettingsKey, PortalID, "False");
                if (bool.TryParse(overrideSetting, out bool overridePortal))
                {
                    if (overridePortal)
                    {
                        // increment this portal version only
                        PortalController.IncrementCrmVersion(PortalID);
                    }
                    else
                    {
                        // increment host version, do not increment other portal versions though.
                        HostController.Instance.IncrementCrmVersion(false);
                    }
                }

                ClientResourceManager.ClearFileExistsCache(relativePath);
                actionResult.IsSuccess = true;
                return actionResult;
            }
            catch (Exception ex)
            {
                actionResult.AddError("", "", ex);
                return actionResult;
            }
        }
    }
}