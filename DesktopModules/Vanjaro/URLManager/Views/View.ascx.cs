using Vanjaro.Common.Engines.UIEngine.AngularBootstrap;
using Vanjaro.Common.Entities.Apps;
using Vanjaro.Common.Foundation;
using Vanjaro.URL.Factories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using static Vanjaro.Common.FrameworkManager;

namespace Vanjaro.URL.Views
{
    public partial class View : AngularModuleBase
    {
        public override string AppTemplatePath { get { return "DesktopModules/Vanjaro/" + App.Name + "/Views/"; } }
        public override List<AngularView> AngularViews { get { return AppFactory.GetViews(); } }
        public override AppInformation App
        {
            get { return AppFactory.GetAppInformation(); }
        }
        public override string AccessRoles
        {
            get
            {
                return AppFactory.GetAccessRoles(ModuleConfiguration, UserInfo);
            }
        }
        public override bool ShowMissingKeys
        {
            get
            {
#if DEBUG
                return true;
#else
                return false;
#endif
            }
        }
        public override string[] Dependencies
        {
            get
            {
                return new string[] {
                    Frameworks.jQueryUI.ToString(),
                    Frameworks.FontAwesome.ToString()
                };
            }
        }
    }
}