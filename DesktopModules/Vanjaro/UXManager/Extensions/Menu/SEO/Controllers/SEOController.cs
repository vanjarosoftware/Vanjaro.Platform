using Dnn.PersonaBar.Seo.Components;
using Dnn.PersonaBar.Seo.Services.Dto;
using Dnn.PersonaBar.SiteSettings.Components;
using Dnn.PersonaBar.SiteSettings.Services.Dto;
using DotNetNuke.Common;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Tabs;
using DotNetNuke.Entities.Urls;
using DotNetNuke.Entities.Users;
using DotNetNuke.Services.Localization;
using DotNetNuke.Services.Search.Entities;
using DotNetNuke.Services.Search.Internals;
using DotNetNuke.Services.Url.FriendlyUrl;
using DotNetNuke.Web.Api;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Web.Http;
using Vanjaro.Common.ASPNET.WebAPI;
using Vanjaro.Common.Engines.UIEngine;
using Vanjaro.UXManager.Extensions.Menu.SEO.Factories;
using Vanjaro.UXManager.Library.Common;
using Vanjaro.UXManager.Library.Entities;

namespace Vanjaro.UXManager.Extensions.Menu.SEO.Controllers
{
    [ValidateAntiForgeryToken]
    [AuthorizeAccessRoles(AccessRoles = "admin")]
    public class SEOController : UIEngineController
    {
        private static readonly SiteSettingsController _controller = new SiteSettingsController();
        public static List<IUIData> GetData(PortalSettings portalSettings, UserInfo userInfo)
        {
            Dictionary<string, IUIData> Settings = new Dictionary<string, IUIData>();
            IList<string> AvailableAnalyzers = _controller.GetAvailableAnalyzers();
            dynamic SitemapSettings = Managers.SEOManager.GetSitemapSettings().Data;
            dynamic UrlRedirectSettings = Managers.SEOManager.GetGeneralSettings().Data;
            dynamic BasicSearchSettings = Managers.SearchManager.GetBasicSearchSettings(AvailableAnalyzers).Data;
            Settings.Add("SiteTitle", new UIData { Name = "SiteTitle", Value = portalSettings.PortalName });
            Settings.Add("HTMLPageHeader", new UIData { Name = "HTMLPageHeader", Value = portalSettings.PageHeadText });
            Settings.Add("Description", new UIData { Name = "Description", Value = portalSettings.Description });
            Settings.Add("Keywords", new UIData { Name = "Keywords", Value = portalSettings.KeyWords });
            bool DeletedPageHandling = UrlRedirectSettings.DeletedTabHandlingType == "Do301RedirectToPortalHome" ? true : false;
            Settings.Add("DeletedPageHandling", new UIData { Name = "DeletedPageHandling", Value = DeletedPageHandling.ToString() });
            Settings.Add("UrlRedirectSettings", new UIData { Name = "UrlRedirectSettings", Options = UrlRedirectSettings });
            Settings.Add("RegexSettings", new UIData { Name = "RegexSettings", Options = Managers.SEOManager.GetRegexSettings().Data });
            Settings.Add("SitemapSettings", new UIData { Name = "SitemapSettings", Options = SitemapSettings });
            Settings.Add("ResultingURLs", new UIData { Name = "ResultingURLs", Value = "" });
            Settings.Add("AddQueryString", new UIData { Name = "AddQueryString", Value = "" });
            Settings.Add("CustomPageName", new UIData { Name = "CustomPageName", Value = "" });
            Settings.Add("URLToTest", new UIData { Name = "URLToTest", Value = "" });
            Settings.Add("UrlRewritingResult", new UIData { Name = "UrlRewritingResult", Options = new UrlRewritingResult() });
            Settings.Add("PageToTest", new UIData { Name = "PageToTest", Options = Library.Managers.PageManager.GetParentPages(portalSettings).Select(a => new { a.TabID, a.TabName }), OptionsText = "TabName", OptionsValue = "TabID", Value = "-1" });
            Settings.Add("SitemapExcludePriority", new UIData { Name = "SitemapExcludePriority", Options = "", OptionsText = "label", OptionsValue = "value", Value = Convert.ToString(SitemapSettings.SitemapExcludePriority) });
            Settings.Add("SitemapMinPriority", new UIData { Name = "SitemapMinPriority", Options = "", OptionsText = "label", OptionsValue = "value", Value = Convert.ToString(SitemapSettings.SitemapMinPriority) });
            Settings.Add("SitemapCacheDays", new UIData { Name = "SitemapCacheDays", Options = Managers.SEOManager.BindSitemapCacheDays().Select(a => new { a.Key, a.Value }), OptionsText = "Value", OptionsValue = "Key", Value = Convert.ToString(SitemapSettings.SitemapCacheDays) });
            Settings.Add("BasicSearch", new UIData { Name = "BasicSearch", Value = "", Options = new UpdateBasicSearchSettingsRequest { } });
            Settings.Add("BasicSettings", new UIData { Name = "BasicSettings", Value = "", Options = BasicSearchSettings });
            Settings.Add("AllSynonymsGroups", new UIData { Name = "AllSynonymsGroups", Value = "", Options = SearchHelper.Instance.GetSynonymsGroups(portalSettings.PortalId, LocaleController.Instance.GetCurrentLocale(portalSettings.PortalId).Code).OrderBy(o => o.SynonymsGroupId) });
            Settings.Add("NewTagGroup", new UIData { Name = "NewTagGroup", Value = "", Options = new UpdateSynonymsGroupRequest { } });
            Settings.Add("IgnoreWord", new UIData { Name = "IgnoreWord", Value = "", Options = Managers.SearchManager.GetIgnoreWords(portalSettings.PortalId, portalSettings.CultureCode).Data });
            Settings.Add("IgnoreWords", new UIData { Name = "IgnoreWords", Value = "", Options = new UpdateIgnoreWordsRequest { } });

            List<SelectListItem> CustomAnalyzer = new List<SelectListItem>();
            foreach (string d in AvailableAnalyzers)
            {
                CustomAnalyzer.Add(new SelectListItem { Text = d, Selected = false, Value = d });
            }

            Settings.Add("Working_SynonymsGroup", new UIData { Name = "Working_SynonymsGroup", Options = new SynonymsGroup() });
            CustomAnalyzer.Add(new SelectListItem { Text = Localization.GetString("NoneSpecified", Components.Constants.ResourcesFile), Selected = false, Value = "None" });
            Settings.Add("CustomAnalyzer", new UIData { Name = "CustomAnalyzer", Options = CustomAnalyzer, OptionsText = "Text", OptionsValue = "Value", Value = string.IsNullOrEmpty(BasicSearchSettings.SearchCustomAnalyzer) ? "None" : BasicSearchSettings.SearchCustomAnalyzer.ToString() });
            return Settings.Values.ToList();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AuthorizeAccessRoles(AccessRoles = "admin")]
        public ActionResult UpdateSettings(dynamic requestSettings)
        {
            ActionResult actionResult = new ActionResult();
            try
            {
                PortalInfo portalInfo = PortalController.Instance.GetPortal(PortalSettings.PortalId);
                portalInfo.PortalName = requestSettings.SiteTitle.Value;
                portalInfo.Description = requestSettings.Description.Value;
                portalInfo.KeyWords = requestSettings.Keywords.Value;
                PortalController.UpdatePortalSetting(PortalSettings.PortalId, "PageHeadText", string.IsNullOrEmpty(requestSettings.HTMLPageHeader.Value) ? "false" : requestSettings.HTMLPageHeader.Value);
                PortalController.Instance.UpdatePortalInfo(portalInfo);
                UpdateGeneralSettingsRequest updateGeneralSettingsRequest = JsonConvert.DeserializeObject<UpdateGeneralSettingsRequest>(requestSettings.UpdateGeneralSettingsRequest.ToString());
                UpdateRegexSettingsRequest updateRegexSettingsRequest = JsonConvert.DeserializeObject<UpdateRegexSettingsRequest>(requestSettings.UpdateRegexSettingsRequest.ToString());
                SitemapSettingsRequest sitemapSettingsRequest = JsonConvert.DeserializeObject<SitemapSettingsRequest>(requestSettings.SitemapSettingsRequest.ToString());
                actionResult = Managers.SEOManager.UpdateGeneralSettings(updateGeneralSettingsRequest);
                if (actionResult.HasErrors)
                {
                    return actionResult;
                }

                actionResult = Managers.SEOManager.UpdateRegexSettings(updateRegexSettingsRequest);
                if (actionResult.HasErrors)
                {
                    return actionResult;
                }

                actionResult = Managers.SEOManager.UpdateSitemapSettings(sitemapSettingsRequest);
                if (actionResult.HasErrors)
                {
                    return actionResult;
                }

                actionResult = UpdateSearch(requestSettings.UpdateSearchData);
            }
            catch (Exception exc)
            {
                actionResult.AddError(HttpStatusCode.InternalServerError.ToString(), exc.Message);
            }
            return actionResult;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ResetCache()
        {
            ActionResult actionResult = new ActionResult();
            try
            {
                Managers.SEOManager.ResetCache();
            }
            catch (Exception exc)
            {
                actionResult.AddError(HttpStatusCode.InternalServerError.ToString(), exc.Message);
            }
            return actionResult;
        }

        /// <summary>
        /// Tests the internal URL
        /// </summary>
        /// <returns>Various forms of the URL and any messages when they exist</returns>
        /// <example>
        /// GET /API/PersonaBar/SEO/TestUrl?pageId=53&amp;queryString=ab%3Dcd&amp;customPageName=test-page
        /// </example>
        [HttpPost]
        public ActionResult TestUrl(int pageId, string queryString, string customPageName)
        {
            ActionResult actionResult = new ActionResult();
            try
            {
                var response = new
                {
                    Success = true,
                    Urls = TestUrlInternal(pageId, queryString, customPageName)
                };
                actionResult.Data = response;
            }
            catch (Exception exc)
            {
                actionResult.AddError(HttpStatusCode.InternalServerError.ToString(), exc.Message);
            }
            return actionResult;
        }

        private IEnumerable<string> TestUrlInternal(int pageId, string queryString, string customPageName)
        {
            DNNFriendlyUrlProvider provider = new DNNFriendlyUrlProvider();
            TabInfo tab = TabController.Instance.GetTab(pageId, PortalSettings.PortalId, false);
            string pageName = string.IsNullOrEmpty(customPageName) ? Globals.glbDefaultPage : customPageName;
            return PortalAliasController.Instance.GetPortalAliasesByPortalId(PortalSettings.PortalId).
                Select(alias => provider.FriendlyUrl(
                    tab, "~/Default.aspx?tabId=" + pageId + "&" + queryString, pageName, alias.HTTPAlias));
        }

        [HttpPost]
        public ActionResult TestUrlRewrite(string uri)
        {
            ActionResult actionResult = new ActionResult();
            try
            {
                if (!string.IsNullOrEmpty(uri))
                {
                    actionResult.Data = TestUrlRewritingInternal(uri);
                }
            }
            catch (Exception exc)
            {
                actionResult.AddError(HttpStatusCode.InternalServerError.ToString(), exc.Message);
            }
            return actionResult;
        }
        #region Site Search
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UpdateBasicSearchSettings(UpdateBasicSearchSettingsRequest request)
        {
            return Managers.SearchManager.UpdateBasicSearchSettings(request);
        }

        private ActionResult UpdateSearch(dynamic updatedata)
        {
            ActionResult actionResult = new ActionResult();
            Dictionary<string, dynamic> ReponseData = new Dictionary<string, dynamic>();
            try
            {
                UpdateBasicSearchSettingsRequest BasicSettings = updatedata["BasicSettings"].ToObject<UpdateBasicSearchSettingsRequest>();
                UpdateIgnoreWordsRequest IgnoreWord = updatedata["IgnoreWord"].ToObject<UpdateIgnoreWordsRequest>();
                List<SynonymsGroup> AllSynonymsGroups = updatedata["AllSynonymsGroups"].ToObject<List<SynonymsGroup>>();
                ActionResult ActionResult_IgnoreWord = new ActionResult();
                ActionResult ActionResult_BasicSettings = new ActionResult();
                ActionResult ActionResult_SynonymsGroups = new ActionResult();
                if (UserInfo.IsSuperUser)
                {
                    ActionResult_BasicSettings = UpdateBasicSearchSettings(BasicSettings);
                    if (!ActionResult_BasicSettings.IsSuccess)
                    {
                        return ActionResult_BasicSettings;
                    }
                    else
                    {
                        ReponseData.Add("ActionResult_BasicSettings", ActionResult_BasicSettings.Data);
                    }
                }

                ActionResult_IgnoreWord = Managers.SearchManager.AddIgnoreWords(IgnoreWord);
                if (!ActionResult_IgnoreWord.IsSuccess)
                {
                    return ActionResult_IgnoreWord;
                }
                else
                {
                    ReponseData.Add("ActionResult_IgnoreWord", ActionResult_IgnoreWord.Data);
                }

                ActionResult_SynonymsGroups = UpdateSynonymsGroup(AllSynonymsGroups);
                if (!ActionResult_SynonymsGroups.IsSuccess)
                {
                    return ActionResult_SynonymsGroups;
                }
                else
                {
                    ReponseData.Add("ActionResult_SynonymsGroups", ActionResult_SynonymsGroups.Data);
                }

                actionResult.Data = ReponseData;
            }
            catch (Exception exc)
            {
                actionResult.AddError("HttpStatusCode.InternalServerError", exc.Message);
            }
            return actionResult;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AuthorizeAccessRoles(AccessRoles = "admin")]
        public ActionResult PortalSearchReindex(int? portalId)
        {
            return Managers.SearchManager.PortalSearchReindex(portalId);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AuthorizeAccessRoles(AccessRoles = "host")]
        public ActionResult CompactSearchIndex()
        {
            ActionResult actionResult = new ActionResult();
            try
            {
                SearchHelper.Instance.SetSearchReindexRequestTime(true);
            }
            catch (Exception exc)
            {
                actionResult.AddError("HttpStatusCode.InternalServerError", exc.Message);
            }
            return actionResult;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AuthorizeAccessRoles(AccessRoles = "host")]
        public ActionResult HostSearchReindex()
        {
            ActionResult actionResult = new ActionResult();
            try
            {
                SearchHelper.Instance.SetSearchReindexRequestTime(-1);
            }
            catch (Exception exc)
            {
                actionResult.AddError("HttpStatusCode.InternalServerError", exc.Message);
            }
            return actionResult;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AuthorizeAccessRoles(AccessRoles = "admin")]
        public ActionResult UpdateSynonymsGroup(List<SynonymsGroup> SynonymsGroupRequest)
        {
            return Managers.SearchManager.UpdateSynonymsGroup(SynonymsGroupRequest);
        }
        #endregion
        private UrlRewritingResult TestUrlRewritingInternal(string uriString)
        {
            UrlRewritingResult rewritingResult = new UrlRewritingResult();
            try
            {
                string noneText = Localization.GetString("None", Localization.GlobalResourceFile);
                Uri uri = new Uri(uriString);
                AdvancedUrlRewriter provider = new AdvancedUrlRewriter();
                UrlAction result = new UrlAction(uri.Scheme, uriString, Globals.ApplicationMapPath)
                {
                    RawUrl = uriString
                };
                HttpContext httpContext = new HttpContext(HttpContext.Current.Request, new HttpResponse(new StringWriter()));
                provider.ProcessTestRequestWithContext(httpContext, uri, true, result, new FriendlyUrlSettings(PortalSettings.PortalId));
                rewritingResult.RewritingResult = string.IsNullOrEmpty(result.RewritePath) ? noneText : result.RewritePath;
                rewritingResult.Culture = string.IsNullOrEmpty(result.CultureCode) ? noneText : result.CultureCode;
                TabInfo tab = TabController.Instance.GetTab(result.TabId, result.PortalId, false);
                rewritingResult.IdentifiedPage = (tab != null ? tab.TabName : noneText);
                rewritingResult.RedirectionReason = Localization.GetString(result.Reason.ToString());
                rewritingResult.RedirectionResult = result.FinalUrl;
                StringBuilder messages = new StringBuilder();
                foreach (string message in result.DebugMessages)
                {
                    messages.AppendLine(message);
                }
                rewritingResult.OperationMessages = messages.ToString();
            }
            catch (Exception ex)
            {
                rewritingResult.OperationMessages = ex.Message;
            }
            return rewritingResult;
        }
        public override string AccessRoles()
        {
            return AppFactory.GetAccessRoles(UserInfo);
        }
    }
}