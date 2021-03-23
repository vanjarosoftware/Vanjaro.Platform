using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Modules.Actions;
using DotNetNuke.Security.Permissions;
using DotNetNuke.Services.Exceptions;
using DotNetNuke.UI.Modules;
using System;
using System.Runtime.CompilerServices;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Vanjaro.Container
{
    public partial class Base : DotNetNuke.UI.Containers.Container
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            Literal lit = new Literal();
            IActionable actionable = ModuleControl as IActionable;
            if (actionable != null)
            {
                string JsonAction = "";

                JsonAction = "[";
                foreach (ModuleAction action in actionable.ModuleActions)
                {
                    if (ModulePermissionController.HasModuleAccess(action.Secure, "CONTENT", ModuleConfiguration))
                    {
                        string url = string.Empty;
                        if (!string.IsNullOrEmpty(action.Url) && action.Url.Length > 0)
                        {
                            var splitarray = action.Url.Split('#');
                            url = splitarray[0];
                            if (url.Contains("?"))
                                url += "&skinsrc=" + "[g]skins/vanjaro/base";
                            else
                                url += "?skinsrc=" + "[g]skins/vanjaro/base";
                            if (splitarray.Length > 1)
                                url += "#" + splitarray[1];
                        }
                        JsonAction += "{\"Title\":\"" + action.Title + "\", \"Icon\":\"" + action.Icon + "\",\"Url\":\"" + url + "\",\"NewWindow\":\"" + action.NewWindow + "\",\"ModuleId\":\"" + ModuleConfiguration.ModuleID + "\"},";
                    }
                }

                JsonAction = JsonAction.TrimEnd(',');
                JsonAction += "]";

                if (JsonAction != "[]")
                    lit.Text = "<script type=\"text/javascript\" data-actionmid=\"" + ModuleConfiguration.ModuleID + "\">" + JsonAction + "</script>";
            }
            if (HasSettings(Page, ModuleConfiguration))
            {
                lit.Text += "<script type=\"text/javascript\" data-settingsmid=\"" + ModuleConfiguration.ModuleID + "\"></script>";
            }

            Page.Header.Controls.Add(lit);
        }

        public bool HasSettings(Page Page, ModuleInfo Module)
        {
            try
            {
                ModuleControlInfo moduleControlInfo = ModuleControlController.GetModuleControlByControlKey("Settings", Module.ModuleDefID);
                if (moduleControlInfo != null)
                {

                    Control _control = ModuleControlFactory.LoadSettingsControl(Page, Module, moduleControlInfo.ControlSrc);
                    ISettingsControl settingsControl = _control as ISettingsControl;
                    if (settingsControl != null)
                    {
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                Exceptions.LogException(ex);
            }
            return false;
        }
    }
}