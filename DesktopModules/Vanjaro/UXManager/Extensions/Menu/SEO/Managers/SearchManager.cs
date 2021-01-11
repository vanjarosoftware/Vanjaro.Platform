using Dnn.PersonaBar.SiteSettings.Components;
using Dnn.PersonaBar.SiteSettings.Components.Constants;
using Dnn.PersonaBar.SiteSettings.Services.Dto;
using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Controllers;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using DotNetNuke.Services.Exceptions;
using DotNetNuke.Services.Localization;
using DotNetNuke.Services.Search.Entities;
using DotNetNuke.Services.Search.Internals;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using Vanjaro.UXManager.Library.Common;

namespace Vanjaro.UXManager.Extensions.Menu.SEO
{
    public static partial class Managers
    {
        public static class SearchManager
        {
            private const string AuthFailureMessage = "Authorization has been denied for this request.";
            private const string SearchTitleBoostSetting = "Search_Title_Boost";
            private const string SearchTagBoostSetting = "Search_Tag_Boost";
            private const string SearchContentBoostSetting = "Search_Content_Boost";
            private const string SearchDescriptionBoostSetting = "Search_Description_Boost";
            private const string SearchAuthorBoostSetting = "Search_Author_Boost";
            private const int DefaultSearchTitleBoost = 50;
            private const int DefaultSearchTagBoost = 40;
            private const int DefaultSearchContentBoost = 35;
            private const int DefaultSearchDescriptionBoost = 20;
            private const int DefaultSearchAuthorBoost = 15;
            internal static ActionResult GetIgnoreWords(int? portalId, string cultureCode)
            {
                ActionResult actionResult = new ActionResult();
                try
                {
                    PortalSettings PortalSettings = PortalController.Instance.GetCurrentSettings() as PortalSettings;
                    UserInfo UserInfo = UserController.Instance.GetCurrentUserInfo();

                    int pid = portalId ?? PortalSettings.PortalId;
                    if (!UserInfo.IsSuperUser && pid != PortalSettings.PortalId)
                    {
                        actionResult.AddError("HttpStatusCode.Unauthorized", AuthFailureMessage);
                    }

                    Locale language = LocaleController.Instance.GetLocale(pid, cultureCode);
                    if (language == null)
                    {
                        actionResult.AddError("InvalidLocale.ErrorMessage", Localization.GetString("InvalidLocale.ErrorMessage", Constants.LocalResourcesFile));
                    }

                    SearchStopWords words = SearchHelper.Instance.GetSearchStopWords(pid, string.IsNullOrEmpty(cultureCode) ? LocaleController.Instance.GetCurrentLocale(pid).Code : cultureCode);
                    var response = new
                    {
                        PortalId = pid,
                        CultureCode = cultureCode,
                        StopWordsId = words?.StopWordsId ?? Null.NullInteger,
                        StopWords = words?.StopWords
                    };
                    actionResult.Data = response;
                }
                catch (Exception exc)
                {
                    actionResult.AddError("HttpStatusCode.InternalServerError", exc.Message);
                }
                return actionResult;
            }
            internal static ActionResult GetBasicSearchSettings(dynamic AvailableAnalyzers)
            {
                ActionResult actionResult = new ActionResult();
                try
                {
                    dynamic settings = new ExpandoObject();

                    settings.MinWordLength = HostController.Instance.GetInteger("Search_MinKeyWordLength", 3);
                    settings.MaxWordLength = HostController.Instance.GetInteger("Search_MaxKeyWordLength", 255);
                    settings.AllowLeadingWildcard = HostController.Instance.GetString("Search_AllowLeadingWildcard", "N") == "Y";
                    settings.SearchCustomAnalyzer = HostController.Instance.GetString("Search_CustomAnalyzer", string.Empty);
                    settings.TitleBoost = HostController.Instance.GetInteger(SearchTitleBoostSetting, DefaultSearchTitleBoost);
                    settings.TagBoost = HostController.Instance.GetInteger(SearchTagBoostSetting, DefaultSearchTagBoost);
                    settings.ContentBoost = HostController.Instance.GetInteger(SearchContentBoostSetting, DefaultSearchContentBoost);
                    settings.DescriptionBoost = HostController.Instance.GetInteger(SearchDescriptionBoostSetting, DefaultSearchDescriptionBoost);
                    settings.AuthorBoost = HostController.Instance.GetInteger(SearchAuthorBoostSetting, DefaultSearchAuthorBoost);
                    settings.SearchIndexPath = Path.Combine(Globals.ApplicationMapPath, HostController.Instance.GetString("SearchFolder", @"App_Data\Search"));

                    SearchStatistics searchStatistics = GetSearchStatistics();
                    if (searchStatistics != null)
                    {
                        settings.SearchIndexDbSize = ((searchStatistics.IndexDbSize / 1024f) / 1024f).ToString("N") + " MB";
                        settings.SearchIndexLastModifedOn = DateUtils.CalculateDateForDisplay(searchStatistics.LastModifiedOn);
                        settings.SearchIndexTotalActiveDocuments = searchStatistics.TotalActiveDocuments.ToString(CultureInfo.InvariantCulture);
                        settings.SearchIndexTotalDeletedDocuments = searchStatistics.TotalDeletedDocuments.ToString(CultureInfo.InvariantCulture);
                    }
                    SiteSettingsController _controller = new SiteSettingsController();
                    var response = new
                    {
                        Success = true,
                        Settings = settings,
                        SearchCustomAnalyzers = AvailableAnalyzers
                    };
                    actionResult.Data = settings;
                }
                catch (Exception exc)
                {
                    actionResult.AddError("HttpStatusCode.InternalServerError", exc.Message);
                }
                return actionResult;
            }
            private static SearchStatistics GetSearchStatistics()
            {
                try
                {
                    return InternalSearchController.Instance.GetSearchStatistics();
                }
                catch (SearchIndexEmptyException ex)
                {
                    Core.Managers.ExceptionManage.LogException(ex);
                    return null;
                }
                catch (Exception ex)
                {
                    Core.Managers.ExceptionManage.LogException(ex);
                    return null;
                }
            }
            #region Update Search Setting
            internal static ActionResult UpdateBasicSearchSettings(UpdateBasicSearchSettingsRequest request)
            {
                ActionResult actionResult = new ActionResult();
                try
                {
                    if (request.MinWordLength == Null.NullInteger || request.MinWordLength == 0)
                    {
                        actionResult.AddError("valIndexWordMinLengthRequired.Error", Localization.GetString("valIndexWordMinLengthRequired.Error", Constants.LocalResourcesFile));
                    }
                    else if (request.MaxWordLength == Null.NullInteger || request.MaxWordLength == 0)
                    {
                        actionResult.AddError("valIndexWordMaxLengthRequired.Error", Localization.GetString("valIndexWordMaxLengthRequired.Error", Constants.LocalResourcesFile));
                    }
                    else if (request.MinWordLength >= request.MaxWordLength)
                    {
                        actionResult.AddError("valIndexWordMaxLengthRequired.Error", Localization.GetString("valIndexWordMaxLengthRequired.Error", Constants.LocalResourcesFile));
                    }

                    if (actionResult.IsSuccess)
                    {
                        int oldMinLength = HostController.Instance.GetInteger("Search_MinKeyWordLength", 3);
                        if (request.MinWordLength != oldMinLength)
                        {
                            HostController.Instance.Update("Search_MinKeyWordLength", request.MinWordLength.ToString());
                        }

                        int oldMaxLength = HostController.Instance.GetInteger("Search_MaxKeyWordLength", 255);
                        if (request.MaxWordLength != oldMaxLength)
                        {
                            HostController.Instance.Update("Search_MaxKeyWordLength", request.MaxWordLength.ToString());
                        }

                        HostController.Instance.Update("Search_AllowLeadingWildcard", request.AllowLeadingWildcard ? "Y" : "N");
                        HostController.Instance.Update(SearchTitleBoostSetting, (request.TitleBoost == Null.NullInteger) ? DefaultSearchTitleBoost.ToString() : request.TitleBoost.ToString());
                        HostController.Instance.Update(SearchTagBoostSetting, (request.TagBoost == Null.NullInteger) ? DefaultSearchTagBoost.ToString() : request.TagBoost.ToString());
                        HostController.Instance.Update(SearchContentBoostSetting, (request.ContentBoost == Null.NullInteger) ? DefaultSearchContentBoost.ToString() : request.ContentBoost.ToString());
                        HostController.Instance.Update(SearchDescriptionBoostSetting, (request.DescriptionBoost == Null.NullInteger) ? DefaultSearchDescriptionBoost.ToString() : request.DescriptionBoost.ToString());
                        HostController.Instance.Update(SearchAuthorBoostSetting, (request.AuthorBoost == Null.NullInteger) ? DefaultSearchAuthorBoost.ToString() : request.AuthorBoost.ToString());

                        string oldAnalyzer = HostController.Instance.GetString("Search_CustomAnalyzer", string.Empty);
                        string newAnalyzer = request.SearchCustomAnalyzer.Trim();
                        if (!oldAnalyzer.Equals(newAnalyzer))
                        {
                            HostController.Instance.Update("Search_CustomAnalyzer", newAnalyzer);
                            //force the app restart to use new analyzer.
                            Config.Touch();
                        }
                    }
                }
                catch (Exception ex)
                {
                    actionResult.AddError("HttpStatusCode.InternalServerError", ex.Message);
                }
                return actionResult;
            }
            internal static ActionResult UpdateSynonymsGroup(List<SynonymsGroup> SynonymsGroupRequest)
            {
                ActionResult actionResult = new ActionResult();

                try
                {
                    PortalSettings PortalSettings = PortalController.Instance.GetCurrentSettings() as PortalSettings;
                    UserInfo UserInfo = UserController.Instance.GetCurrentUserInfo();
                    int pid = (PortalController.Instance.GetCurrentSettings() as PortalSettings).PortalId;
                    foreach (SynonymsGroup request in SynonymsGroupRequest)
                    {
                        //if (!UserInfo.IsSuperUser)
                        //    actionResult.AddError(HttpStatusCode.Unauthorized.ToString(), AuthFailureMessage);

                        if (actionResult.IsSuccess)
                        {
                            string cultureCode = LocaleController.Instance.GetCurrentLocale(pid).Code;

                            Locale language = LocaleController.Instance.GetLocale(pid, cultureCode);
                            if (language == null)
                            {
                                actionResult.AddError(HttpStatusCode.BadRequest.ToString(), Localization.GetString("InvalidLocale.ErrorMessage", Constants.LocalResourcesFile));
                            }

                            if (actionResult.IsSuccess)
                            {
                                //IF no SynomsTag found then Delete the synonymsTags Group
                                if (request.SynonymsGroupId > 0 && string.IsNullOrEmpty(request.SynonymsTags))
                                {
                                    SearchHelper.Instance.DeleteSynonymsGroup(request.SynonymsGroupId, pid, cultureCode);
                                }
                                else
                                {
                                    //Add and Update UpdateSynonymsGroup
                                    string duplicateWord; int synonymsGroupId;
                                    if (request.SynonymsGroupId > 0)
                                    {
                                        synonymsGroupId = SearchHelper.Instance.UpdateSynonymsGroup(request.SynonymsGroupId, request.SynonymsTags, pid, cultureCode, out duplicateWord);
                                    }
                                    else
                                    {
                                        synonymsGroupId = SearchHelper.Instance.AddSynonymsGroup(request.SynonymsTags, pid, cultureCode, out duplicateWord);
                                    }

                                    if (!(synonymsGroupId > 0))
                                    {
                                        actionResult.AddError(HttpStatusCode.BadRequest.ToString(), "[" + duplicateWord + "] " + Localization.GetString("SynonymsTagDuplicated", Constants.LocalResourcesFile));
                                    }
                                }
                            }
                            else
                            {
                                actionResult.AddError(HttpStatusCode.BadRequest.ToString(), Localization.GetString(HttpStatusCode.BadRequest.ToString(), Components.Constants.ResourcesFile));
                            }
                        }
                    }

                    if (actionResult.IsSuccess)
                    {
                        List<dynamic> synonymsGroups = new List<dynamic>();
                        foreach (SynonymsGroup sg in SearchHelper.Instance.GetSynonymsGroups(pid, LocaleController.Instance.GetCurrentLocale(pid).Code).Cast<SynonymsGroup>().OrderBy(o => o.SynonymsGroupId))
                        {
                            dynamic synonymsGroup = new ExpandoObject();
                            synonymsGroup.CreatedByUserId = sg.CreatedByUserId;
                            synonymsGroup.CreatedOnDate = sg.CreatedOnDate;
                            synonymsGroup.LastModifiedByUserId = sg.LastModifiedByUserId;
                            synonymsGroup.LastModifiedOnDate = sg.LastModifiedOnDate;
                            synonymsGroup.PortalId = sg.PortalId;
                            synonymsGroup.SynonymsGroupId = sg.SynonymsGroupId;
                            synonymsGroup.SynonymsTags = sg.SynonymsTags;
                            synonymsGroups.Add(synonymsGroup);
                        }
                        actionResult.Data = synonymsGroups;
                    }
                }
                catch (Exception exc)
                {
                    actionResult.AddError(HttpStatusCode.InternalServerError.ToString(), exc.Message);
                }
                return actionResult;
            }
            internal static ActionResult AddIgnoreWords(UpdateIgnoreWordsRequest request)
            {
                ActionResult actionResult = new ActionResult();
                try
                {
                    PortalSettings PortalSettings = PortalController.Instance.GetCurrentSettings() as PortalSettings;
                    UserInfo UserInfo = UserController.Instance.GetCurrentUserInfo();

                    int pid = request.PortalId ?? PortalSettings.PortalId;
                    if (!UserInfo.IsSuperUser && pid != PortalSettings.PortalId)
                    {
                        actionResult.AddError("HttpStatusCode.Unauthorized", AuthFailureMessage);
                    }

                    string cultureCode = string.IsNullOrEmpty(request.CultureCode)
                        ? LocaleController.Instance.GetCurrentLocale(pid).Code
                        : request.CultureCode;

                    Locale language = LocaleController.Instance.GetLocale(pid, cultureCode);
                    if (language == null)
                    {
                        actionResult.AddError("InvalidLocale.ErrorMessage", Localization.GetString("InvalidLocale.ErrorMessage", Constants.LocalResourcesFile));
                    }
                    //var stopWordsId = SearchHelper.Instance.AddSearchStopWords(request.StopWords, pid, cultureCode);
                    //actionResult.Data = stopWordsId;

                    if (request.StopWordsId > 0)
                    {
                        SearchHelper.Instance.UpdateSearchStopWords(request.StopWordsId, request.StopWords, pid, cultureCode);
                    }
                    else
                    {
                        int stopWordsId = SearchHelper.Instance.AddSearchStopWords(request.StopWords, pid, cultureCode);
                        actionResult.Data = stopWordsId;
                    }
                }
                catch (Exception exc)
                {
                    actionResult.AddError("HttpStatusCode.InternalServerError", exc.Message);
                }
                return actionResult;
            }
            internal static ActionResult PortalSearchReindex(int? portalId)
            {
                ActionResult actionResult = new ActionResult();
                try
                {
                    PortalSettings PortalSettings = PortalController.Instance.GetCurrentSettings() as PortalSettings;
                    UserInfo UserInfo = UserController.Instance.GetCurrentUserInfo();

                    int pid = portalId ?? PortalSettings.PortalId;
                    if (!UserInfo.IsSuperUser && pid != PortalSettings.PortalId)
                    {
                        actionResult.AddError("HttpStatusCode.Unauthorized", AuthFailureMessage);
                    }

                    SearchHelper.Instance.SetSearchReindexRequestTime(pid);
                    //actionResult.Data = Request.CreateResponse(HttpStatusCode.OK, new { Success = true });
                }
                catch (Exception exc)
                {
                    actionResult.AddError("HttpStatusCode.InternalServerError", exc.Message);
                }
                return actionResult;
            }
            #endregion
        }
    }
}