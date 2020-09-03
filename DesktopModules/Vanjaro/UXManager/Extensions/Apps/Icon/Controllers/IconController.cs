using DotNetNuke.Common;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using DotNetNuke.Web.Api;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Http;
using System.Xml.Linq;
using Vanjaro.Common.ASPNET.WebAPI;
using Vanjaro.Common.Engines.UIEngine;
using Vanjaro.UXManager.Library.Common;

namespace Vanjaro.UXManager.Extensions.Apps.Block.Icon.Controllers
{
    [ValidateAntiForgeryToken]
    [AuthorizeAccessRoles(AccessRoles = "editpage")]
    public class IconController : UIEngineController
    {
        internal static List<IUIData> GetData(int PortalID, UserInfo UserInfo)
        {
            Dictionary<string, IUIData> Settings = new Dictionary<string, IUIData>();
            string[] IconFolders = GetIconFolders();
            Settings.Add("IconFolders", new UIData { Name = "IconFolders", Options = IconFolders.Select(a => new { Value = a }), Value = "0", OptionsText = "Value", OptionsValue = "Value" });
            Settings.Add("DefaultIconLocation", new UIData { Name = "DefaultIconLocation", Value = PortalController.GetPortalSetting("DefaultIconLocation", PortalID, "Sigma", PortalSettings.Current.CultureCode).Replace("icons/", "") });
            Settings.Add("All_Icons", new UIData { Name = "All_Icons", Options = new List<dynamic>() });
            return Settings.Values.ToList();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Search(string Keyword, int index, int size, string IconFolder)
        {
            ActionResult actionResult = new ActionResult();
            string path = !string.IsNullOrEmpty(IconFolder) ? Path.Combine(Globals.ApplicationMapPath, "Icons\\" + IconFolder) : Path.Combine(Globals.ApplicationMapPath, PortalSettings.Current.DefaultIconLocation.Replace('/', '\\'));
            DirectoryInfo di = new DirectoryInfo(path);
            List<dynamic> data = new List<dynamic>();
            if (di.Exists)
            {
                try
                {
                    string SearchPattern = !string.IsNullOrEmpty(Keyword) ? "*" + Keyword.ToLower() + "*.svg" : "*.svg";
                    IOrderedEnumerable<FileInfo> icons = di.GetFiles(SearchPattern, SearchOption.AllDirectories).OrderBy(i => i.Name);

                    foreach (FileInfo file in icons.Skip(index).Take(size))
                    {
                        if (file.Exists)
                        {
                            data.Add(new { Name = file.Name.Replace(file.Extension.ToString(), ""), SVG = XDocument.Load(file.FullName.ToString()).ToString() });
                        }
                    }

                    if (actionResult.IsSuccess)
                    {
                        actionResult.Data = new { All_Icons = data, Total_Icon = data.Count > 0 ? icons.Count() : 0 };
                    }
                }
                catch (Exception ex)
                {
                    actionResult.AddError("Icon_Search", ex.Message);
                }
            }
            return actionResult;
        }
        public override string AccessRoles()
        {
            return Factories.AppFactory.GetAccessRoles(UserInfo);
        }

        public static string[] GetIconFolders()
        {
            string str = Path.Combine(Globals.ApplicationMapPath, "icons");
            DirectoryInfo directoryInfo = new DirectoryInfo(str);
            string str1 = "";
            foreach (DirectoryInfo directoryInfo1 in directoryInfo.EnumerateDirectories())
            {
                str1 = string.Concat(str1, directoryInfo1.Name, ",");
            }
            return str1.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
        }
    }
}