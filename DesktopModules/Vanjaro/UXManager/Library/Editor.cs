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
        internal static dynamic VjObjects = new ExpandoObject();

        public static void RequestRegistration(dynamic NewVjObjects)
        {
            if (NewVjObjects == null)
            {
                VjObjects.SetURL = "parent.window.location.origin + $.ServicesFramework(-1).getServiceRoot('Vanjaro') + 'page/save'";
                VjObjects.GetURL = "parent.window.location.origin + $.ServicesFramework(-1).getServiceRoot('Vanjaro') + 'page/get'";
                VjObjects.Container = "#VjContentPane";
                VjObjects.InitTabGrapesjs = true;
                VjObjects.ModuleId = -1;
            }
            else
            {
                NewVjObjects.InitTabGrapesjs = false;
                VjObjects = NewVjObjects;
            }
        }
    }
}