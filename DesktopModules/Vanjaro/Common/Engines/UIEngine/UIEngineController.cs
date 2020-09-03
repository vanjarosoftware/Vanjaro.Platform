using DotNetNuke.Abstractions;
using DotNetNuke.Web.Api;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Http;
using Vanjaro.Common.ASPNET.WebAPI;
using Vanjaro.Common.Entities;
using Vanjaro.Common.Factories;
using Vanjaro.Common.Utilities;

namespace Vanjaro.Common.Engines.UIEngine
{
    [AuthorizeAccessRoles]
    [ValidateAntiForgeryToken]
    public abstract class UIEngineController : WebApiController
    {
        public Dictionary<string, string> UIEngineInfo;
        private static readonly object LockObject = new object();
        [ValidateAntiForgeryToken]
        [HttpGet]
        public string GetLink(int EntityId)
        {
            if (EntityId > 0 || EntityId == -1)
            {
                if (!PortalSettings.EnablePopUps)
                {
                    return ServiceProvider.NavigationManager.NavigateURL("URLLibrary_View", "mid=" + ActiveModule.ModuleID, "hidecommandbar=true&SkinSrc=[G]Skins/_default/popUpSkin");
                }
                else
                {
                    return ServiceProvider.NavigationManager.NavigateURL("URLLibrary_View", "mid=" + ActiveModule.ModuleID);
                }
            }
            else
            {
                return null;
            }
        }

        [ValidateAntiForgeryToken]
        [HttpPost]
        public dynamic Render([FromBody]dynamic data)
        {
            dynamic result = null;

            IEnumerable<string> headerValues = Request.Headers.GetValues("ModuleId");
            string ModuleID = headerValues.FirstOrDefault();

            if (string.IsNullOrEmpty(ModuleID))
            {
                ThrowError(ref result, "Identifier is required to process UIEngine requests");
            }

            Dictionary<string, string> parameters = new Dictionary<string, string>();
            foreach (dynamic item in data.parameters)
            {
                parameters.Add(item.Name.ToLower(), item.Value.Value);
            }

            UIEngineInfo = new Dictionary<string, string>();
            foreach (dynamic item in data)
            {
                UIEngineInfo.Add(item.Name.ToLower(), item.Value.Value);
            }

            UIEngineInfo.Add("moduleid", ModuleID);

            //Requried for script initialization such as Sortable
            UIEngineInfo.Add("InitScript", string.Empty);

            //Fix for Ckeditor
            UIEngineInfo.Add("PreInitScript", string.Empty);


            if (!UIEngineInfo.ContainsKey("provider"))
            {
                ThrowError(ref result, "Did you forget to specify a UI Engine Provider? &lt;uielement provider=&quot;AngularBootstrap&quot;&gt;&lt;/uielement&gt;");
            }

            if (!UIEngineInfo.ContainsKey("identifier") || (UIEngineInfo.ContainsKey("identifier") && (UIEngineInfo["identifier"].Contains(".") || UIEngineInfo["identifier"].Contains(' '))))
            {
                ThrowError(ref result, "Did you forget to specify a UI Identifier and Identifier shouldn't contain whitespace and dot(.)? &lt;uielement identifier=&quot;admin_settings_general&quot;&gt;&lt;/uielement&gt;");
            }

            if (UIEngineInfo.ContainsKey("layout") && UIEngineInfo.ContainsKey("uienginepath"))
            {
                string _OverLoadPath = HttpContext.Current.Server.MapPath(UIEngineInfo["uienginepath"]);// + "/Layouts/" + UIEngineInfo["layout"] + ".htm");
                UIEngineInfo.Add("overloadpath", _OverLoadPath);
            }

            //if (!UIEngineInfo.ContainsKey("layoutmarkup"))
            //    ThrowError(ref result, "Did you forget to specify UI Elements? &lt;uielement identifier=&quot;admin_settings_general&quot;&gt;&lt;/uielement&gt;");

            if (!UIEngineInfo.ContainsKey("showmissingkeys"))
            {
                UIEngineInfo.Add("showmissingkeys", "false");
            }

            UIEngineInfo.Add("layoutmarkup", GetLayoutMarkup(UIEngineInfo["templateurl"]));


            //Return Cached Result
            //var cachedResult = Utilities.DataCache.GetItemFromCache<dynamic>(UIEngineInfo["moduleid"] + UIEngineInfo["identifier"]);
            //if (cachedResult != null)
            //    return cachedResult;

            ValidateMarkup(ref result);


            if (result == null) // No validation errors
            {
                lock (LockObject)
                {
                    //Get UI Data
                    List<IUIData> IUIDataList;

                    //if (UIEngineInfo["identifier"] == "licensing")
                    //    IUIDataList = GetLicensingData(UIEngineInfo["appname"]);
                    //else
                    IUIDataList = GetData(UIEngineInfo["identifier"], parameters);

                    //Convert it to Dynamic
                    if (IUIDataList != null)
                    {
                        dynamic UIData = ConvertToDynamic(IUIDataList);

                        switch (UIEngineInfo["provider"])
                        {
                            case "AngularBootstrap":
                                {
                                    result = AngularBootstrapUIEngine.GetMarkup(UIEngineInfo, UIData);

                                    break;
                                }
                            default:
                                {
                                    ThrowError(ref result, UIEngineInfo["provider"] + ": This is not a valid UI Engine Provider");
                                                                        
                                    break;
                                }
                        }
                    }
                }
            }
            return result;
        }

        private string GetLayoutMarkup(string TemplatePath)
        {
            string result = string.Empty;
            if (!string.IsNullOrEmpty(TemplatePath))
            {
                TemplatePath = TemplatePath.Split('?')[0].TrimStart('/').ToLower();
                if (TemplatePath.Contains("desktopmodules"))
                {
                    var splitarr = TemplatePath.Split(new string[] { "desktopmodules" }, StringSplitOptions.None);
                    if (splitarr.Length > 1)
                        TemplatePath = "desktopmodules" + splitarr[1];
                    else
                        TemplatePath = "desktopmodules" + splitarr[0];
                }
                else if (TemplatePath.Contains("portals"))
                {
                    var splitarr = TemplatePath.Split(new string[] { "portals" }, StringSplitOptions.None);
                    if (splitarr.Length > 1)
                        TemplatePath = "portals" + splitarr[1];
                    else
                        TemplatePath = "portals" + splitarr[0];
                }
                if (!TemplatePath.StartsWith("~"))
                    TemplatePath = HttpContext.Current.Server.MapPath(VirtualPathUtility.ToAbsolute("~/" + TemplatePath));
                else
                    TemplatePath = HttpContext.Current.Server.MapPath(TemplatePath);
                if (File.Exists(TemplatePath))
                {
                    result = File.ReadAllText(TemplatePath);
                    if (!string.IsNullOrEmpty(result))
                    {
                        LayoutMarkUp markUp = new LayoutMarkUp();
                        HtmlNode.ElementsFlags["option"] = HtmlElementFlag.Closed;
                        HtmlDocument html = new HtmlDocument();
                        html.LoadHtml(result);
                        var query = html.DocumentNode.Descendants("uiengine");
                        foreach (var item in query.ToList())
                        {
                            markUp.tag = item.Name;
                            markUp.attr = new Dictionary<string, object>();
                            foreach (var attr in item.Attributes)
                            {
                                if (attr.Name.ToLower() == "class")
                                {
                                    List<string> values = attr.Value.Split(' ').ToList();
                                    markUp.attr.Add(attr.Name, values);
                                }
                                else
                                    markUp.attr.Add(attr.Name, attr.Value);
                            }
                            markUp.text = GetMarkUpText(item.ChildNodes);
                            markUp.child = new List<LayoutMarkUp>();
                            ProcessChild(item.ChildNodes, markUp.child);
                        }
                        result = DotNetNuke.Common.Utilities.Json.Serialize(markUp);
                    }
                }
            }
            return result;
        }

        private string UpdateTemplatePath(string templatePath, string identifier)
        {
            if (identifier.StartsWith("common_"))
            {
                return "DesktopModules/Vanjaro/Common/Engines/UIEngine/AngularBootstrap/Views/";
            }
            else
            {
                return templatePath;
            }
        }

        private void ProcessChild(HtmlNodeCollection childNodes, List<LayoutMarkUp> child)
        {
            if (childNodes != null && childNodes.Count > 0)
            {
                foreach (HtmlNode item in childNodes)
                {
                    if (item.Name.ToLower() != "#text" && item.Name.ToLower() != "#comment" && item.Name.ToLower() != "#comments")
                    {
                        LayoutMarkUp markUp = new LayoutMarkUp
                        {
                            tag = item.Name,
                            attr = new Dictionary<string, object>()
                        };
                        foreach (HtmlAttribute attr in item.Attributes)
                        {
                            if (attr.Name.ToLower() == "class")
                            {
                                List<string> values = attr.Value.Split(' ').ToList();
                                markUp.attr.Add(attr.Name, values);
                            }
                            else
                            {
                                markUp.attr.Add(attr.Name, attr.Value);
                            }
                        }
                        markUp.text = GetMarkUpText(item.ChildNodes);
                        markUp.child = new List<LayoutMarkUp>();
                        ProcessChild(item.ChildNodes, markUp.child);
                        child.Add(markUp);
                    }
                }
            }
        }

        private string GetMarkUpText(HtmlNodeCollection childNodes)
        {
            StringBuilder sb = new StringBuilder();
            if (childNodes != null && childNodes.Count > 0)
            {
                foreach (HtmlNode item in childNodes)
                {
                    if (item.Name.ToLower() == "#text")
                    {
                        sb.Append(item.InnerText);
                    }
                }
            }
            return sb.ToString();
        }

        private dynamic ConvertToDynamic(List<IUIData> UIDataList)
        {
            List<dynamic> DynamicList = new List<dynamic>();

            foreach (IUIData ui in UIDataList)
            {
                DynamicList.Add(ui.ToDynamic());
            }

            return DynamicList;
        }

        private void ValidateMarkup(ref dynamic result)
        {

            try
            {
                dynamic LayoutInfoData = DotNetNuke.Common.Utilities.Json.Deserialize<dynamic>(UIEngineInfo["layoutmarkup"]);
                foreach (dynamic layout in LayoutInfoData.child)
                {
                    string TryUiLayoutName = layout.attr.name.GetType().Name;
                }
            }
            catch
            {
                ThrowError(ref result, "Did you forget to specify uilayout name? &lt;uilayout name=&quot;default&quot;&gt;");
            }
        }

        private void ThrowError(ref dynamic result, string Message)
        {
            string markup = "<div class=\"ms-alert ms-alert-danger\" role=\"alert\">" + Message + "</div>";

            dynamic response = new
            {
                markup,
                data = "{}"
            };

            result = response;
        }

        public virtual List<IUIData> GetData(string Identifier, Dictionary<string, string> parameters)
        {
            if (!string.IsNullOrEmpty(Identifier))
            {
                switch (Identifier)
                {
                    case "common_controls_editorconfig":
                        return EditorConfigFactory.GetData(PortalSettings, ActiveModule, parameters);
                    case "common_controls_url":
                        return BrowseUploadFactory.GetData(PortalSettings, ActiveModule, parameters);
                    default:
                        return new List<IUIData>();
                }
            }
            else
            {
                return new List<IUIData>();
            }
        }

        [ValidateAntiForgeryToken]
        [HttpGet]
        public virtual string Cancel()
        {
            return CancelUrl;
        }

        [ValidateAntiForgeryToken]
        [HttpPost]
        public virtual void UpdateData([FromUri]string Identifier, [FromBody]dynamic UIData)
        {
            //Clear Cache
        }

        public virtual string SubmitUrl => Url.Request.RequestUri.AbsoluteUri.TrimEnd('r', 'e', 'n', 'd', 'e', 'r') + "updateData";
        public virtual string CancelUrl => ServiceProvider.NavigationManager.NavigateURL();
    }


}
