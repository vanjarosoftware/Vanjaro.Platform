using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using DotNetNuke.Web.Api;
using HtmlAgilityPack;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Web.Http;
using Vanjaro.Common.ASPNET.WebAPI;
using Vanjaro.Common.Engines.UIEngine;
using Vanjaro.Core.Data.Entities;
using Vanjaro.Core.Entities.Menu;
using Vanjaro.UXManager.Extensions.Toolbar.VersionManagement.Factories;
using Vanjaro.UXManager.Extensions.Toolbar.VersionManagement.Managers;

namespace Vanjaro.UXManager.Extensions.Toolbar.VersionManagement.Controllers
{
    [ValidateAntiForgeryToken]
    [AuthorizeAccessRoles(AccessRoles = "editpage")]
    public class RevisionsController : UIEngineController
    {
        public static List<IUIData> GetData(PortalSettings PortalSettings, Dictionary<string, string> Parameters)
        {
            Dictionary<string, IUIData> Settings = new Dictionary<string, IUIData>();
            string guid = string.Empty;
            if (Parameters.Count > 0)
            {
                try
                {
                    guid = Parameters["bguid"];
                }
                catch { }
            }

            Settings.Add("BlockGuid", new UIData { Name = "BlockGuid", Value = guid });
            return Settings.Values.ToList();
        }

        [HttpGet]
        public dynamic GetDate(string Locale, string BlockGuid)
        {
            return RevisionsManager.GetData(PortalSettings, Locale, BlockGuid);
        }

        [HttpPost]
        public dynamic Rollback(int Version, string Locale)
        {
            Locale = PortalSettings.DefaultLanguage == Locale ? null : Locale;
            RevisionsManager.Rollback(PortalSettings.ActiveTab.TabID, Version, Locale, UserInfo.UserID);
            return RevisionsManager.GetData(PortalSettings, Locale);
        }

        [HttpPost]
        public dynamic Delete(int Version, string Locale)
        {
            Locale = PortalSettings.DefaultLanguage == Locale ? null : Locale;
            Core.Managers.PageManager.Delete(PortalSettings.ActiveTab.TabID, Version);
            return RevisionsManager.GetData(PortalSettings, Locale);
        }

        [HttpGet]
        public dynamic GetVersion(int Version, string Locale)
        {
            dynamic Result = new ExpandoObject();
            Locale = PortalSettings.DefaultLanguage == Locale ? null : Locale;
            Pages page = Core.Managers.PageManager.GetByVersion(PortalSettings.ActiveTab.TabID, Version, Locale);
            if (page != null)
            {
                Core.Managers.PageManager.ApplyGlobalBlockJSON(page);
                Result.html = page.Content.ToString();
                HtmlDocument html = new HtmlDocument();
                html.LoadHtml(Result.html);
                InjectBlocks(html, Result);
                Result.css = page.Style.ToString();
                Result.components = page.ContentJSON.ToString();
                Result.style = page.StyleJSON.ToString();
            }
            Result.Version = RevisionsManager.GetAllVersionByTabID(PortalSettings.PortalId, PortalSettings.ActiveTab.TabID, Locale);
            return Result;
        }

        [HttpGet]
        public dynamic GetBlockVersion(int Version, string BlockGuid)
        {
            dynamic Result = new ExpandoObject();

            GlobalBlock Block = Core.Managers.BlockManager.GetAllGlobalBlocks(this.PortalSettings.PortalId, BlockGuid).Where(a => a.Version == Version).FirstOrDefault();
            if (Block != null)
            {
                Result.html = Block.Html.ToString();
                HtmlDocument html = new HtmlDocument();
                html.LoadHtml(Result.html);
                InjectBlocks(html, Result);
                Result.css = Block.Css.ToString();
                Result.components = Block.ContentJSON.ToString();
                Result.style = Block.StyleJSON.ToString();
                Result.Guid = Block.Guid;
            }
            return Result;
        }
        private void InjectBlocks(HtmlDocument html, dynamic Result)
        {
            Result.Scripts = new List<string>();
            Result.Styles = new List<string>();
            Result.BlocksMarkUp = string.Empty;
            Result.Script = string.Empty;
            Result.Style = string.Empty;
            IEnumerable<HtmlNode> query = html.DocumentNode.Descendants("div");
            foreach (HtmlNode item in query.ToList())
            {
                if (item.Attributes.Where(a => a.Name == "data-block-guid").FirstOrDefault() != null && !string.IsNullOrEmpty(item.Attributes.Where(a => a.Name == "data-block-guid").FirstOrDefault().Value))
                {
                    Dictionary<string, string> Attributes = new Dictionary<string, string>();
                    foreach (HtmlAttribute attr in item.Attributes)
                    {
                        Attributes.Add(attr.Name, attr.Value);
                    }

                    ThemeTemplateResponse response = Core.Managers.BlockManager.Render(Attributes);
                    if (response != null)
                    {
                        string Markup = response.Markup;
                        if (item.Attributes.Where(a => a.Name == "data-block-type").FirstOrDefault().Value.ToLower() == "global")
                        {
                            Markup = ProcessGlobalBlock(Markup, Result);
                        }

                        Result.BlocksMarkUp += Markup;
                        if (response.Scripts != null)
                        {
                            foreach (string script in response.Scripts)
                            {
                                if (!string.IsNullOrEmpty(script))
                                {
                                    Result.Scripts.Add(script);
                                }
                            }
                        }

                        if (response.Styles != null)
                        {
                            foreach (string style in response.Styles)
                            {
                                if (!string.IsNullOrEmpty(style))
                                {
                                    Result.Styles.Add(style);
                                }
                            }
                        }

                        if (!string.IsNullOrEmpty(response.Script))
                        {
                            Result.Script += response.Script;
                        }

                        if (!string.IsNullOrEmpty(response.Style))
                        {
                            Result.Style += response.Style;
                        }
                    }

                }
            }
            Result.html = html.DocumentNode.OuterHtml;
        }

        private string ProcessGlobalBlock(string MarkUp, dynamic Result)
        {
            HtmlDocument globalhtml = new HtmlDocument();
            globalhtml.LoadHtml(MarkUp);
            IEnumerable<HtmlNode> query = globalhtml.DocumentNode.Descendants("div");
            foreach (HtmlNode item in query.ToList())
            {
                if (item.Attributes.Where(a => a.Name == "data-block-guid").FirstOrDefault() != null && !string.IsNullOrEmpty(item.Attributes.Where(a => a.Name == "data-block-guid").FirstOrDefault().Value))
                {
                    if (item.Attributes.Where(a => a.Name == "data-block-type").FirstOrDefault().Value.ToLower() != "global")
                    {
                        Dictionary<string, string> Attributes = new Dictionary<string, string>();
                        foreach (HtmlAttribute attr in item.Attributes)
                        {
                            Attributes.Add(attr.Name, attr.Value);
                        }

                        ThemeTemplateResponse response = Core.Managers.BlockManager.Render(Attributes);
                        if (response != null)
                        {
                            item.InnerHtml = response.Markup;
                            if (response.Scripts != null)
                            {
                                foreach (string script in response.Scripts)
                                {
                                    if (!string.IsNullOrEmpty(script))
                                    {
                                        Result.Scripts.Add(script);
                                    }
                                }
                            }

                            if (response.Styles != null)
                            {
                                foreach (string style in response.Styles)
                                {
                                    if (!string.IsNullOrEmpty(style))
                                    {
                                        Result.Styles.Add(style);
                                    }
                                }
                            }

                            if (!string.IsNullOrEmpty(response.Script))
                            {
                                Result.Script += response.Script;
                            }

                            if (!string.IsNullOrEmpty(response.Style))
                            {
                                Result.Style += response.Style;
                            }
                        }
                    }
                }
            }
            MarkUp = globalhtml.DocumentNode.OuterHtml;
            return MarkUp;
        }

        public override string AccessRoles()
        {
            return AppFactory.GetAccessRoles(UserInfo);
        }
    }
}