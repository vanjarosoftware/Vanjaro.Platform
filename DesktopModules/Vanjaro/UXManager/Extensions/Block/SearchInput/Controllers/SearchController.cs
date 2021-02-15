using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using DotNetNuke.Services.Localization;
using DotNetNuke.Services.Search.Entities;
using DotNetNuke.Services.Search.Internals;
using DotNetNuke.Web.Api;
using DotNetNuke.Web.Api.Internal;
using DotNetNuke.Web.InternalServices.Views.Search;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Http;
using Vanjaro.Common.ASPNET.WebAPI;
using Vanjaro.Common.Engines.UIEngine;
using Vanjaro.Core.Components;
using Vanjaro.UXManager.Library.Common;
using static Vanjaro.Core.Managers;
using static Vanjaro.UXManager.Extensions.Block.SearchInput.Managers;

namespace Vanjaro.UXManager.Extensions.Block.SearchInput.Controllers
{
    [ValidateAntiForgeryToken]
    [AuthorizeAccessRoles(AccessRoles = "admin,anonymous")]
    public class SearchController : UIEngineController
    {
        internal static List<IUIData> GetData(string identifier, Dictionary<string, string> parameters, UserInfo userInfo, PortalSettings portalSettings)
        {
            List<StringText> templates = GetTemplates();
            Dictionary<string, IUIData> Settings = new Dictionary<string, IUIData>
            {
                { "Global", new UIData { Name = "Global", Value = "true" } },                
                { "EnableWildSearch", new UIData { Name = "EnableWildSearch", Value = "true" } },
                { "GlobalConfigs", new UIData { Name = "GlobalConfigs", Options = Core.Managers.BlockManager.GetGlobalConfigs(portalSettings, "search result") } },
                { "IsAdmin", new UIData { Name = "IsAdmin", Value = userInfo.IsInRole("Administrators").ToString().ToLower() } },
                { "Template", new UIData { Name = "Template", Options = templates, OptionsText = "Text", OptionsValue = "Value" } }
            };
            return Settings.Values.ToList();
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult Preview(string keywords)
        {
            ActionResult actionResult = new ActionResult();
            string culture = PortalSettings.CultureCode; int portal = -1;
            keywords = (keywords ?? string.Empty).Trim();
            IList<string> tags = SearchQueryStringParser.Instance.GetTags(keywords, out string cleanedKeywords);
            DateTime beginModifiedTimeUtc = SearchQueryStringParser.Instance.GetLastModifiedDate(cleanedKeywords, out cleanedKeywords);
            IList<string> searchTypes = SearchQueryStringParser.Instance.GetSearchTypeList(keywords, out cleanedKeywords);

            IList<SearchContentSource> contentSources = SearchInputManager.GetSearchContentSources(searchTypes);
            System.Collections.Hashtable settings = SearchInputManager.GetSearchModuleSettings();
            List<int> searchTypeIds = SearchInputManager.GetSearchTypeIds(settings, contentSources);
            IEnumerable<int> moduleDefids = SearchInputManager.GetSearchModuleDefIds(settings, contentSources);
            List<int> portalIds = SearchInputManager.GetSearchPortalIds(settings, portal);

            int userSearchTypeId = SearchHelper.Instance.GetSearchTypeByName("user").SearchTypeId;
            SearchContentSource userSearchSource = contentSources.FirstOrDefault(s => s.SearchTypeId == userSearchTypeId);

            List<GroupedBasicView> results = new List<GroupedBasicView>();
            if (portalIds.Any() && searchTypeIds.Any() &&
                (!string.IsNullOrEmpty(cleanedKeywords) || tags.Any()))
            {
                SearchQuery query = new SearchQuery
                {
                    KeyWords = cleanedKeywords,
                    Tags = tags,
                    PortalIds = portalIds,
                    SearchTypeIds = searchTypeIds,
                    ModuleDefIds = moduleDefids,
                    BeginModifiedTimeUtc = beginModifiedTimeUtc,
                    PageIndex = 1,
                    PageSize = 5,
                    TitleSnippetLength = 40,
                    BodySnippetLength = 100,
                    CultureCode = culture,
                    WildCardSearch = SearchInputManager.IsWildCardEnabled()
                };

                try
                {
                    results = SearchInputManager.GetGroupedBasicViews(query, userSearchSource, PortalSettings.PortalId);
                    if (results.Count <= 0)
                    {
                        //var basicView = new List<BasicView>();
                        List<GroupedBasicView> NotFound = new List<GroupedBasicView>
                        {
                            new GroupedBasicView(new BasicView { Description = Localization.GetString("NoSearchResultFound", Components.Constants.LocalResourcesFile), Title = "NoSearchResultFound" })
                        };
                        results = NotFound;
                    }
                }
                catch (Exception ex)
                {
                    List<GroupedBasicView> NotFound = new List<GroupedBasicView>
                    {
                        new GroupedBasicView(new BasicView { Description = Localization.GetString("NoSearchResultFound", Components.Constants.LocalResourcesFile), Title = "NoSearchResultFound" })
                    };
                    results = NotFound;
                    ExceptionManager.LogException(ex);
                }
            }
            actionResult.Data = results;

            return actionResult;
        }

        [HttpPost]
        [DnnPageEditor]
        [AuthorizeAccessRoles(AccessRoles = "admin")]
        public void Update(Dictionary<string, string> Attributes)
        {
            Core.Managers.BlockManager.UpdateDesignElement(PortalSettings, Attributes);
        }

        private static List<StringText> GetTemplates()
        {
            string TemplatesPath = HttpContext.Current.Server.MapPath("~/Portals/_default/vThemes/" + ThemeManager.CurrentTheme.Name + "/blocks/search input/Templates");
            List<StringText> Templates = new List<StringText>();
            if (Directory.Exists(TemplatesPath))
            {
                foreach (string file in Directory.GetFiles(TemplatesPath))
                {
                    string FileName = Path.GetFileName(file);
                    if (!string.IsNullOrEmpty(FileName))
                    {
                        if (FileName.EndsWith(".cshtml"))
                        {
                            FileName = FileName.Replace(".cshtml", "");
                            Templates.Add(new StringText() { Value = FileName.ToString(), Text = FileName.ToString() });
                        }
                    }
                }
            }

            return Templates;
        }


        public override string AccessRoles()
        {
            return Factories.AppFactory.GetAccessRoles(UserInfo);
        }
    }
}