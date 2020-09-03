using DotNetNuke.Abstractions;
using DotNetNuke.Common;
using DotNetNuke.Entities.Portals;
using System.Collections.Generic;
using System.Linq;
using Vanjaro.Common.Utilities;
using Vanjaro.UXManager.Library.Entities.Enum;
using Vanjaro.UXManager.Library.Entities.Interface;
using Vanjaro.UXManager.Library.Entities.Menu;
using static Vanjaro.UXManager.Library.Factories;

namespace Vanjaro.UXManager.Library
{
    public static partial class Managers
    {
        public class AppManager
        {

            public static List<IAppExtension> GetExtentions(AppType type)
            {
                switch (type)
                {
                    case AppType.Module:
                        return AppExtensionFactory.ModuleExtentions.ToList();
                    default:
                        return AppExtensionFactory.Extentions.ToList();
                }
            }

            public static List<AppExtension> GetAll(AppType type)
            {
                List<AppExtension> result = new List<AppExtension>();
                switch (type)
                {
                    case AppType.Module:
                        {
                            foreach (IAppExtension ext in AppExtensionFactory.ModuleExtentions.Where(x => x.Visibility).ToList())
                            {
                                result.Add(ext.Item);
                            }

                            break;
                        }
                    default:
                        {
                            foreach (IAppExtension ext in AppExtensionFactory.Extentions.Where(x => x.Visibility).ToList())
                            {
                                result.Add(ext.Item);
                            }

                            break;
                        }
                }
                return result;
            }

            //internal static string GetAboutUrl()
            //{
            //    IAppExtension app = GetExtentions(AppType.None).Where(e => e.App.Name == "About").FirstOrDefault();
            //    if (app != null)
            //        return "OpenAbout(event,\"" + app.Item.Text + "\", \"" + ServiceProvider.NavigationManager.NavigateURL().TrimEnd('/') + "?mid=0&icp=true&guid=" + app.SettingGuid + "\")";
            //    else
            //        return "";
            //}
            internal static string GetAboutUrl()
            {
                IAppExtension app = GetExtentions(AppType.None).Where(e => e.App.Name == "About").FirstOrDefault();
                if (app != null)
                {
                    string url = null;
                    url = ServiceProvider.NavigationManager.NavigateURL().ToLower().Replace(PortalSettings.Current.DefaultLanguage.ToLower(), PortalSettings.Current.CultureCode.ToLower()).TrimEnd('/') + MenuManager.GetURL() + "mid=0&icp=true&guid=" + app.SettingGuid;
                    return "OpenAbout(event,\"" + app.Item.Text + "\", \"" + url + "\")";
                }
                else
                {
                    return "";
                }
            }



        }
    }
}