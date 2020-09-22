using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Web;
using System.Web.UI;

namespace Vanjaro.UXManager.Library
{
    public class Editor
    {
        public static dynamic Settings = new ExpandoObject();

        public static void RequestRegistration(dynamic settings)
        {
            if (settings == null)
            {
                Settings.SetURL = "parent.window.location.origin + $.ServicesFramework(-1).getServiceRoot('Vanjaro') + 'page/save'";
                Settings.GetURL = "parent.window.location.origin + $.ServicesFramework(-1).getServiceRoot('Vanjaro') + 'page/get'";
                Settings.Container = "#vjEditor";
                Settings.EditPage = true;
                Settings.ModuleId = -1;
            }
            else
            {
                settings.EditPage = false;
                Settings = settings;
            }
        }
    }
}