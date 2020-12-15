using DotNetNuke.Security.Permissions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Dynamic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Web;
using System.Web.UI;
using Vanjaro.UXManager.Library.Entities;

namespace Vanjaro.UXManager.Library.Entities
{
    public class Editor
    {
        public static void RequestRegistration(EditorOptions options)
        {
            Options = options;
            Options.EditPage = false;
        }

        private static EditorOptions DefaultSettings()
        {
            EditorOptions options = new EditorOptions()
            {
                UpdateContentUrl = "parent.window.location.origin + $.ServicesFramework(-1).getServiceRoot('Vanjaro') + 'page/save'",
                GetContentUrl = "parent.window.location.origin + $.ServicesFramework(-1).getServiceRoot('Vanjaro') + 'page/get'",
                ContainerID = "#vjEditor",
                EditPage = TabPermissionController.HasTabPermission("EDIT"),
                ModuleId = -1
            };

            return options;
        }
        public static EditorOptions Options
        {
            get
            {
                if (HttpContext.Current != null && HttpContext.Current.Items["vjeditor"] != null)
                    return HttpContext.Current.Items["vjeditor"] as EditorOptions;

                return DefaultSettings();
            }
            set
            {
                if (HttpContext.Current != null)
                    HttpContext.Current.Items["vjeditor"] = value;
            }
        }
    }
}