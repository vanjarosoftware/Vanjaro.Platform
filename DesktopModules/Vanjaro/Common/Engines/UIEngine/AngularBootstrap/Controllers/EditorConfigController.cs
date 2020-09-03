using DotNetNuke.Entities.Modules;
using DotNetNuke.Security.Permissions;
using DotNetNuke.Web.Api;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Web.Http;
using Vanjaro.Common.ASPNET.WebAPI;
using Vanjaro.Common.Components;
using Vanjaro.Common.Data.Entities;
using Vanjaro.Common.Factories;

namespace Vanjaro.Common.Engines.UIEngine.AngularBootstrap.Controllers
{
    [AuthorizeAccessRoles(AccessRoles = "admin")]
    public class EditorConfigController : WebApiController
    {
        [ValidateAntiForgeryToken]
        [HttpPost]
        public bool SaveEditorProfile(string uid, int profileid, string applyto, HTMLEditorSetting settings)
        {
            if (!string.IsNullOrEmpty(uid) && !string.IsNullOrEmpty(applyto))
            {
                string uniqueUid = uid;
                try
                {
                    switch (applyto)
                    {
                        case "CurrentModule":
                            {
                                if (ActiveModule != null)
                                {
                                    uid = uniqueUid.ToLower().Trim() + ActiveModule.ModuleID;
                                }
                                else
                                {
                                    uid = uniqueUid.ToLower().Trim() + "0";
                                }

                                HTMLEditor HTMLEditor = HTMLEditor.Query("where UID=@0", uid).SingleOrDefault();
                                if (HTMLEditor != null)
                                {
                                    HTMLEditor.ProfileID = profileid;
                                    HTMLEditor.Settings = DotNetNuke.Common.Utilities.Json.Serialize(settings);
                                    HTMLEditor.Update();
                                }
                                else
                                {
                                    HTMLEditor = new HTMLEditor
                                    {
                                        ProfileID = profileid,
                                        UID = uid,
                                        Settings = DotNetNuke.Common.Utilities.Json.Serialize(settings),
                                        PortalID = PortalSettings.ActiveTab.PortalID,
                                        TabID = PortalSettings.ActiveTab.TabID
                                    };
                                    HTMLEditor.Insert();
                                }
                                break;
                            }
                        case "PageModule":
                            {
                                foreach (ModuleInfo moduleinfo in EditorConfigFactory.GetTabModules(ActiveModule))
                                {
                                    uid = uniqueUid.ToLower().Trim() + moduleinfo.ModuleID;
                                    HTMLEditor HTMLEditor = HTMLEditor.Query("where UID=@0", uid).SingleOrDefault();
                                    if (HTMLEditor != null)
                                    {
                                        HTMLEditor.ProfileID = profileid;
                                        HTMLEditor.Settings = DotNetNuke.Common.Utilities.Json.Serialize(settings);
                                        HTMLEditor.Update();
                                    }
                                    else
                                    {
                                        HTMLEditor = new HTMLEditor
                                        {
                                            ProfileID = profileid,
                                            UID = uid,
                                            Settings = DotNetNuke.Common.Utilities.Json.Serialize(settings)
                                        };
                                        HTMLEditor.Insert();
                                    }
                                }
                                break;
                            }
                        case "PortalModule":
                            {
                                foreach (ModuleInfo moduleinfo in EditorConfigFactory.GetPortalModules(ActiveModule))
                                {
                                    uid = uniqueUid.ToLower().Trim() + moduleinfo.ModuleID;
                                    HTMLEditor HTMLEditor = HTMLEditor.Query("where UID=@0", uid).SingleOrDefault();
                                    if (HTMLEditor != null)
                                    {
                                        HTMLEditor.ProfileID = profileid;
                                        HTMLEditor.Settings = DotNetNuke.Common.Utilities.Json.Serialize(settings);
                                        HTMLEditor.Update();
                                    }
                                    else
                                    {
                                        HTMLEditor = new HTMLEditor
                                        {
                                            ProfileID = profileid,
                                            UID = uid,
                                            Settings = DotNetNuke.Common.Utilities.Json.Serialize(settings)
                                        };
                                        HTMLEditor.Insert();
                                    }
                                }
                                break;
                            }
                        default:
                            goto case "CurrentModule";

                    }
                    return true;
                }
                catch (Exception ex)
                {
                    DotNetNuke.Services.Exceptions.Exceptions.LogException(ex);
                }
            }
            return false;
        }

        [ValidateAntiForgeryToken]
        [HttpPost]
        public dynamic SaveProfile(int profileid, string profileName, string uid, EditorOptions EditorOptions)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>
            {
                { "uid", uid }
            };
            return EditorConfigFactory.SaveProfile(PortalSettings, ActiveModule, profileid, profileName, EditorOptions, parameters);
        }

        [ValidateAntiForgeryToken]
        [HttpGet]
        public dynamic GetNewProfile()
        {
            EditorOptions EditorOptions = new EditorOptions();
            EditorOptions.Plugins = EditorConfigFactory.GetPluginsKeyValue("FullCustom", EditorOptions);
            dynamic result = new ExpandoObject();
            result.EditorOptions = EditorConfigFactory.GetEditorOptions(EditorConfigFactory.GetHTMLEditorProfiles(ActiveModule.PortalID), 0);
            result.FullPlugins = EditorOptions.Plugins.ToDictionary(u => u.Key, u => false);
            return result;
        }

        [ValidateAntiForgeryToken]
        [HttpDelete]
        public string DeleteProfile(int profileid)
        {
            return EditorConfigFactory.DeleteProfile(ActiveModule.PortalID, profileid);
        }
        public override string AccessRoles()
        {
            if (UserInfo.IsInRole("Administrators") || ModulePermissionController.CanEditModuleContent(ActiveModule))
            {
                return "admin";
            }
            else
            {
                return "";
            }
        }
    }
}