using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Tabs;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Vanjaro.UXManager.Library
{
    public partial class ModuleControl : System.Web.UI.Page
    {
        string FriendlyName = string.Empty;
        string HashControl = string.Empty;
        protected void Page_Load(object sender, EventArgs e)
        {
            string GUID = Request.QueryString["guid"];
            FriendlyName = Request.QueryString["moduledefinition"];
            HashControl = Request.QueryString["modulecontrol"];
            ListItem listItem = GetAllModules(PortalSettings.Current.PortalId, FriendlyName).FirstOrDefault();
            if (listItem != null && !string.IsNullOrEmpty(listItem.Href) && !string.IsNullOrEmpty(FriendlyName))
            {
                string URL = !string.IsNullOrEmpty(HashControl) ? (listItem.Href + "#!/" + HashControl) : listItem.Href;
                Response.Redirect(URL, false);
            }
        }


        public List<ModuleInfo> GetModules(int PortalID)
        {
            List<ModuleInfo> Modules = new List<ModuleInfo>();
            ModuleController modController = new ModuleController();
            ArrayList mods = modController.GetModulesByDefinition(PortalID, FriendlyName);

            foreach (ModuleInfo modInfo in mods)
            {
                if (modInfo.ModuleDefinition.FriendlyName == FriendlyName && !modInfo.IsDeleted)
                    Modules.Add(modInfo);

            }
            return Modules;
        }


        public static List<ListItem> GetAllModules(int PortalID, string FriendlyName)
        {
            List<ListItem> Modules = new List<ListItem>();
            ModuleController modController = new ModuleController();
            ArrayList mods = modController.GetModulesByDefinition(PortalID, FriendlyName);

            foreach (ModuleInfo modInfo in mods)
            {
                if (modInfo.ModuleDefinition.FriendlyName == FriendlyName && !modInfo.IsDeleted)
                {
                    TabInfo tabinfo = TabController.Instance.GetTab(modInfo.TabID, PortalID);
                    ListItem Module = new ListItem();

                    Module.Text = tabinfo.TabName;
                    Module.Value = modInfo.ModuleID.ToString();
                    Module.Href = new TabController().GetTab(modInfo.TabID, PortalID).FullUrl + "?ctl=Manage&mid=" + modInfo.ModuleID;
                    Modules.Add(Module);
                }
            }

            return Modules.OrderBy(x => x.Text)?.ToList();
        }
        public class ListItem
        {
            public string Text { get; set; }
            public string Value { get; set; }
            public string Href { get; set; }
            public bool Enable { get; set; }
            public string ControlType { get; set; }
        }
    }
}