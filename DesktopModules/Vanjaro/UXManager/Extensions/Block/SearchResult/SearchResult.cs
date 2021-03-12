using DotNetNuke.Common;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using DotNetNuke.Framework;
using DotNetNuke.Services.Localization;
using System;
using System.Collections.Generic;
using System.Web;
using Vanjaro.Common.Engines.TokenEngine;
using Vanjaro.Common.Engines.UIEngine.AngularBootstrap;
using Vanjaro.Common.Entities.Apps;
using Vanjaro.Common.Manager;
using Vanjaro.Core.Entities.Menu;
using Vanjaro.UXManager.Extensions.Block.SearchResult.Factories;
using Vanjaro.UXManager.Library.Entities.Interface;
using static Vanjaro.Core.Managers;

namespace Vanjaro.UXManager.Extensions.Block.SearchResult
{
    public class Login : Core.Entities.Interface.IBlock, IExtension
    {
        public string Category => "Design";

        public string Name => "Search Result";
        public string DisplayName => Localization.GetString("DisplayName", Components.Constants.LocalResourcesFile);
        public string Icon => "fa fa-search";

        public Guid Guid => Guid.Parse(ExtensionInfo.GUID);
        public bool Visible { get; set; } = true;

        public Dictionary<string, string> Attributes
        {
            get
            {
                Dictionary<string, string> result = new Dictionary<string, string>();
                return result;
            }
        }

        public ThemeTemplateResponse Render(Dictionary<string, string> Attributes)
        {
            ServicesFramework.Instance.RequestAjaxScriptSupport();
            ServicesFramework.Instance.RequestAjaxAntiForgerySupport();
            ThemeTemplateResponse response = new ThemeTemplateResponse
            {
                Markup = GenerateMarkup(Attributes),
                Scripts = new string[2] { ResourcesFolderPath + "Scripts/SearchResult.js", "~/DesktopModules/Vanjaro/Common/Frameworks/Toastr/2.1.4/js/toastr.min.js" },
                Styles = new string[1] { "~/DesktopModules/Vanjaro/Common/Frameworks/Toastr/2.1.4/css/toastr.min.css" }
            };
            return response;
        }

        public AppInformation App => AppFactory.GetAppInformation();

        public List<AngularView> AngularViews => AppFactory.GetViews();

        public string UIPath => "~/DesktopModules/Vanjaro/UXManager/Extensions/Block/" + ExtensionInfo.Name + "/Views/";

        public string AppCssPath => "~/DesktopModules/Vanjaro/UXManager/Extensions/Block/" + ExtensionInfo.Name + "/Resources/Stylesheets/app.css";

        public string AppJsPath => "~/DesktopModules/Vanjaro/UXManager/Extensions/Block/" + ExtensionInfo.Name + "/Resources/Scripts/app.js";

        public string ResourcesFolderPath => "~/DesktopModules/Vanjaro/UXManager/Extensions/Block/" + ExtensionInfo.Name + "/Resources/";

        public string UIEngineAngularBootstrapPath => string.Empty;

        public string[] Dependencies => new string[] {
                    "Bootstrap" };

        public string BlockPath
        {
            get
            {
                PortalSettings ps = PortalController.Instance.GetCurrentSettings() as PortalSettings;
                return Core.Managers.BlockManager.GetTemplateDir(ps, "search result");
            }
        }

        public string ResouceFilePath
        {
            get
            {

                return Globals.ApplicationMapPath + @"\Portals\_default\" + Core.Managers.BlockManager.GetTheme(PortalSettings.Current.PortalId) + "App_LocalResources\\Shared.resx";
            }
        }

        private string GenerateMarkup(Dictionary<string, string> Attributes)
        {
            try
            {
                string Template = string.Empty;
                Dictionary<string, string> blockAttribute = new Dictionary<string, string>();
                string Keyword = HttpContext.Current.Request.QueryString["Search"];
                if (!string.IsNullOrEmpty(Keyword))
                {
                    PortalSettings ps = PortalController.Instance.GetCurrentSettings() as PortalSettings;
                    if (Attributes.ContainsKey("data-block-pageindex"))
                    {
                        blockAttribute.Add("data-block-pageindex", Attributes["data-block-pageindex"]);
                    }
                    Dictionary<string, string> baseAttributes = Core.Managers.BlockManager.GetGlobalConfigs(ps, "search result");

                    if (Attributes["data-block-global"] == "true")
                    {
                        Attributes.Clear();
                        if (baseAttributes != null)
                        {
                            foreach (KeyValuePair<string, string> baseattr in baseAttributes)
                            {
                                Attributes.Add(baseattr.Key, baseattr.Value);
                            }
                        }
                    }
                    else
                    {
                        //Loop on base attributes and add missing attribute
                        if (baseAttributes != null)
                        {
                            foreach (KeyValuePair<string, string> attr in baseAttributes)
                            {
                                if (!Attributes.ContainsKey(attr.Key))
                                {
                                    Attributes.Add(attr.Key, attr.Value);
                                }
                            }
                        }
                    }

                    foreach (KeyValuePair<string, string> t in blockAttribute)
                    {
                        if (!Attributes.ContainsKey(t.Key))
                        {
                            Attributes.Add(t.Key, t.Value);
                        }
                    }

                    Entities.SearchResult searchResult = new Entities.SearchResult(Keyword, Attributes)
                    {
                        LinkTargetOpenInNewTab = Attributes.ContainsKey("data-block-linktarget") && Attributes["data-block-linktarget"] == "false" ? false : true
                    };
                    IDictionary<string, object> dynObjects = new System.Dynamic.ExpandoObject() as IDictionary<string, object>;
                    dynObjects.Add("SearchResult", searchResult);
                    Template = RazorEngineManager.RenderTemplate(ExtensionInfo.GUID, BlockPath, Attributes["data-block-template"], dynObjects);
                    Template = new DNNLocalizationEngine(null, ResouceFilePath, false).Parse(Template);
                }
                else
                {
                    Template = "<div class='Searchresultempty'><div class='no-search-result-found search-info-msg'>" + Localization.GetString("NoSearchResultFound", Components.Constants.LocalResourcesFile) + "</div></div>";
                }
                return Template;
            }
            catch (Exception ex)
            {
                ExceptionManager.LogException(ex);
                return ex.Message;
            }
        }

        public string AccessRoles(UserInfo userInfo)
        {
            return AppFactory.GetAccessRoles(userInfo);
        }
    }
}