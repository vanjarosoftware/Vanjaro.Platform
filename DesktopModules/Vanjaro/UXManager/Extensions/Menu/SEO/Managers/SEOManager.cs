using Dnn.PersonaBar.Seo.Services.Dto;
using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Controllers;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Urls;
using DotNetNuke.Services.Localization;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using Vanjaro.UXManager.Extensions.Menu.SEO.Components;
using Vanjaro.UXManager.Library.Common;

namespace Vanjaro.UXManager.Extensions.Menu.SEO
{
    public static partial class Managers
    {
        public class SEOManager
        {
            #region General settings
            public static ActionResult GetGeneralSettings()
            {
                ActionResult actionResult = new ActionResult();
                try
                {
                    FriendlyUrlSettings urlSettings = new FriendlyUrlSettings(PortalSettings.Current.PortalId);
                    var Settings = new
                    {
                        EnableSystemGeneratedUrls = urlSettings.ReplaceSpaceWith != FriendlyUrlSettings.ReplaceSpaceWithNothing,
                        urlSettings.ReplaceSpaceWith,
                        urlSettings.ForceLowerCase,
                        urlSettings.AutoAsciiConvert,
                        urlSettings.ForcePortalDefaultLanguage,
                        DeletedTabHandlingType = urlSettings.DeletedTabHandlingType.ToString(),
                        urlSettings.RedirectUnfriendly,
                        urlSettings.RedirectWrongCase
                    };
                    actionResult.Data = Settings;
                }
                catch (Exception exc)
                {
                    actionResult.AddError(HttpStatusCode.InternalServerError.ToString(), exc.Message);
                }
                return actionResult;
            }
            internal static ActionResult UpdateGeneralSettings(UpdateGeneralSettingsRequest request)
            {
                ActionResult actionResult = new ActionResult();
                try
                {
                    string characterSub = FriendlyUrlSettings.ReplaceSpaceWithNothing;
                    int PortalId = PortalSettings.Current.PortalId;
                    if (request.EnableSystemGeneratedUrls)
                    {
                        characterSub = request.ReplaceSpaceWith;
                    }
                    PortalController.UpdatePortalSetting(PortalId, FriendlyUrlSettings.DeletedTabHandlingTypeSetting, request.DeletedTabHandlingType, false);
                    DataCache.ClearPortalCache(PortalId, false);
                    DataCache.ClearTabsCache(PortalId);
                }
                catch (Exception exc)
                {
                    actionResult.AddError(HttpStatusCode.InternalServerError.ToString(), exc.Message);
                }
                return actionResult;
            }
            #endregion

            #region Regex Settings
            public static ActionResult GetRegexSettings()
            {
                ActionResult actionResult = new ActionResult();
                try
                {
                    FriendlyUrlSettings urlSettings = new FriendlyUrlSettings(PortalSettings.Current.PortalId);
                    var Settings = new
                    {
                        urlSettings.IgnoreRegex,
                        urlSettings.DoNotRewriteRegex,
                        urlSettings.UseSiteUrlsRegex,
                        urlSettings.DoNotRedirectRegex,
                        urlSettings.DoNotRedirectSecureRegex,
                        urlSettings.ForceLowerCaseRegex,
                        urlSettings.NoFriendlyUrlRegex,
                        urlSettings.DoNotIncludeInPathRegex,
                        urlSettings.ValidExtensionlessUrlsRegex,
                        urlSettings.RegexMatch
                    };
                    actionResult.Data = Settings;
                }
                catch (Exception exc)
                {
                    actionResult.AddError(HttpStatusCode.InternalServerError.ToString(), exc.Message);
                }
                return actionResult;
            }
            internal static ActionResult UpdateRegexSettings(UpdateRegexSettingsRequest request)
            {
                ActionResult actionResult = new ActionResult();
                try
                {
                    List<KeyValuePair<string, string>> validationErrors = new List<KeyValuePair<string, string>>();
                    if (!ValidateRegex(request.IgnoreRegex))
                    {
                        validationErrors.Add(new KeyValuePair<string, string>("IgnoreRegex", Localization.GetString("ignoreRegExInvalidPattern", Constants.DNNLocalResourcesFile)));
                    }
                    if (!ValidateRegex(request.DoNotRewriteRegex))
                    {
                        validationErrors.Add(new KeyValuePair<string, string>("DoNotRewriteRegex", Localization.GetString("doNotRewriteRegExInvalidPattern", Constants.DNNLocalResourcesFile)));
                    }
                    if (!ValidateRegex(request.UseSiteUrlsRegex))
                    {
                        validationErrors.Add(new KeyValuePair<string, string>("UseSiteUrlsRegex", Localization.GetString("siteUrlsOnlyRegExInvalidPattern", Constants.DNNLocalResourcesFile)));
                    }
                    if (!ValidateRegex(request.DoNotRedirectRegex))
                    {
                        validationErrors.Add(new KeyValuePair<string, string>("DoNotRedirectRegex", Localization.GetString("doNotRedirectUrlRegExInvalidPattern", Constants.DNNLocalResourcesFile)));
                    }
                    if (!ValidateRegex(request.DoNotRedirectSecureRegex))
                    {
                        validationErrors.Add(new KeyValuePair<string, string>("DoNotRedirectSecureRegex", Localization.GetString("doNotRedirectHttpsUrlRegExInvalidPattern", Constants.DNNLocalResourcesFile)));
                    }
                    if (!ValidateRegex(request.ForceLowerCaseRegex))
                    {
                        validationErrors.Add(new KeyValuePair<string, string>("ForceLowerCaseRegex", Localization.GetString("preventLowerCaseUrlRegExInvalidPattern", Constants.DNNLocalResourcesFile)));
                    }
                    if (!ValidateRegex(request.NoFriendlyUrlRegex))
                    {
                        validationErrors.Add(new KeyValuePair<string, string>("NoFriendlyUrlRegex", Localization.GetString("doNotUseFriendlyUrlsRegExInvalidPattern", Constants.DNNLocalResourcesFile)));
                    }
                    if (!ValidateRegex(request.DoNotIncludeInPathRegex))
                    {
                        validationErrors.Add(new KeyValuePair<string, string>("DoNotIncludeInPathRegex", Localization.GetString("keepInQueryStringRegExInvalidPattern", Constants.DNNLocalResourcesFile)));
                    }
                    if (!ValidateRegex(request.ValidExtensionlessUrlsRegex))
                    {
                        validationErrors.Add(new KeyValuePair<string, string>("ValidExtensionlessUrlsRegex", Localization.GetString("urlsWithNoExtensionRegExInvalidPattern", Constants.DNNLocalResourcesFile)));
                    }
                    if (!ValidateRegex(request.RegexMatch))
                    {
                        validationErrors.Add(new KeyValuePair<string, string>("RegexMatch", Localization.GetString("validFriendlyUrlRegExInvalidPattern", Constants.DNNLocalResourcesFile)));
                    }

                    if (validationErrors.Count > 0)
                    {
                        foreach (KeyValuePair<string, string> error in validationErrors)
                        {
                            actionResult.AddError(error.Key, error.Value);
                        }
                    }
                    else
                    {
                        if (actionResult.IsSuccess)
                        {
                            // if no errors, update settings in db
                            UpdateRegexSettingsInternal(request);
                            // clear cache
                            ClearCache();
                        }
                    }
                }
                catch (Exception exc)
                {
                    actionResult.AddError(HttpStatusCode.InternalServerError.ToString(), exc.Message);
                }
                return actionResult;
            }
            private static void UpdateRegexSettingsInternal(UpdateRegexSettingsRequest request)
            {
                Dictionary<string, string> settings = new Dictionary<string, string>() {
                        { FriendlyUrlSettings.IgnoreRegexSetting, request.IgnoreRegex },
                        { FriendlyUrlSettings.DoNotRewriteRegExSetting, request.DoNotRewriteRegex },
                        { FriendlyUrlSettings.SiteUrlsOnlyRegexSetting, request.UseSiteUrlsRegex },
                        { FriendlyUrlSettings.DoNotRedirectUrlRegexSetting, request.DoNotRedirectRegex },
                        { FriendlyUrlSettings.DoNotRedirectHttpsUrlRegexSetting, request.DoNotRedirectSecureRegex },
                        { FriendlyUrlSettings.PreventLowerCaseUrlRegexSetting, request.ForceLowerCaseRegex },
                        { FriendlyUrlSettings.DoNotUseFriendlyUrlRegexSetting, request.NoFriendlyUrlRegex },
                        { FriendlyUrlSettings.KeepInQueryStringRegexSetting, request.DoNotIncludeInPathRegex },
                        { FriendlyUrlSettings.UrlsWithNoExtensionRegexSetting, request.ValidExtensionlessUrlsRegex },
                        { FriendlyUrlSettings.ValidFriendlyUrlRegexSetting, request.RegexMatch }};

                settings.ToList().ForEach((value) =>
                {
                    if (PortalSettings.Current.PortalId == Null.NullInteger)
                    {
                        HostController.Instance.Update(value.Key, value.Value, false);
                    }
                    else
                    {
                        PortalController.Instance.UpdatePortalSetting(PortalSettings.Current.PortalId, value.Key, value.Value, false, Null.NullString, false);
                    }
                });
            }

            private static void ClearCache()
            {
                if (PortalSettings.Current.PortalId == Null.NullInteger)
                {
                    DataCache.ClearHostCache(false);
                }
                else
                {
                    DataCache.ClearPortalCache(PortalSettings.Current.PortalId, false);
                }
                CacheController.FlushPageIndexFromCache();
                CacheController.FlushFriendlyUrlSettingsFromCache();
            }

            private static bool ValidateRegex(string regexPattern)
            {
                try
                {
                    if (Regex.IsMatch("", regexPattern))
                    {
                    }

                    return true;
                }
                catch
                {
                    //ignore
                }
                return false;
            }
            #endregion

            #region Sitemap Settings
            public static ActionResult GetSitemapSettings()
            {
                ActionResult actionResult = new ActionResult();
                try
                {
                    string portalAlias = !string.IsNullOrEmpty(PortalSettings.Current.DefaultPortalAlias)
                                    ? PortalSettings.Current.DefaultPortalAlias
                                    : PortalSettings.Current.PortalAlias.HTTPAlias;

                    string str = PortalController.GetPortalSetting("SitemapMinPriority", PortalSettings.Current.PortalId, "0.1");
                    if (!float.TryParse(str, out float sitemapMinPriority))
                    {
                        sitemapMinPriority = 0.1f;
                    }

                    str = PortalController.GetPortalSetting("SitemapExcludePriority", PortalSettings.Current.PortalId, "0.1");
                    if (!float.TryParse(str, out float sitemapExcludePriority))
                    {
                        sitemapExcludePriority = 0.1f;
                    }

                    var Settings = new
                    {
                        SitemapUrl = Globals.AddHTTP(portalAlias) + @"/SiteMap.aspx",
                        SitemapLevelMode = PortalController.GetPortalSettingAsBoolean("SitemapLevelMode", PortalSettings.Current.PortalId, false),
                        SitemapMinPriority = sitemapMinPriority,
                        SitemapIncludeHidden = PortalController.GetPortalSettingAsBoolean("SitemapIncludeHidden", PortalSettings.Current.PortalId, false),
                        SitemapExcludePriority = sitemapExcludePriority,
                        SitemapCacheDays = PortalController.GetPortalSettingAsInteger("SitemapCacheDays", PortalSettings.Current.PortalId, 1)
                    };
                    actionResult.Data = Settings;
                }
                catch (Exception exc)
                {
                    actionResult.AddError(HttpStatusCode.InternalServerError.ToString(), exc.Message);
                }
                return actionResult;
            }
            internal static ActionResult UpdateSitemapSettings(SitemapSettingsRequest request)
            {
                ActionResult actionResult = new ActionResult();
                try
                {
                    PortalController.UpdatePortalSetting(PortalSettings.Current.PortalId, "SitemapLevelMode", request.SitemapLevelMode.ToString());

                    if (request.SitemapMinPriority < 0)
                    {
                        request.SitemapMinPriority = 0;
                    }
                    PortalController.UpdatePortalSetting(PortalSettings.Current.PortalId, "SitemapMinPriority", request.SitemapMinPriority.ToString(NumberFormatInfo.InvariantInfo));

                    PortalController.UpdatePortalSetting(PortalSettings.Current.PortalId, "SitemapIncludeHidden", request.SitemapIncludeHidden.ToString());

                    if (request.SitemapExcludePriority < 0)
                    {
                        request.SitemapExcludePriority = 0;
                    }
                    PortalController.UpdatePortalSetting(PortalSettings.Current.PortalId, "SitemapExcludePriority", request.SitemapExcludePriority.ToString(NumberFormatInfo.InvariantInfo));

                    if (request.SitemapCacheDays == 0)
                    {
                        Managers.SEOManager.ResetCache();
                    }
                    PortalController.UpdatePortalSetting(PortalSettings.Current.PortalId, "SitemapCacheDays", request.SitemapCacheDays.ToString());
                }
                catch (Exception exc)
                {
                    actionResult.AddError(HttpStatusCode.InternalServerError.ToString(), exc.Message);
                }
                return actionResult;
            }
            public static Dictionary<int, string> BindSitemapCacheDays()
            {
                Dictionary<int, string> keyValuePairs = new Dictionary<int, string>();
                for (int i = 0; i <= 7; i++)
                {
                    if (i == 0)
                    {
                        keyValuePairs.Add(i, Localization.GetString("DisableCaching", Components.Constants.DNNLocalResourcesFile));
                    }
                    else if (i == 1)
                    {
                        keyValuePairs.Add(i, Localization.GetString("1Day", Components.Constants.DNNLocalResourcesFile));
                    }
                    else
                    {
                        keyValuePairs.Add(i, Localization.GetString(i + "Days", Components.Constants.DNNLocalResourcesFile));
                    }
                }
                return keyValuePairs;
            }
            #endregion

            // Resets cache
            internal static void ResetCache()
            {
                DirectoryInfo cacheFolder = new DirectoryInfo(PortalSettings.Current.HomeSystemDirectoryMapPath + "sitemap\\");

                if (cacheFolder.Exists)
                {
                    foreach (FileInfo file in cacheFolder.GetFiles("sitemap*.xml"))
                    {
                        file.Delete();
                    }
                }
            }
        }
    }
}