using DotNetNuke.Abstractions;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Modules.Definitions;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Tabs;
using DotNetNuke.Entities.Users;
using DotNetNuke.Framework;
using DotNetNuke.Security;
using DotNetNuke.Security.Permissions;
using DotNetNuke.Services.FileSystem;
using DotNetNuke.Services.Localization;
using HtmlAgilityPack;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.UI;
using Vanjaro.Common.ASPNET;
using Vanjaro.Common.Factories;
using Vanjaro.Common.Utilities;
using Vanjaro.Core.Components;
using Vanjaro.Core.Components.Interfaces;
using Vanjaro.Core.Data.Entities;
using Vanjaro.Core.Data.Scripts;
using Vanjaro.Core.Entities;
using static Vanjaro.Core.Components.Enum;
using static Vanjaro.Core.Factories;

namespace Vanjaro.Core
{
    public static partial class Managers
    {
        public class PageManager : IReviewComment
        {

            private const string PortalRootToken = "{{PortalRoot}}";
            public const string ExportTemplateRootToken = "{{TemplateRoot}}";
            public static void Init(Page Page, PortalSettings PortalSettings)
            {
                string Markup = GetReviewToastMarkup(PortalSettings);
                if (!string.IsNullOrEmpty(Markup) && string.IsNullOrEmpty(Page.Request.QueryString["icp"]) && string.IsNullOrEmpty(Page.Request.QueryString["guid"]))
                {
                    WebForms.RegisterStartupScript(Page, "WorkflowReview", "<script type=\"text/javascript\" vanjarocore=\"true\">" + Markup + "</script>", false);
                }
            }
            public static bool InjectEditor(PortalSettings PortalSettings)
            {
                if (PortalSettings.UserId > 0 && TabPermissionController.CanViewPage() && (TabPermissionController.HasTabPermission("EDIT") || !Editor.Options.EditPage))
                    return true;
                else
                    return false;
            }
            public static string GetReviewToastMarkup(PortalSettings PortalSettings)
            {
                string Markup = string.Empty;
                Pages page = GetLatestVersion(PortalSettings.ActiveTab.TabID, PortalSettings.UserInfo);
                WorkflowState State = null;
                bool ShowReview = false;

                if (page != null && page.StateID.HasValue)
                {
                    State = WorkflowManager.GetStateByID(page.StateID.Value);
                    if (State != null && WorkflowManager.HasReviewPermission(page.StateID.Value, PortalSettings.UserInfo))
                    {
                        ShowReview = WorkflowManager.GetWorkflowType(State.WorkflowID) == WorkflowTypes.ContentApproval && !WorkflowManager.IsFirstState(State.WorkflowID, State.StateID) && !WorkflowManager.IsLastState(State.WorkflowID, State.StateID);
                    }
                }

                if (State != null)
                {
                    string URL = ServiceProvider.NavigationManager.NavigateURL("", "mid=0", "icp=true", "guid=33d8efed-0f1d-471e-80a4-6a7f10e87a42");
                    URL += "#!/moderator?version=" + page.Version + "&entity=" + WorkflowType.Page.ToString() + "&entityid=" + page.TabID;
                    string ReviewChangesBtn = ShowReview ? "ReviewChangeMarkup.append(ReviewChangesBtn);" : string.Empty;
                    string Subject = ShowReview ? State.Name : DotNetNuke.Services.Localization.Localization.GetString("PendingReview", Components.Constants.LocalResourcesFile);
                    string Message = !ShowReview ? "ReviewChangeMarkup.append('" + DotNetNuke.Services.Localization.Localization.GetString("ThisPageIsWaiting", Components.Constants.LocalResourcesFile) + "');" : string.Empty;
                    string UnlockChangesBtn = TabPermissionController.CanManagePage(PortalSettings.ActiveTab) ? "ReviewChangeMarkup.append(UnlockChangesBtn);" : string.Empty;
                    string LocalCanvasMarkup = string.Empty;
                    string FirstStateName = WorkflowManager.GetFirstStateID(State.WorkflowID).Name;
                    string ReviewChangeFunction = !ShowReview ? "ConfirmReviewChange('" + FirstStateName + "');" : "$('.gjs-cv-canvas__frames').removeClass('lockcanvas'); VJIsLocked='False'; $('.toast-close-button').click();";
                    if (State != null && ShowReview || (!string.IsNullOrEmpty(UnlockChangesBtn) && !WorkflowManager.IsFirstState(State.WorkflowID, State.StateID) && !WorkflowManager.IsLastState(State.WorkflowID, State.StateID)))
                    {
                        Markup = "var ReviewChangeMarkup=$('<div>'); var ReviewChangesBtn = $('<button>'); ReviewChangesBtn.text('" + DotNetNuke.Services.Localization.Localization.GetString("ReviewPage", Components.Constants.LocalResourcesFile) + "'); ReviewChangesBtn.addClass('btn btn-success btn-sm'); ReviewChangesBtn.click(function(){OpenPopUp(null,600,'right','','" + URL + "'); }); var UnlockChangesBtn = $('<button>'); UnlockChangesBtn.text('" + DotNetNuke.Services.Localization.Localization.GetString("MakeChanges", Components.Constants.LocalResourcesFile) + "'); UnlockChangesBtn.addClass('btn btn-danger btn-sm'); UnlockChangesBtn.click(function(){" + ReviewChangeFunction + " });" + Message + " " + ReviewChangesBtn + " " + UnlockChangesBtn + " window.parent.ShowNotification('" + Subject + "',ReviewChangeMarkup, 'info', '', false,false);" + LocalCanvasMarkup + "";
                    }
                }

                return Markup;
            }

            internal static void BuildCustomBlocks(int portalID, dynamic contentJSON, dynamic styleJSON)
            {
                if (contentJSON != null)
                {
                    foreach (dynamic con in contentJSON)
                    {
                        if (con.attributes != null && con.attributes["data-custom-wrapper"] != null && con.attributes["data-guid"] != null)
                        {
                            bool isLibrary = false;
                            try
                            {
                                if (con.attributes["data-islibrary"] != null)
                                    isLibrary = Convert.ToBoolean(con.attributes["data-islibrary"].Value);
                            }
                            catch { }
                            CustomBlock block = BlockManager.GetCustomByGuid(portalID, con.attributes["data-guid"].Value, isLibrary);
                            if (block != null)
                            {
                                string prefix = con.attributes["id"] != null ? con.attributes["id"].Value : string.Empty;
                                dynamic blockContentJSON = JsonConvert.DeserializeObject(block.ContentJSON);
                                List<string> Ids = new List<string>();
                                if (blockContentJSON != null)
                                {
                                    List<string> conKeys = new List<string>();
                                    foreach (JProperty prop in con.Properties())
                                        conKeys.Add(prop.Name);
                                    foreach (string prop in conKeys)
                                        (con as JObject).Remove(prop);
                                    if (blockContentJSON.Count > 1)
                                    {
                                        (con as JObject).Add("type", "div");
                                        (con as JObject).Add("components", blockContentJSON);
                                        if (isLibrary)
                                            (con as JObject).Add("forcesave", "true");
                                    }
                                    else
                                    {
                                        foreach (JProperty prop in blockContentJSON[0].Properties())
                                            (con as JObject).Add(prop.Name, prop.Value);
                                        if (isLibrary)
                                            (con as JObject).Add("forcesave", "true");
                                    }
                                    UpdateIds(con, prefix, Ids);
                                }
                                dynamic blockStyleJSON = JsonConvert.DeserializeObject(block.StyleJSON);
                                if (blockStyleJSON != null)
                                {
                                    foreach (dynamic style in blockStyleJSON)
                                    {
                                        if (!StyleExists(style, styleJSON))
                                        {
                                            if (!string.IsNullOrEmpty(prefix) && style.selectors != null)
                                            {
                                                foreach (dynamic st in style.selectors)
                                                {
                                                    if (st.name != null)
                                                    {
                                                        string val = st.name;
                                                        if (!Ids.Contains(val))
                                                            st.name.Value = prefix + "-" + val.Split('-').Last();
                                                    }
                                                }
                                            }
                                            styleJSON.Add(style);
                                        }
                                    }
                                }
                                if (isLibrary)
                                    BlockManager.DeleteCustom(portalID, block.Guid);
                            }
                        }
                        else if (con.components != null)
                        {
                            BuildCustomBlocks(portalID, con.components, styleJSON);
                        }
                    }
                }
            }

            private static bool StyleExists(dynamic style, dynamic styleJSON)
            {
                bool result = false;
                if (styleJSON != null && style != null && style.selectors != null)
                {
                    foreach (dynamic con in styleJSON)
                    {
                        if (result)
                            break;
                        if (con.selectors != null)
                        {
                            foreach (dynamic cons in con.selectors)
                            {
                                if (result)
                                    break;
                                foreach (dynamic st in style.selectors)
                                {
                                    if (st.name != null && cons.name != null && cons.name.Value == st.name.Value)
                                    {
                                        result = true;
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }
                return result;
            }

            internal static void RemoveGlobalBlockComponents(dynamic contentJSON, Dictionary<string, List<string>> StyleIds, Dictionary<string, dynamic> GlobalKeyValuePairs, dynamic DeserializedGlobalBlocksJSON)
            {
                if (contentJSON != null)
                {
                    foreach (dynamic con in contentJSON)
                    {
                        if (con.type != null && con.type.Value == "module")
                        {
                            con.content = "";
                        }
                        else if (con.type != null && con.type.Value == "blockwrapper")
                        {
                            con.content = "";
                        }
                        else if (con.type != null && con.type.Value == "globalblockwrapper")
                        {
                            if (con.attributes != null && con.attributes["data-guid"] != null && !GlobalKeyValuePairs.ContainsKey(con.attributes["data-guid"].Value))
                            {
                                string ccid = string.Empty;
                                if (con.attributes["id"] != null)
                                    ccid = con.attributes["id"].Value;
                                if (IsGlobalBlockUnlocked(ccid, con.attributes["data-guid"].Value, DeserializedGlobalBlocksJSON))
                                    GlobalKeyValuePairs.Add(con.attributes["data-guid"].Value, con.components);
                                ExtractStyleIds(con.attributes["data-guid"].Value, con.components, StyleIds);
                                (con as JObject).Remove("components");
                            }
                        }
                        else if (con.components != null)
                        {
                            RemoveGlobalBlockComponents(con.components, StyleIds, GlobalKeyValuePairs, DeserializedGlobalBlocksJSON);
                        }
                    }
                }
            }

            private static bool IsGlobalBlockUnlocked(string ccid, string guid, dynamic blocksJSON)
            {
                if (blocksJSON != null && blocksJSON.Count > 0)
                {
                    foreach (dynamic con in blocksJSON)
                    {
                        if (con.guid != null && con.guid.Value == guid)
                        {
                            if (!string.IsNullOrEmpty(ccid) && con.ccid != null)
                            {
                                if (con.ccid.Value == ccid)
                                    return true;
                            }
                            else
                                return true;
                        }
                    }
                }
                return false;
            }

            internal static void RemoveGlobalBlockStyles(dynamic contentJSON, Dictionary<string, List<string>> StyleIds, Dictionary<string, dynamic> GlobalStyleKeyValuePairs)
            {
                if (contentJSON != null)
                {
                    Dictionary<string, List<dynamic>> itemsToRemove = new Dictionary<string, List<dynamic>>();
                    foreach (dynamic con in contentJSON)
                    {
                        if (con.selectors != null)
                        {
                            foreach (dynamic cons in con.selectors)
                            {
                                bool breaked = false;
                                foreach (var style in StyleIds)
                                {
                                    if (cons.name != null && style.Value.Contains(cons.name.Value))
                                    {
                                        if (itemsToRemove.ContainsKey(style.Key))
                                        {
                                            List<dynamic> existing = itemsToRemove[style.Key];
                                            existing.Add(con);
                                            itemsToRemove[style.Key] = existing;
                                        }
                                        else
                                        {
                                            List<dynamic> obj = new List<dynamic>();
                                            obj.Add(con);
                                            itemsToRemove.Add(style.Key, obj);
                                        }
                                        breaked = true;
                                        break;
                                    }
                                }
                                if (breaked)
                                    break;
                            }
                        }
                    }
                    foreach (var item in itemsToRemove)
                    {
                        foreach (var con in item.Value)
                        {
                            contentJSON.Remove(con);
                        }
                        GlobalStyleKeyValuePairs.Add(item.Key, item.Value);
                    }
                }
            }

            private static void ExtractStyleIds(string guid, dynamic components, Dictionary<string, List<string>> styleIds)
            {
                if (components != null)
                {
                    foreach (dynamic con in components)
                    {
                        if (con.attributes != null && con.attributes["id"] != null)
                        {
                            if (styleIds.ContainsKey(guid))
                            {
                                List<string> existing = styleIds[guid];
                                existing.Add(con.attributes["id"].Value);
                                styleIds[guid] = existing;
                            }
                            else
                            {
                                List<string> obj = new List<string>();
                                obj.Add(con.attributes["id"].Value);
                                styleIds.Add(guid, obj);
                            }
                        }
                        if (con.components != null)
                        {
                            ExtractStyleIds(guid, con.components, styleIds);
                        }
                    }
                }
            }

            public static dynamic Update(PortalSettings PortalSettings, dynamic Data)
            {
                dynamic result = new ExpandoObject();
                try
                {
                    if (Data != null)
                    {
                        int TabId = PortalSettings.ActiveTab.TabID;
                        Pages page = new Pages();
                        Pages pageVersion = GetLatestVersion(PortalSettings.ActiveTab.TabID, PortalSettings.UserInfo);
                        var aliases = from PortalAliasInfo pa in PortalAliasController.Instance.GetPortalAliasesByPortalId(PortalSettings.PortalId)
                                      select pa.HTTPAlias;
                        page.TabID = TabId;
                        string Css = Data["gjs-css"].ToString();
                        page.Content = AbsoluteToRelativeUrls(ResetModuleMarkup(PortalSettings.PortalId, Data["gjs-html"].ToString(), ref Css, PortalSettings.UserId), aliases);

                        List<string> Ids = new List<string>();
                        Dictionary<string, List<string>> StyleIds = new Dictionary<string, List<string>>();
                        Dictionary<string, dynamic> GlobalKeyValuePairs = new Dictionary<string, dynamic>();
                        Dictionary<string, dynamic> GlobalStyleKeyValuePairs = new Dictionary<string, dynamic>();
                        try { var globalblocks = Data["gjs-globalblocks"]; }
                        catch { Data["gjs-globalblocks"] = ""; }
                        var DeserializedGlobalBlocksJSON = Data["gjs-globalblocks"] != null ? JsonConvert.DeserializeObject(Data["gjs-globalblocks"].ToString()) : string.Empty;
                        var DeserializedContentJSON = JsonConvert.DeserializeObject(Data["gjs-components"].ToString());
                        var DeserializedStyleJSON = JsonConvert.DeserializeObject(Data["gjs-styles"].ToString());
                        GetAllIds(DeserializedContentJSON, Ids);
                        FilterStyle(DeserializedStyleJSON, Ids);
                        RemoveGlobalBlockComponents(DeserializedContentJSON, StyleIds, GlobalKeyValuePairs, DeserializedGlobalBlocksJSON);
                        RemoveGlobalBlockStyles(DeserializedStyleJSON, StyleIds, GlobalStyleKeyValuePairs);
                        BuildCustomBlocks(PortalSettings.PortalId, DeserializedContentJSON, DeserializedStyleJSON);
                        page.ContentJSON = AbsoluteToRelativeUrls(JsonConvert.SerializeObject(DeserializedContentJSON), aliases);
                        page.StyleJSON = JsonConvert.SerializeObject(DeserializedStyleJSON);
                        page.Style = FilterCss(Css, StyleIds);

                        if (Data["IsPublished"] != null && Convert.ToBoolean(Data["IsPublished"].ToString()) && (pageVersion != null && pageVersion.IsPublished))
                        {
                            page.IsPublished = true;
                            page.Version = GetNextVersionByTabID(TabId);
                        }
                        else
                        {
                            page.IsPublished = Convert.ToBoolean(Data["IsPublished"].ToString());

                            if (pageVersion != null && pageVersion.IsPublished)
                            {
                                page.Version = GetNextVersionByTabID(TabId);
                            }
                            else if (pageVersion != null && !pageVersion.IsPublished)
                            {
                                page.Version = pageVersion.Version;
                            }
                            else
                            {
                                page.Version = GetNextVersionByTabID(TabId);
                            }
                        }

                        Pages SavedPage = GetByVersion(page.TabID, page.Version, GetCultureCode(PortalSettings));
                        if (SavedPage != null)
                        {
                            page.CreatedBy = SavedPage.CreatedBy;
                            page.CreatedOn = SavedPage.CreatedOn;
                            page.ID = SavedPage.ID;
                            page.StateID = SavedPage.StateID;
                        }
                        page.Locale = GetCultureCode(PortalSettings);
                        if (pageVersion != null && page.Version == pageVersion.Version)
                        {
                            page.StateID = pageVersion.StateID;
                        }

                        page.PortalID = PortalSettings.PortalId;
                        AddComment(PortalSettings, "publish", Data["Comment"].ToString(), page);

                        ReviewSettings ReviewSettings = GetPageReviewSettings(PortalSettings);
                        if (!string.IsNullOrEmpty(Data["Comment"].ToString()) || ReviewSettings.IsPageDraft)
                        {
                            result.ReviewToastMarkup = GetReviewToastMarkup(PortalSettings);
                            result.PageReviewSettings = ReviewSettings;
                        }
                        result.NotifyCount = NotificationManager.RenderNotificationsCount(PortalSettings.PortalId);

                        try
                        {
                            if (page.IsPublished && Data != null && Data["m2v"] != null && Data["m2v"].Value)
                            {
                                PageManager.MigrateToVanjaro(PortalSettings);
                                result.RedirectAfterm2v = Common.Utilities.ServiceProvider.NavigationManager.NavigateURL(PortalSettings.ActiveTab.TabID);
                            }
                            else
                            {
                                result.RedirectAfterm2v = null;
                            }
                        }
                        catch (Exception ex)
                        {
                            result.IsSuccess = false;
                            result.Message = ex.Message;
                        }

                        result.IsSuccess = true;
                        result.ShowNotification = Data["IsPublished"];

                        UpdateGlobalBlocks(PortalSettings, GlobalKeyValuePairs, GlobalStyleKeyValuePairs, DeserializedGlobalBlocksJSON, aliases, Convert.ToBoolean(Data["IsPublished"].ToString()));
                    }
                }
                catch (Exception ex)
                {
                    result.IsSuccess = false;
                    result.Message = ex.Message;
                }
                return result;
            }

            internal static void FilterStyle(dynamic styleJSON, List<string> ids)
            {
                if (styleJSON != null)
                {
                    List<dynamic> itemsToRemove = new List<dynamic>();
                    foreach (dynamic con in styleJSON)
                    {
                        List<string> selectorIds = new List<string>();
                        if (con.selectors != null)
                        {
                            foreach (dynamic cons in con.selectors)
                            {
                                if (cons.name != null)
                                    selectorIds.Add(cons.name.Value);
                            }
                        }
                        bool delete = true;
                        foreach (string selectorId in selectorIds)
                        {
                            if (ids.Contains(selectorId))
                            {
                                delete = false;
                                break;
                            }
                        }
                        if (delete)
                            itemsToRemove.Add(con);
                    }
                    foreach (var item in itemsToRemove)
                        styleJSON.Remove(item);
                }
            }

            internal static void GetAllIds(dynamic contentJSON, List<string> ids)
            {
                if (contentJSON != null)
                {
                    foreach (dynamic con in contentJSON)
                    {
                        if (con.attributes != null && con.attributes["id"] != null)
                            ids.Add(con.attributes["id"].Value);
                        if (con.components != null)
                            GetAllIds(con.components, ids);
                    }
                }
            }

            internal static string FilterCss(string css, Dictionary<string, List<string>> styleIds)
            {
                if (styleIds.Count > 0)
                {
                    foreach (var item in styleIds)
                    {
                        foreach (string style in item.Value)
                            css = Regex.Replace(css, @"(#" + style + @"*\s*{[^}]*})|(#" + style + @":[^{]*\s*{[^}]*})", string.Empty);
                    }
                }
                return css;
            }

            private static void UpdateGlobalBlocks(PortalSettings portalSettings, Dictionary<string, dynamic> globalKeyValuePairs, Dictionary<string, dynamic> globalStyleKeyValuePairs, dynamic blocksJSON, IEnumerable<string> aliases, bool IsPublished)
            {
                foreach (var item in globalKeyValuePairs)
                {
                    GlobalBlock block = BlockManager.GetGlobalByGuid(portalSettings.PortalId, item.Key);
                    if (block != null)
                    {
                        block.ContentJSON = PageManager.DeTokenizeLinks(JsonConvert.SerializeObject(item.Value), portalSettings.PortalId);
                        if (globalStyleKeyValuePairs.ContainsKey(item.Key))
                            block.StyleJSON = PageManager.DeTokenizeLinks(JsonConvert.SerializeObject(globalStyleKeyValuePairs[item.Key]), portalSettings.PortalId);
                        if (blocksJSON != null && blocksJSON.Count > 0)
                        {
                            foreach (dynamic con in blocksJSON)
                            {
                                if (con.guid != null && con.guid.Value == item.Key)
                                {
                                    string Css = con.css.Value;
                                    block.Html = DeTokenizeLinks(AbsoluteToRelativeUrls(ResetModuleMarkup(portalSettings.PortalId, con.html.Value, ref Css, portalSettings.UserId), aliases), portalSettings.PortalId);
                                    List<string> Ids = new List<string>();
                                    Dictionary<string, List<string>> StyleIds = new Dictionary<string, List<string>>();
                                    Dictionary<string, dynamic> GlobalKeyValuePairs = new Dictionary<string, dynamic>();
                                    Dictionary<string, dynamic> GlobalStyleKeyValuePairs = new Dictionary<string, dynamic>();
                                    var DeserializedContentJSON = JsonConvert.DeserializeObject(block.ContentJSON);
                                    var DeserializedStyleJSON = block.StyleJSON != null ? JsonConvert.DeserializeObject(block.StyleJSON) : string.Empty;
                                    GetAllIds(DeserializedContentJSON, Ids);
                                    FilterStyle(DeserializedStyleJSON, Ids);
                                    RemoveGlobalBlockComponents(DeserializedContentJSON, StyleIds, GlobalKeyValuePairs, null);
                                    RemoveGlobalBlockStyles(DeserializedStyleJSON, StyleIds, GlobalStyleKeyValuePairs);
                                    BuildCustomBlocks(portalSettings.PortalId, DeserializedContentJSON, DeserializedStyleJSON);
                                    block.ContentJSON = DeTokenizeLinks(AbsoluteToRelativeUrls(JsonConvert.SerializeObject(DeserializedContentJSON), aliases), portalSettings.PortalId);
                                    block.Css = FilterCss(Css, StyleIds);
                                    block.StyleJSON = DeTokenizeLinks(JsonConvert.SerializeObject(DeserializedStyleJSON), portalSettings.PortalId);
                                }
                            }
                        }
                        if (block.IsPublished)
                        {
                            block.ID = 0;
                            block.CreatedOn = DateTime.UtcNow;
                            block.CreatedBy = PortalSettings.Current.UserInfo.UserID;
                        }
                        if (IsPublished)
                        {
                            block.PublishedBy = PortalSettings.Current.UserInfo.UserID;
                            block.IsPublished = IsPublished;
                            block.PublishedOn = DateTime.UtcNow;
                        }

                        block.UpdatedOn = DateTime.UtcNow;
                        block.UpdatedBy = PortalSettings.Current.UserInfo.UserID;
                        BlockManager.UpdateGlobalBlock(block);
                    }
                }
            }

            public static string AbsoluteToRelativeUrls(string content, IEnumerable<string> aliases)
            {
                if (string.IsNullOrWhiteSpace(content))
                    return string.Empty;

                foreach (string portalAlias in aliases)
                {
                    content = content.Replace("href=\"http://" + portalAlias, "href=\"").Replace("href=\"https://" + portalAlias, "href=\"").Replace("src=\"http://" + portalAlias, "src=\"").Replace("src=\"https://" + portalAlias, "src=\"");
                    content = content.Replace("href\":\"http://" + portalAlias, "href\":\"").Replace("href\":\"https://" + portalAlias, "href\":\"").Replace("src\":\"http://" + portalAlias, "src\":\"").Replace("src\":\"https://" + portalAlias, "src\":\"");
                }
                return content;
            }
            public static void MigrateToVanjaro(PortalSettings PortalSettings)
            {
                PageFactory.MigrateToVanjaro(PortalSettings);
            }

            public static void Rollback(int TabId, int Version, string Locale, int UserID)
            {
                Pages page = GetByVersion(TabId, Version, Locale);
                page.Version = GetNextVersionByTabID(TabId);
                PageFactory.Update(page, UserID);
            }

            public static dynamic Get(int TabID)
            {
                Pages tab = PageFactory.Get(TabID);
                Dictionary<string, object> result = new Dictionary<string, object>();
                if (tab != null)
                {
                    result.Add("gjs-css", tab.Style);
                    result.Add("gjs-styles", tab.StyleJSON);
                    result.Add("gjs-html", tab.Content);
                    result.Add("gjs-component", tab.ContentJSON);
                }
                return result;
            }

            public static List<Pages> GetPages(int TabID, bool HasTabEditPermission = true)
            {
                return PageFactory.GetAllByTabID(TabID, HasTabEditPermission).ToList();
            }

            public static string ResetModuleMarkup(int PortalId, string Markup, ref string Css, int UserId)
            {
                if (!string.IsNullOrEmpty(Markup))
                {
                    Dictionary<string, string> keyValuePairs = new Dictionary<string, string>();
                    HtmlDocument html = new HtmlDocument();
                    html.LoadHtml(Markup);
                    IEnumerable<HtmlNode> query = html.DocumentNode.Descendants("div");
                    foreach (HtmlNode item in query.ToList())
                    {
                        if (item.Attributes.Where(a => a.Name == "dmid").FirstOrDefault() != null && item.Attributes.Where(a => a.Name == "mid").FirstOrDefault() != null)
                        {
                            item.InnerHtml = "<div vjmod=\"true\"><app id=\"" + item.Attributes.Where(a => a.Name == "mid").FirstOrDefault().Value + "\"></app>";
                        }
                        else if (item.Attributes.Where(a => a.Name == "data-block-type").FirstOrDefault() != null)
                        {
                            if (item.Attributes.Where(a => a.Name == "data-block-type").FirstOrDefault().Value.ToLower() == "global")
                            {
                                item.InnerHtml = "";
                            }
                            else
                            {
                                item.InnerHtml = item.Attributes.Where(a => a.Name == "data-block-type").FirstOrDefault().Value;
                            }
                        }
                        else if (item.Attributes.Where(a => a.Name == "data-custom-wrapper").FirstOrDefault() != null && item.Attributes.Where(a => a.Name == "data-guid").FirstOrDefault() != null)
                        {
                            keyValuePairs.Add(item.OuterHtml, string.Empty);
                        }
                    }
                    Markup = html.DocumentNode.OuterHtml;
                    foreach (var keyValue in keyValuePairs)
                        Markup = Markup.Replace(keyValue.Key, keyValue.Value);
                }
                return Markup;
            }

            public static int GetNextVersionByTabID(int TabID)
            {
                List<Pages> pages = PageFactory.GetAllByTabID(TabID);
                if (pages.Count > 0)
                {
                    return pages.Max(a => a.Version) + 1;
                }

                return 1;
            }

            public static void Delete(int TabID)
            {
                PageFactory.Delete(TabID);
            }
            public static void Delete(int TabID, int Version)
            {
                PageFactory.Delete(TabID, Version);
            }

            public static Pages GetLatestVersion(int TabID, string Locale)
            {
                return GetLatestVersion(TabID, Locale, true);
            }

            public static Pages GetLatestVersion(int TabID, string Locale, bool GetDefaultLocale)
            {
                return GetLatestVersion(TabID, false, Locale, GetDefaultLocale);
            }

            public static Pages GetLatestVersion(int TabID, bool IgnoreDraft, string Locale, bool GetDefaultLocale)
            {
                UserInfo UserInfo = (PortalController.Instance.GetCurrentSettings() as PortalSettings).UserInfo;
                bool HasTabEditPermission = TabPermissionController.HasTabPermission("EDIT") || WorkflowManager.HasReviewPermission(UserInfo);
                List<Pages> pages = GetPages(TabID, HasTabEditPermission).Where(a => a.Locale == Locale).ToList();
                Pages page = new Pages();

                if (!IgnoreDraft && HasTabEditPermission)
                {
                    page = pages.OrderByDescending(a => a.Version).FirstOrDefault();
                }
                else
                {
                    page = pages.Where(a => a.IsPublished == true).OrderByDescending(a => a.Version).FirstOrDefault();
                }

                if (page != null && !TabPermissionController.HasTabPermission("EDIT") && !WorkflowManager.HasReviewPermission(page.StateID.Value, UserInfo))
                {
                    page = pages.Where(a => a.IsPublished == true).OrderByDescending(a => a.Version).FirstOrDefault();
                }

                if (page == null && !string.IsNullOrEmpty(Locale) && GetDefaultLocale)
                {
                    return GetLatestVersion(TabID, IgnoreDraft, null, false);
                }
                return page;
            }

            public static void ChangePageWorkflow(PortalSettings PortalSettings, int TabID, int Workflowid)
            {

                SettingManager.UpdateValue(PortalSettings.PortalId, TabID, "setting_workflow", "WorkflowID", Workflowid.ToString());
                WorkflowState fstate = WorkflowManager.GetFirstStateID(Workflowid);
                if (fstate != null)
                {
                    foreach (string Locale in GetCultureListItems())
                    {
                        string _Locale = Locale == PortalSettings.DefaultLanguage ? null : Locale;
                        Pages page = GetLatestVersion(TabID, _Locale);
                        if (page != null && page.StateID.HasValue)
                        {
                            WorkflowState state = WorkflowManager.GetStateByID(page.StateID.Value);
                            if (page.StateID.HasValue && WorkflowManager.IsFirstState(state.WorkflowID, state.StateID))
                            {
                                page.StateID = fstate.StateID;
                                UpdatePage(page, PortalSettings.UserId);
                            }
                        }
                    }
                }
            }

            public static Pages GetLatestVersion(int TabID, UserInfo UserInfo)
            {

                List<Pages> pages = GetPages(TabID).ToList();
                Pages page = new Pages();

                try
                {
                    if (TabPermissionController.HasTabPermission("EDIT") || WorkflowManager.HasReviewPermission(UserInfo))
                    {
                        page = pages.OrderByDescending(a => a.Version).FirstOrDefault();
                    }
                    else
                    {
                        page = pages.Where(a => a.IsPublished == true).OrderByDescending(a => a.Version).FirstOrDefault();
                    }
                }
                catch (Exception)
                {
                    page = new Pages();
                }
                return page;
            }


            public static Pages GetByVersion(int TabID, int Version, string Locale)
            {
                return PageFactory.GetByVersion(TabID, Version, Locale);
            }

            public static List<Pages> GetLatestLocaleVersion(int TabID)
            {
                return GetPages(TabID).Where(a => a.IsPublished == true).OrderByDescending(a => a.Version).GroupBy(g => g.Version).FirstOrDefault()?.ToList() ?? new List<Pages>();
            }

            public static void AddModules(PortalSettings PortalSettings, Dictionary<string, object> LayoutData, UserInfo userInfo, string portableModulesPath)
            {
                string Markup = LayoutData["gjs-html"].ToString();
                string MarkupJson = string.Empty;
                if (LayoutData.ContainsKey("gjs-components"))
                    MarkupJson = LayoutData["gjs-components"].ToString();
                if (!string.IsNullOrEmpty(Markup))
                {
                    HtmlDocument html = new HtmlDocument();
                    html.LoadHtml(Markup);
                    IEnumerable<HtmlNode> query = html.DocumentNode.Descendants("div");
                    foreach (HtmlNode item in query.ToList())
                    {
                        if (item.Attributes.Where(a => a.Name == "dmid").FirstOrDefault() != null && item.Attributes.Where(a => a.Name == "mid").FirstOrDefault() != null && item.Attributes.Where(a => a.Name == "fname").FirstOrDefault() != null)
                        {
                            ModuleDefinitionInfo moduleDefinition = ModuleDefinitionController.GetModuleDefinitionByFriendlyName(item.Attributes.Where(a => a.Name == "fname").FirstOrDefault().Value);
                            if (moduleDefinition != null)
                            {
                                ModuleInfo objModule = new ModuleInfo();
                                objModule.Initialize(PortalSettings.ActiveTab.PortalID);
                                objModule.PortalID = PortalSettings.ActiveTab.PortalID;
                                objModule.TabID = PortalSettings.ActiveTab.TabID;
                                objModule.ModuleOrder = -1;
                                objModule.ModuleTitle = moduleDefinition.FriendlyName;
                                objModule.PaneName = "ContentPane";
                                objModule.ModuleDefID = moduleDefinition.ModuleDefID;
                                objModule.ContainerSrc = "[g]containers/vanjaro/base.ascx";
                                objModule.DisplayTitle = true;

                                if (moduleDefinition.DefaultCacheTime > 0)
                                {
                                    objModule.CacheTime = moduleDefinition.DefaultCacheTime;
                                    if (PortalSettings.DefaultModuleId > Null.NullInteger && PortalSettings.DefaultTabId > Null.NullInteger)
                                    {
                                        ModuleInfo defaultModule = ModuleController.Instance.GetModule(PortalSettings.DefaultModuleId, PortalSettings.DefaultTabId, true);
                                        if ((defaultModule != null))
                                            objModule.CacheTime = defaultModule.CacheTime;
                                    }
                                }
                                ModuleController.Instance.InitialModulePermission(objModule, objModule.TabID, 0);
                                for (int i = 0; i < objModule.ModulePermissions.Count; i++)
                                {
                                    if (objModule.ModulePermissions[i].RoleID == 0)
                                        objModule.ModulePermissions[i].RoleID = PortalSettings.AdministratorRoleId;

                                }
                                if (PortalSettings.ContentLocalizationEnabled)
                                {
                                    Locale defaultLocale = LocaleController.Instance.GetDefaultLocale(PortalSettings.PortalId);
                                    //set the culture of the module to that of the tab
                                    TabInfo tabInfo = TabController.Instance.GetTab(objModule.TabID, PortalSettings.PortalId, false);
                                    objModule.CultureCode = tabInfo != null ? tabInfo.CultureCode : defaultLocale.Code;
                                }
                                else
                                    objModule.CultureCode = Null.NullString;
                                objModule.AllTabs = false;
                                ModuleController.Instance.AddModule(objModule);
                                int oldDmid = int.Parse(item.Attributes.Where(a => a.Name == "dmid").FirstOrDefault().Value);
                                int oldMid = int.Parse(item.Attributes.Where(a => a.Name == "mid").FirstOrDefault().Value);
                                int newDmid = moduleDefinition.DesktopModuleID;
                                int newMid = objModule.ModuleID;
                                item.Attributes.Where(a => a.Name == "dmid").FirstOrDefault().Value = newDmid.ToString();
                                item.Attributes.Where(a => a.Name == "mid").FirstOrDefault().Value = newMid.ToString();
                                item.InnerHtml = "<div vjmod=\"true\"><app id=\"" + newMid + "\"></app>";
                                MarkupJson = MarkupJson.Replace("\"dmid\":\"" + oldDmid + "\",\"mid\":" + oldMid + "", "\"dmid\":\"" + newDmid + "\",\"mid\":" + newMid);
                                if (Directory.Exists(portableModulesPath) && File.Exists(portableModulesPath + "/" + oldMid + ".json"))
                                {
                                    var desktopModuleInfo = DesktopModuleController.GetDesktopModule(newDmid, PortalSettings.PortalId);
                                    if (!string.IsNullOrEmpty(desktopModuleInfo?.BusinessControllerClass))
                                    {
                                        if (!objModule.IsDeleted && !string.IsNullOrEmpty(desktopModuleInfo.BusinessControllerClass) && desktopModuleInfo.IsPortable)
                                        {
                                            var businessController = Reflection.CreateObject(
                                                desktopModuleInfo.BusinessControllerClass,
                                                desktopModuleInfo.BusinessControllerClass);
                                            var controller = businessController as IPortable;
                                            controller?.ImportModule(objModule.ModuleID, File.ReadAllText(portableModulesPath + "/" + oldMid + ".json", Encoding.Unicode), desktopModuleInfo.Version, userInfo.UserID);
                                        }
                                    }
                                }
                            }
                            else
                                item.InnerHtml = "";
                        }
                    }
                    Markup = html.DocumentNode.OuterHtml;
                }
                LayoutData["gjs-html"] = Markup;
                if (LayoutData.ContainsKey("gjs-components"))
                    LayoutData["gjs-components"] = MarkupJson;
            }

            internal static List<Pages> GetAllByState(int State)
            {
                return PageFactory.GetAllByState(State);
            }

            public static string GetCultureCode(PortalSettings PortalSettings)
            {
                return PortalSettings.DefaultLanguage != PortalSettings.CultureCode ? PortalSettings.CultureCode : null;
            }

            public static void ModeratePage(string Action, Pages Page, PortalSettings PortalSettings)
            {

                UserInfo UserInfo = PortalSettings.UserInfo;
                WorkflowState wState = Page.StateID.HasValue ? WorkflowManager.GetStateByID(Page.StateID.Value) : null;
                bool IsHaveReviewPermission = false;

                if (wState != null)
                {
                    IsHaveReviewPermission = WorkflowManager.HasReviewPermission(wState.StateID, UserInfo);
                }

                if (string.IsNullOrEmpty(Action) && !IsHaveReviewPermission)
                {
                    Page.StateID = WorkflowManager.GetFirstStateID(WorkflowManager.GetDefaultWorkflow(Page.TabID)).StateID;
                    AddLocalPages(Page, PortalSettings);
                }
                else if (!string.IsNullOrEmpty(Action) && Page.StateID.HasValue)
                {
                    foreach (string Locale in GetCultureListItems())
                    {
                        string _Locale = PortalSettings.DefaultLanguage == Locale ? null : Locale;

                        Pages LocalPage = GetLatestVersion(Page.TabID, _Locale, false);
                        if (LocalPage != null)
                        {
                            if (_Locale == Page.Locale)
                            {
                                LocalPage = Page;
                            }

                            if (Action == "approve" || Action == "publish")
                            {
                                int StateID = WorkflowManager.GetNextStateID(wState.WorkflowID, wState.StateID);
                                if (WorkflowManager.IsLastState(wState.WorkflowID, StateID))
                                {
                                    LocalPage.IsPublished = true;
                                    LocalPage.PublishedBy = UserInfo.UserID;
                                    LocalPage.PublishedOn = DateTime.UtcNow;
                                }
                                else
                                {
                                    LocalPage.IsPublished = false;
                                    LocalPage.PublishedBy = null;
                                    LocalPage.PublishedOn = null;
                                }
                                LocalPage.StateID = StateID;

                            }
                            else if (Action == "reject")
                            {
                                LocalPage.StateID = WorkflowManager.GetPreviousStateID(wState.WorkflowID, wState.StateID);
                            }

                            PageFactory.Update(LocalPage, UserInfo.UserID);
                        }
                    }

                }

                if (string.IsNullOrEmpty(Action) && IsHaveReviewPermission && !WorkflowManager.IsFirstState(wState.WorkflowID, wState.StateID))
                {
                    string SystemLog = "System Log: Changes made by user";
                    WorkflowLog log = WorkflowManager.GetEntityWorkflowLogs(WorkflowType.Page.ToString(), Page.TabID, Page.Version).LastOrDefault();
                    if (log == null || (log != null && (!log.Comment.Contains(SystemLog) || log.ReviewedBy != UserInfo.UserID)))
                    {
                        WorkflowFactory.AddWorkflowLog(PortalSettings.PortalId, 0, UserInfo.UserID, WorkflowType.Page.ToString(), Page.TabID, Page.StateID.Value, Page.Version, "approve", "System Log: Changes made by user");

                    }
                }

                PageFactory.Update(Page, UserInfo.UserID);
            }

            private static void AddLocalPages(Pages Page, PortalSettings PortalSettings)
            {
                int UserID = PortalSettings.UserInfo.UserID;
                string DefaultLanguage = PortalSettings.DefaultLanguage;

                foreach (string local in GetCultureListItems())
                {
                    string _TempLocale = local == DefaultLanguage ? null : local;
                    Pages LocalPage = GetLatestVersion(Page.TabID, _TempLocale, false);
                    if (Page.Version > 1)
                    {
                        if (LocalPage != null && Page.Locale != _TempLocale)
                        {
                            if (Page.Version > LocalPage.Version)
                            {
                                LocalPage.ID = 0;
                            }

                            LocalPage.Version = Page.Version;
                            LocalPage.StateID = Page.StateID;
                            LocalPage.IsPublished = false;
                            LocalPage.PublishedBy = null;
                            LocalPage.PublishedOn = null;
                        }
                        else
                        {
                            LocalPage = Page;
                        }
                        PageFactory.Update(LocalPage, UserID);
                    }
                    else
                    {
                        if (_TempLocale == Page.Locale)
                        {
                            PageFactory.Update(Page, PortalSettings.UserInfo.UserID);
                        }
                        else if (LocalPage != null)
                        {
                            LocalPage.Version = Page.Version;
                            LocalPage.StateID = Page.StateID;
                            LocalPage.IsPublished = false;
                            LocalPage.PublishedBy = null;
                            LocalPage.PublishedOn = null;
                            PageFactory.Update(LocalPage, PortalSettings.UserInfo.UserID);
                        }
                    }
                }

            }

            public static void UpdatePage(Pages page, int UserID)
            {
                PageFactory.Update(page, UserID);
            }

            public static List<Pages> GetAllPublishedPages(int PortalID, string Locale)
            {
                return PageFactory.GetAllPublishedPages(PortalID, Locale);
            }

            public static List<string> GetCultureListItems()
            {

                List<string> Languages = new List<string>();
                try
                {
                    IEnumerable<System.Web.UI.WebControls.ListItem> cultureListItems = DotNetNuke.Services.Localization.Localization.LoadCultureInListItems(CultureDropDownTypes.NativeName, CultureInfo.CurrentCulture.ToString(), "", false);
                    PortalSettings ps = PortalController.Instance.GetCurrentSettings() as PortalSettings;
                    foreach (Locale loc in LocaleController.Instance.GetLocales(ps.PortalId).Values)
                    {
                        string defaultRoles = PortalController.GetPortalSetting(string.Format("DefaultTranslatorRoles-{0}", loc.Code), ps.PortalId, "Administrators");
                        if (!ps.ContentLocalizationEnabled || (LocaleIsAvailable(loc) && (PortalSecurity.IsInRoles(ps.AdministratorRoleName) || loc.IsPublished || PortalSecurity.IsInRoles(defaultRoles))))
                        {
                            foreach (System.Web.UI.WebControls.ListItem cultureItem in cultureListItems)
                            {
                                if (cultureItem.Value == loc.Code)
                                {
                                    Languages.Add(loc.Code);
                                }
                            }
                        }
                    }
                }
                catch
                {
                }

                return Languages;
            }
            private static bool LocaleIsAvailable(Locale Locale)
            {
                TabInfo tab = (PortalController.Instance.GetCurrentSettings() as PortalSettings).ActiveTab;
                if (tab.DefaultLanguageTab != null)
                {
                    tab = tab.DefaultLanguageTab;
                }

                TabInfo localizedTab = TabController.Instance.GetTabByCulture(tab.TabID, tab.PortalID, Locale);

                return localizedTab != null && !localizedTab.IsDeleted && TabPermissionController.CanViewPage(localizedTab);
            }

            private static void BuildGlobalBlockMarkup(int PortalId, int UserId, HtmlNode item)
            {
                string blockguid = item.Attributes.Where(a => a.Name == "data-guid").FirstOrDefault().Value;
                if (!string.IsNullOrEmpty(blockguid))
                {
                    GlobalBlock customBlock = BlockManager.GetGlobalByLocale(PortalId, blockguid, GetCultureCode(PortalController.Instance.GetCurrentSettings() as PortalSettings));
                    if (customBlock == null)
                    {
                        customBlock = new GlobalBlock
                        {
                            Guid = blockguid.ToLower(),
                            PortalID = PortalId,
                            Name = item.Attributes.Where(a => a.Name == "data-name").FirstOrDefault().Value,
                            Category = item.Attributes.Where(a => a.Name == "data-category").FirstOrDefault().Value,
                            CreatedBy = UserId,
                            UpdatedBy = UserId,
                            CreatedOn = DateTime.UtcNow,
                            UpdatedOn = DateTime.UtcNow
                        };
                        string[] str = item.InnerHtml.Split(new string[] { "</style>" }, StringSplitOptions.None);
                        if (str.Length > 1)
                        {
                            customBlock.Html = str[1];
                            customBlock.Css = str[0];
                        }
                        else
                        {
                            customBlock.Html = str[0];
                        }

                        GlobalBlockFactory.AddUpdate(customBlock);
                    }
                    item.InnerHtml = item.Attributes.Where(a => a.Name == "data-block-type").FirstOrDefault().Value;
                    item.Attributes.Remove("data-category");
                    item.Attributes.Remove("data-name");
                }
            }

            public static IEnumerable<TabInfo> GetPageList(PortalSettings settings, int parentId = -1, string searchKey = "", bool includeHidden = true, bool includeDeleted = false, bool includeSubpages = false)
            {
                PortalSettings portalSettings = settings ?? PortalController.Instance.GetCurrentSettings() as PortalSettings;
                int adminTabId = portalSettings.AdminTabId;

                List<TabInfo> tabs = TabController.GetPortalTabs(portalSettings.PortalId, adminTabId, false, includeHidden, includeDeleted, true);
                IEnumerable<TabInfo> pages = from t in tabs
                                             where (t.ParentId != adminTabId || t.ParentId == Null.NullInteger) &&
                                                     !t.IsSystem &&
                                                         ((string.IsNullOrEmpty(searchKey) && (includeSubpages || t.ParentId == parentId))
                                                             || (!string.IsNullOrEmpty(searchKey) &&
                                                                     (t.TabName.IndexOf(searchKey, StringComparison.OrdinalIgnoreCase) > Null.NullInteger
                                                                         || t.LocalizedTabName.IndexOf(searchKey, StringComparison.OrdinalIgnoreCase) > Null.NullInteger)))
                                             select t;

                return includeSubpages ? pages.OrderBy(x => x.ParentId > -1 ? x.ParentId : x.TabID).ThenBy(x => x.TabID) : pages;
            }

            public static IEnumerable<TabInfo> GetPageList(PortalSettings portalSettings, bool? deleted, string tabName, string tabTitle, string tabPath,
                string tabSkin, bool? visible, int parentId, out int total, string searchKey = "", int pageIndex = -1, int pageSize = 10, bool includeSubpages = false)
            {
                pageIndex = pageIndex <= 0 ? 0 : pageIndex;
                pageSize = pageSize > 0 && pageSize <= 100 ? pageSize : 10;
                IEnumerable<TabInfo> tabs = GetPageList(portalSettings, parentId, searchKey, true, deleted ?? false, includeSubpages);
                List<TabInfo> finalList = new List<TabInfo>();
                if (deleted.HasValue)
                {
                    tabs = tabs.Where(tab => tab.IsDeleted == deleted);
                }

                if (visible.HasValue)
                {
                    tabs = tabs.Where(tab => tab.IsVisible == visible);
                }

                if (!string.IsNullOrEmpty(tabTitle) || !string.IsNullOrEmpty(tabName) || !string.IsNullOrEmpty(tabPath) ||
                    !string.IsNullOrEmpty(tabSkin))
                {
                    foreach (TabInfo tab in tabs)
                    {
                        bool bIsMatch = true;
                        if (!string.IsNullOrEmpty(tabTitle))
                        {
                            bIsMatch &= Regex.IsMatch(tab.Title, tabTitle.Replace("*", ".*"), RegexOptions.IgnoreCase);
                        }

                        if (!string.IsNullOrEmpty(tabName))
                        {
                            bIsMatch &= Regex.IsMatch(tab.TabName, tabName.Replace("*", ".*"), RegexOptions.IgnoreCase);
                        }

                        if (!string.IsNullOrEmpty(tabPath))
                        {
                            bIsMatch &= Regex.IsMatch(tab.TabPath, tabPath.Replace("*", ".*"), RegexOptions.IgnoreCase);
                        }

                        if (!string.IsNullOrEmpty(tabSkin))
                        {
                            string escapedString = Regex.Replace(tabSkin, "([^\\w^\\*\\s]+)+", @"\$1", RegexOptions.Compiled | RegexOptions.ECMAScript | RegexOptions.IgnoreCase | RegexOptions.Multiline);
                            bIsMatch &= Regex.IsMatch(tab.SkinSrc, escapedString.Replace("*", ".*"), RegexOptions.IgnoreCase);
                        }

                        if (bIsMatch)
                        {
                            finalList.Add(tab);
                        }
                    }
                }
                else
                {
                    finalList.AddRange(tabs);
                }
                total = finalList.Count;
                return finalList.Skip(pageIndex * pageSize).Take(pageSize);
            }

            public static ReviewSettings GetPageReviewSettings(PortalSettings PortalSettings)
            {
                int TabID = PortalSettings.ActiveTab.TabID;
                UserInfo UserInfo = PortalSettings.UserInfo;
                Pages page = GetLatestVersion(TabID, PortalSettings.UserInfo);

                bool IsPageDraft = false;
                bool IsContentApproval = false;
                bool IsLocked = false;
                string NextStateName = string.Empty;
                bool IsModeratorEditPermission = false;
                int CurruntWorkflowID = WorkflowManager.GetDefaultWorkflow(TabID);
                if (page != null && page.StateID.HasValue)
                {
                    WorkflowState State = WorkflowManager.GetStateByID(page.StateID.Value);
                    if (State != null)
                    {

                        bool IsFirstState = WorkflowManager.IsFirstState(State.WorkflowID, State.StateID);
                        bool IsLastState = WorkflowManager.IsLastState(State.WorkflowID, State.StateID);
                        IsPageDraft = WorkflowManager.IsFirstState(State.WorkflowID, State.StateID);
                        IsLocked = !IsFirstState && !IsLastState;

                        if (IsLastState && State.WorkflowID != CurruntWorkflowID)
                        {
                            State = WorkflowManager.GetFirstStateID(CurruntWorkflowID);
                        }

                        if (WorkflowManager.GetWorkflowType(State.WorkflowID) == Core.Components.Enum.WorkflowTypes.ContentApproval)
                        {
                            IsContentApproval = true;


                            if (WorkflowManager.HasReviewPermission(State.StateID, UserInfo) && TabPermissionController.CanManagePage(PortalSettings.ActiveTab))
                            {
                                WorkflowState NextState = Core.Managers.WorkflowManager.GetStateByID(Core.Managers.WorkflowManager.GetNextStateID(State.WorkflowID, State.StateID));
                                NextStateName = NextState != null ? NextState.Name : string.Empty;
                                IsModeratorEditPermission = true;
                            }
                            else
                            {
                                WorkflowState NextState = Core.Managers.WorkflowManager.GetStateByID(Core.Managers.WorkflowManager.GetNextStateID(State.WorkflowID, Core.Managers.WorkflowManager.GetFirstStateID(State.WorkflowID).StateID));
                                NextStateName = NextState != null ? NextState.Name : string.Empty;
                            }
                        }

                    }
                }
                else
                {
                    if (CurruntWorkflowID > 0)
                    {
                        IsContentApproval = Core.Managers.WorkflowManager.GetWorkflowType(CurruntWorkflowID) == Core.Components.Enum.WorkflowTypes.ContentApproval;
                        WorkflowState NextState = Core.Managers.WorkflowManager.GetStateByID(Core.Managers.WorkflowManager.GetNextStateID(CurruntWorkflowID, Core.Managers.WorkflowManager.GetFirstStateID(CurruntWorkflowID).StateID));
                        NextStateName = NextState != null ? NextState.Name : string.Empty;
                    }
                }

                ReviewSettings ReviewSetting = new ReviewSettings
                {
                    IsPageDraft = IsPageDraft,
                    IsContentApproval = IsContentApproval,
                    NextStateName = NextStateName,
                    IsLocked = IsLocked,
                    IsModeratorEditPermission = IsModeratorEditPermission
                };

                return ReviewSetting;
            }
            public static List<int> GetAllTabIdByPortalID(int Portalid, bool OnlyPublished = true)
            {
                return PageFactory.GetAllTabIdByPortalID(Portalid, OnlyPublished);
            }
            public static string TokenizeTemplateLinks(string content, bool IsJson, Dictionary<string, string> Assets)
            {
                if (IsJson)
                {
                    dynamic deserializeObject = JsonConvert.DeserializeObject(content);
                    if (deserializeObject != null)
                    {
                        foreach (dynamic arr in deserializeObject)
                        {
                            ProcessJsonObject(arr, Assets);
                        }
                        content = JsonConvert.SerializeObject(deserializeObject);
                    }
                }
                else
                {
                    HtmlDocument html = new HtmlDocument();
                    html.LoadHtml(content);
                    HtmlNodeCollection NodeCollectionSrc = html.DocumentNode.SelectNodes("//*[@src]");
                    if (NodeCollectionSrc != null)
                    {
                        foreach (HtmlNode node in NodeCollectionSrc)
                        {
                            node.Attributes["src"].Value = GetNewLink(node.Attributes["src"].Value, Assets);
                        }
                    }
                    HtmlNodeCollection NodeCollectionSrcSet = html.DocumentNode.SelectNodes("//*[@srcset]");
                    if (NodeCollectionSrcSet != null)
                    {
                        foreach (HtmlNode node in NodeCollectionSrcSet)
                        {
                            node.Attributes["srcset"].Value = GetNewLink(node.Attributes["srcset"].Value, Assets);
                        }
                    }
                    HtmlNodeCollection NodeCollectionThumb = html.DocumentNode.SelectNodes("//*[@thumbnail]");
                    if (NodeCollectionThumb != null)
                    {
                        foreach (HtmlNode node in NodeCollectionThumb)
                        {
                            node.Attributes["thumbnail"].Value = GetNewLink(node.Attributes["thumbnail"].Value, Assets);
                        }
                    }
                    content = html.DocumentNode.OuterHtml;
                }
                foreach (Match match in Regex.Matches(content, "url\\(([^)]*)\\)"))
                {
                    try
                    {
                        string matchurl = match.Value.Replace("url(\"", "").Replace("\")", "").Replace("url(\\\"", "").Replace("\\\")", "");
                        string newlink = GetNewLink(matchurl, Assets);
                        if (matchurl != newlink)
                        {
                            if (matchurl.EndsWith("\\") || matchurl.EndsWith(@"\"))
                                content = content.Replace(matchurl, newlink + @"\");
                            else
                                content = content.Replace(matchurl, newlink);
                        }
                    }
                    catch { }
                }
                return content;
            }

            private static string GetNewLink(string url, Dictionary<string, string> Assets)
            {
                if (url.Contains(','))
                {
                    List<string> result = new List<string>();
                    foreach (var item in url.Split(','))
                    {
                        var obj = item.Split(' ');
                        result.Add(ExtractAndProcessLink(item, Assets) + (obj.Length > 1 ? (" " + item.Split(' ')[1]) : ""));
                    }
                    return string.Join(",", result);
                }
                else
                {
                    return ExtractAndProcessLink(url, Assets);
                }
            }

            private static string ExtractAndProcessLink(string url, Dictionary<string, string> Assets)
            {
                if (url.StartsWith("http"))
                    return url;
                url = url.Split('?')[0];
                string newurl = ExportTemplateRootToken + (url.ToLower().Contains(".versions") ? ".versions/" : "") + System.IO.Path.GetFileName(url);
                if (!Assets.ContainsKey(newurl))
                {
                    Assets.Add(newurl, url);
                }
                else if (Assets.ContainsKey(newurl) && Assets[newurl] != url)
                {
                    string FileExtension = newurl.Substring(newurl.LastIndexOf('.'));
                    string tempNewUrl = newurl;
                    int count = 1;
                    Find:
                    if (Assets.ContainsKey(tempNewUrl) && Assets[tempNewUrl] != url)
                    {
                        tempNewUrl = newurl.Remove(newurl.Length - FileExtension.Length) + count + FileExtension;
                        count++;
                        goto Find;
                    }
                    else
                    {
                        newurl = tempNewUrl;
                        if (!Assets.ContainsKey(newurl))
                        {
                            Assets.Add(newurl, url);
                        }
                    }
                }
                return newurl;
            }

            private static void ProcessJsonObject(dynamic arr, Dictionary<string, string> Assets)
            {
                foreach (JProperty prop in arr.Properties())
                {
                    if ((prop.Name == "src" || prop.Name == "srcset" || prop.Name == "thumbnail") && !string.IsNullOrEmpty(prop.Value.ToString()))
                    {
                        prop.Value = GetNewLink(prop.Value.ToString(), Assets);
                    }
                }
                if (arr.attributes != null)
                {
                    foreach (dynamic prop in arr.attributes)
                    {
                        if ((prop.Name == "src" || prop.Name == "srcset" || prop.Name == "thumbnail") && !string.IsNullOrEmpty(prop.Value.ToString()))
                        {
                            prop.Value = GetNewLink(prop.Value.ToString(), Assets);
                        }
                    }
                }
                if (arr.components != null)
                {
                    foreach (dynamic obj in arr.components)
                    {
                        ProcessJsonObject(obj, Assets);
                    }
                }
            }

            public static string TokenizeLinks(string content, int portalId)
            {
                string portalRoot = GetPortalRoot(portalId);
                Regex exp = new Regex(portalRoot, RegexOptions.IgnoreCase);
                content = exp.Replace(content, PortalRootToken);
                return content;
            }
            public static string DeTokenizeLinks(string content, int portalId)
            {
                string portalRoot = GetPortalRoot(portalId);
                content = content.Replace(PortalRootToken, portalRoot);
                content = content.Replace(ExportTemplateRootToken, portalRoot.EndsWith("/") ? portalRoot + "Images/" : portalRoot + "/Images/");
                return content;
            }

            public static string GetPortalRoot(int portalId)
            {

                PortalInfo portal = PortalController.Instance.GetPortal(portalId);

                string portalRoot = UrlUtils.Combine(DotNetNuke.Common.Globals.ApplicationPath, portal.HomeDirectory);

                if (!portalRoot.StartsWith("/"))
                {
                    portalRoot = "/" + portalRoot;
                }

                return portalRoot;
            }

            public static void AddComment(PortalSettings PortalSettings, string Action, string Comment, Pages Page)
            {
                if ((Page.ID == 0 || !Page.IsPublished || WorkflowManager.HasReviewPermission(Page.StateID.Value, PortalSettings.UserInfo)) && string.IsNullOrEmpty(Comment))
                {
                    ModeratePage(Page.IsPublished ? Action : string.Empty, Page, PortalSettings);
                }
                else if (Page.IsPublished)
                {
                    AddComment(PortalSettings, Action, Comment);
                }
            }

            public static void AddComment(PortalSettings PortalSettings, string Action, string Comment)
            {
                Pages Page = GetLatestVersion(PortalSettings.ActiveTab.TabID, PortalSettings.UserInfo);
                if (Page != null)
                {
                    UserInfo userinfo = PortalSettings.UserInfo;
                    int PortalID = PortalSettings.PortalId;
                    bool wfper = WorkflowManager.HasReviewPermission(Page.StateID.Value, userinfo);
                    WorkflowState State = WorkflowManager.GetStateByID(Page.StateID.Value);

                    if (wfper || (TabPermissionController.CanManagePage(PortalSettings.ActiveTab) && WorkflowManager.IsFirstState(State.WorkflowID, Page.StateID.Value)))
                    {
                        ModeratePage(Action, Page, PortalSettings);
                        WorkflowManager.SendWorkflowNotification(PortalID, Page, Comment, Action);
                        WorkflowFactory.AddWorkflowLog(PortalID, 0, userinfo.UserID, WorkflowType.Page.ToString(), Page.TabID, Page.StateID.Value, Page.Version, Action, Comment);
                    }
                }

            }

            public void AddComment(string Entity, int EntityID, string Action, string Comment, PortalSettings PortalSettings)
            {
                if (Entity == WorkflowType.Page.ToString())
                    AddComment(PortalSettings, Action, Comment);
            }

            public static void ProcessPortableModules(int PortalID, string Content, Dictionary<int, string> ExportedModulesContent)
            {
                if (!string.IsNullOrEmpty(Content))
                {
                    HtmlDocument html = new HtmlDocument();
                    html.LoadHtml(Content);
                    IEnumerable<HtmlNode> query = html.DocumentNode.Descendants("div");
                    foreach (HtmlNode item in query.ToList())
                    {
                        if (item.Attributes.Where(a => a.Name == "dmid").FirstOrDefault() != null && item.Attributes.Where(a => a.Name == "mid").FirstOrDefault() != null)
                        {
                            int mid = int.Parse(item.Attributes.Where(a => a.Name == "mid").FirstOrDefault().Value);
                            ModuleInfo module = ModuleController.Instance.GetModule(mid, Null.NullInteger, false);
                            if (module != null)
                            {
                                var moduleDef = ModuleDefinitionController.GetModuleDefinitionByID(module.ModuleDefID);
                                var desktopModuleInfo = DesktopModuleController.GetDesktopModule(moduleDef.DesktopModuleID, PortalID);
                                if (!string.IsNullOrEmpty(desktopModuleInfo?.BusinessControllerClass))
                                {
                                    if (!module.IsDeleted && !string.IsNullOrEmpty(module.DesktopModule.BusinessControllerClass) && module.DesktopModule.IsPortable)
                                    {
                                        var businessController = Reflection.CreateObject(
                                            module.DesktopModule.BusinessControllerClass,
                                            module.DesktopModule.BusinessControllerClass);
                                        var controller = businessController as IPortable;
                                        var content = controller?.ExportModule(module.ModuleID);
                                        if (!string.IsNullOrEmpty(content))
                                            ExportedModulesContent.Add(module.ModuleID, content);
                                    }
                                }
                            }
                        }
                    }
                }
            }

            public static void ApplyGlobalBlockJSON(Pages page)
            {
                if (page != null)
                {
                    string CacheKey = CacheFactory.GetCacheKey(CacheFactory.Keys.Page + "ApplyGlobalBlockJSON", page.ID);
                    string IsCached = CacheFactory.Get(CacheKey);
                    if (string.IsNullOrEmpty(IsCached) && page.ContentJSON != null)
                    {
                        var contentJSON = JsonConvert.DeserializeObject(page.ContentJSON);
                        if (contentJSON != null)
                        {
                            try
                            {
                                if (string.IsNullOrEmpty(page.StyleJSON))
                                    page.StyleJSON = "";
                                var styleJSON = JsonConvert.DeserializeObject(page.StyleJSON);
                                UpdateGlobalBlockJSON(contentJSON, styleJSON);
                                page.ContentJSON = JsonConvert.SerializeObject(contentJSON);
                                if (styleJSON != null)
                                    page.StyleJSON = JsonConvert.SerializeObject(styleJSON);
                                CacheFactory.Set(CacheKey, "true");
                            }
                            catch (Exception ex) { ExceptionManager.LogException(ex); }
                        }
                    }
                }
            }

            private static void UpdateGlobalBlockJSON(dynamic contentJSON, dynamic styleJSON)
            {
                foreach (dynamic con in contentJSON)
                {
                    if (con.type != null && con.type.Value == "globalblockwrapper" && con.attributes != null && con.attributes["data-guid"] != null)
                    {
                        GlobalBlock block = BlockManager.GetGlobalByGuid(PortalSettings.Current.PortalId, con.attributes["data-guid"].Value);

                        if (block != null && !string.IsNullOrEmpty(block.ContentJSON))
                        {
                            con.components = JsonConvert.DeserializeObject(block.ContentJSON);
                            string prefix = con.attributes["id"] != null ? con.attributes["id"].Value : string.Empty;
                            List<string> Ids = new List<string>();
                            UpdateIds(con.components, prefix, Ids);
                            if (!string.IsNullOrEmpty(block.StyleJSON) && styleJSON != null)
                            {
                                dynamic styles = JsonConvert.DeserializeObject(block.StyleJSON);
                                foreach (var style in styles)
                                {
                                    if (!string.IsNullOrEmpty(prefix) && style.selectors != null)
                                    {
                                        foreach (dynamic st in style.selectors)
                                        {
                                            if (st.name != null)
                                            {
                                                string val = st.name;
                                                if (!Ids.Contains(val))
                                                    st.name.Value = prefix + "-" + val.Split('-').Last();
                                            }
                                        }
                                    }
                                    styleJSON.Add(style);
                                }
                            }
                        }

                    }
                    else if (con.components != null)
                    {
                        UpdateGlobalBlockJSON(con.components, styleJSON);
                    }
                }
            }

            private static void UpdateIds(dynamic contentJSON, string prefix, List<string> Ids)
            {
                if (!string.IsNullOrEmpty(prefix) && contentJSON != null)
                {
                    if (contentJSON is JArray)
                    {
                        foreach (dynamic con in contentJSON)
                        {
                            if (con.attributes != null && con.attributes["id"] != null && con.type != null && con.type.Value != "module" && con.type.Value != "blockwrapper")
                            {
                                string val = con.attributes["id"].Value;
                                con.attributes["id"].Value = prefix + "-" + val.Split('-').Last();
                            }
                            else if (con.attributes != null && con.attributes["id"] != null)
                                Ids.Add(con.attributes["id"].Value);
                            if (con.components != null)
                                UpdateIds(con.components, prefix, Ids);
                        }
                    }
                    else
                    {
                        if (contentJSON.attributes != null && contentJSON.attributes["id"] != null && contentJSON.type != null && contentJSON.type.Value != "module" && contentJSON.type.Value != "blockwrapper")
                        {
                            string val = contentJSON.attributes["id"].Value;
                            contentJSON.attributes["id"].Value = prefix + "-" + val.Split('-').Last();
                        }
                        else
                            Ids.Add(contentJSON.attributes["id"].Value);
                        if (contentJSON.components != null)
                            UpdateIds(contentJSON.components, prefix, Ids);
                    }
                }
            }
        }
    }
}