using DotNetNuke.Abstractions.Portals;
using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Data;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Tabs;
using DotNetNuke.Entities.Users;
using DotNetNuke.Services.Localization;
using DotNetNuke.Services.Log.EventLog;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Vanjaro.UXManager.Extensions.Menu.Languages.Components;
using Vanjaro.UXManager.Library.Common;

namespace Vanjaro.UXManager.Extensions.Menu.Languages.Managers
{
    public class LanguagesManager
    {
        public static List<LanguageRequest> GetLanguages(PortalSettings PortalSettings, UserInfo UserInfo)
        {
            List<LanguageRequest> Languages = new List<LanguageRequest>();
            try
            {
                foreach (KeyValuePair<string, Locale> item in LocaleController.Instance.GetLocales(Null.NullInteger))
                {
                    LanguageRequest language = new LanguageRequest
                    {
                        LanguageId = item.Value.LanguageId,
                        Icon = Globals.ResolveUrl(
                            string.IsNullOrEmpty(item.Value.Code)
                                ? "~/images/Flags/none.gif"
                                : $"~/images/Flags/{item.Value.Code}.gif"),
                        Code = item.Value.Code,
                        NativeName = item.Value.NativeName,
                        EnglishName = item.Value.EnglishName,
                        Enabled = IsLanguageEnabled(PortalSettings.PortalId, item.Value.Code),
                        IsDefault = item.Value.Code == PortalSettings.DefaultLanguage
                    };
                    Languages.Add(language);
                }
            }
            catch (Exception exc)
            {
                DotNetNuke.Services.Exceptions.Exceptions.LogException(exc);
            }
            return Languages;
        }
        public static bool IsLanguageEnabled(int portalId, string code)
        {
            return LocaleController.Instance.GetLocales(portalId).TryGetValue(code, out Locale enabledLanguage);
        }
        public static ActionResult Update(PortalSettings PortalSettings, UserInfo UserInfo, string Code)
        {
            ActionResult actionResult = new ActionResult();
            try
            {
                if (!UserInfo.IsSuperUser)
                {
                    actionResult.AddError("AuthFailureMessage", Constants.AuthFailureMessage);
                    return actionResult;
                }

                Locale language = LocaleController.Instance.GetLocale(Code) ?? new Locale { Code = Code };
                language.Code = Code;
                language.Fallback = "";
                language.Text = CultureInfo.GetCultureInfo(Code).NativeName;
                Localization.SaveLanguage(language);

                if (!IsLanguageEnabled(PortalSettings.PortalId, language.Code))
                {
                    Localization.AddLanguageToPortal(PortalSettings.PortalId, language.LanguageId, true);
                    UpdateTabUrlsDefaultLocale();
                }

                string roles = $"Administrators;{$"Translator ({language.Code})"}";
                PortalController.UpdatePortalSetting(PortalSettings.PortalId, $"DefaultTranslatorRoles-{language.Code}", roles);
                Config.Touch();
                actionResult.IsSuccess = true;
            }
            catch (Exception exc)
            {
                actionResult.AddError("", "", exc);
            }
            return actionResult;
        }
        public static List<LanguageRequest> GetAllLanguages(bool IsNativeName)
        {
            List<LanguageRequest> Languages = new List<LanguageRequest>();
            try
            {
                List<CultureInfo> supportedLanguages = LocaleController.Instance.GetCultures(LocaleController.Instance.GetLocales(Null.NullInteger));
                List<CultureInfo> cultures = new List<CultureInfo>(CultureInfo.GetCultures(CultureTypes.SpecificCultures));

                foreach (CultureInfo info in supportedLanguages)
                {
                    string cultureCode = info.Name;
                    CultureInfo culture = cultures.SingleOrDefault(c => c.Name == cultureCode);
                    if (culture != null)
                    {
                        cultures.Remove(culture);
                    }
                }

                foreach (CultureInfo info in cultures)
                {
                    LanguageRequest language = new LanguageRequest
                    {
                        Icon = Globals.ResolveUrl(
                            string.IsNullOrEmpty(info.Name)
                                ? "~/images/Flags/none.gif"
                                : $"~/images/Flags/{info.Name}.gif"),
                        Code = info.Name
                    };
                    if (IsNativeName)
                    {
                        language.DisplayName = info.NativeName;
                    }
                    else
                    {
                        language.DisplayName = info.EnglishName;
                    }

                    language.NativeName = info.NativeName;
                    language.EnglishName = info.EnglishName;
                    Languages.Add(language);
                }

            }
            catch (Exception exc)
            {
                DotNetNuke.Services.Exceptions.Exceptions.LogException(exc);
            }
            return Languages.OrderBy(l => l.DisplayName).ToList();
        }

        public static IEnumerable<KeyValuePair<string, string>> GetResxDirectories(string path)
        {
            if (!Directory.Exists(path))
            {
                return new List<KeyValuePair<string, string>>();
            }

            return Directory.GetDirectories(path)
                .Select(folder => new DirectoryInfo(folder))
                .Where(folderInfo => HasLocalResources(folderInfo.FullName))
                .Select(folderInfo => new KeyValuePair<string, string>(folderInfo.Name, folderInfo.FullName));
        }

        public static IEnumerable<KeyValuePair<string, string>> GetResxFiles(string path)
        {
            string sysLocale = Localization.SystemLocale.ToLowerInvariant();
            return
                from file in Directory.GetFiles(path, "*.resx")
                select new FileInfo(file) into fileInfo
                let match = Constants.FileInfoRegex.Match(fileInfo.Name)
                where !match.Success || match.Groups[1].Value.ToLowerInvariant() == sysLocale
                select new KeyValuePair<string, string>(Path.GetFileNameWithoutExtension(fileInfo.Name), fileInfo.FullName);
        }

        private static bool HasLocalResources(string path)
        {
            DirectoryInfo folderInfo = new DirectoryInfo(path);

            if (path.ToLowerInvariant().EndsWith(Localization.LocalResourceDirectory))
            {
                return true;
            }

            if (!Directory.Exists(path))
            {
                return false;
            }

            bool hasResources = false;
            foreach (string folder in Directory.GetDirectories(path))
            {
                if ((File.GetAttributes(folder) & FileAttributes.ReparsePoint) != FileAttributes.ReparsePoint)
                {
                    folderInfo = new DirectoryInfo(folder);
                    hasResources = hasResources || HasLocalResources(folderInfo.FullName);
                }
            }
            return hasResources || folderInfo.GetFiles("*.resx").Length > 0;

        }
        #region Taburls 
        //Set Taburl active to redirect when language is disabled
        internal static void SetTabUrlsActiveToRedirect(int languageID)
        {
            Locale language = LocaleController.Instance.GetLocale(languageID);
            TabCollection tabs = TabController.Instance.GetTabsByPortal(PortalSettings.Current.PortalId);
            foreach (KeyValuePair<int, TabInfo> tab in tabs)
            {
                foreach (TabUrlInfo taburl in tab.Value.TabUrls)
                {
                    if (taburl.CultureCode == language.Code && taburl.HttpStatus == "200")
                    {
                        taburl.HttpStatus = "301";
                        SaveTabUrl(taburl, PortalSettings.Current.PortalId, true);
                    }
                }
            }
        }
        internal static void UpdateTabUrlsDefaultLocale()
        {
            List<PortalAliasInfo> PortalAliases = PortalAliasController.Instance.GetPortalAliasesByPortalId(PortalSettings.Current.PortalId).ToList();
            PortalAliasInfo portalAliasInfo = PortalAliases.Where(x => string.IsNullOrEmpty(x.CultureCode)).FirstOrDefault();
            PortalAliasInfo DefaultLocalePortalAliasInfo = PortalAliases.Where(x => x.CultureCode.ToLower() == PortalSettings.Current.DefaultLanguage.ToLower()).FirstOrDefault();
            TabCollection tabs = TabController.Instance.GetTabsByPortal(PortalSettings.Current.PortalId);
            foreach (KeyValuePair<int, TabInfo> tab in tabs)
            {
                foreach (TabUrlInfo taburl in tab.Value.TabUrls)
                {
                    if (taburl.PortalAliasId == portalAliasInfo.PortalAliasID)
                    {
                        taburl.PortalAliasId = DefaultLocalePortalAliasInfo.PortalAliasID;
                        SaveTabUrl(taburl, PortalSettings.Current.PortalId, true);
                    }
                }
            }
        }
        public static void SaveTabUrl(TabUrlInfo tabUrl, int portalId, bool clearCache)
        {
            //var portalAliasId = (tabUrl.PortalAliasUsage == PortalAliasUsageType.Default) ? Null.NullInteger : tabUrl.PortalAliasId;           
            int portalAliasId = tabUrl.PortalAliasId;
            if (portalAliasId > 0)
            {
                tabUrl.CultureCode = PortalAliasController.Instance.GetPortalAliasByPortalAliasID(portalAliasId).CultureCode;
            }

            EventLogController.EventLogType saveLog = EventLogController.EventLogType.TABURL_CREATED;

            if (tabUrl.HttpStatus == "200")
            {
                saveLog = EventLogController.EventLogType.TABURL_CREATED;
            }
            else
            {
                //need to see if sequence number exists to decide if insert or update
                List<TabUrlInfo> t = TabController.Instance.GetTabUrls(portalId, tabUrl.TabId);
                TabUrlInfo existingSeq = t.FirstOrDefault(r => r.SeqNum == tabUrl.SeqNum);
                if (existingSeq == null)
                {
                    saveLog = EventLogController.EventLogType.TABURL_CREATED;
                }
            }

            DataProvider.Instance().SaveTabUrl(tabUrl.TabId, tabUrl.SeqNum, portalAliasId, (int)tabUrl.PortalAliasUsage, tabUrl.Url, tabUrl.QueryString, tabUrl.CultureCode, tabUrl.HttpStatus, tabUrl.IsSystem, UserController.Instance.GetCurrentUserInfo().UserID);

            EventLogController.Instance.AddLog("tabUrl",
                               tabUrl.ToString(),
                               PortalController.Instance.GetCurrentSettings() as IPortalSettings,
                               UserController.Instance.GetCurrentUserInfo().UserID,
                               saveLog);

            if (clearCache)
            {
                DataCache.RemoveCache(string.Format(DataCache.TabUrlCacheKey, portalId));
                TabController.Instance.ClearCache(portalId);
            }
        }
        #endregion
    }
    public class LanguageRequest
    {
        public int LanguageId { get; set; }
        public string Icon { get; set; }
        public string Code { get; set; }
        public string NativeName { get; set; }
        public string EnglishName { get; set; }
        public string DisplayName { get; set; }
        public bool Enabled { get; set; }
        public bool IsDefault { get; set; }
    }
}