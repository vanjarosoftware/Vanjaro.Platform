using Dnn.PersonaBar.Pages.Components;
using Dnn.PersonaBar.Pages.Components.Dto;
using Dnn.PersonaBar.Pages.Components.Security;
using Dnn.PersonaBar.Pages.Services.Dto;
using DotNetNuke.Entities.Portals;
using System.Collections.Generic;
using System.Linq;
using Vanjaro.UXManager.Library.Common;
using DNNLocalization = DotNetNuke.Services.Localization;

namespace Vanjaro.UXManager.Extensions.Menu.Pages
{
    public static partial class Managers
    {
        public class UrlManager
        {

            public static ActionResult CreateCustomUrl(SeoUrl dto)
            {
                ActionResult actionResult = new ActionResult();
                if (!SecurityService.Instance.CanManagePage(dto.TabId))
                {
                    //HttpStatusCode.Forbidden  message is hardcoded in DotNetnuke so we localized our side.
                    actionResult.AddError("HttpStatusCode.Forbidden", DotNetNuke.Services.Localization.Localization.GetString("UserAuthorizationForbidden", Components.Constants.LocalResourcesFile));
                }

                if (actionResult.IsSuccess)
                {
                    PageUrlResult result = PagesController.Instance.CreateCustomUrl(dto);
                    if (result.Success)
                    {
                        actionResult.Data = GetCustomUrls(dto.TabId);
                    }
                    else
                    {
                        actionResult.AddError("UrlPathNotUnique.Error", result.ErrorMessage);
                        actionResult.Data = actionResult.Data = new { result.SuggestedUrlPath };
                    }
                }
                return actionResult;
            }

            public static ActionResult UpdateCustomUrl(SeoUrl dto)
            {
                ActionResult actionResult = new ActionResult();
                if (!SecurityService.Instance.CanManagePage(dto.TabId))
                {
                    actionResult.AddError("CustomUrlPortalAlias.Error", Localization.GetString("CustomUrlPortalAlias.Error"));
                }

                if (actionResult.IsSuccess)
                {
                    PageUrlResult result = PagesController.Instance.UpdateCustomUrl(dto);
                    if (result.Success)
                    {
                        actionResult.Data = GetCustomUrls(dto.TabId);
                    }
                    else
                    {
                        actionResult.AddError("UrlPathNotUnique.Error", result.ErrorMessage);
                        actionResult.Data = actionResult.Data = new { result.SuggestedUrlPath };
                    }
                }
                return actionResult;
            }

            public static IEnumerable<Url> GetCustomUrls(int pageId)
            {
                List<Url> CustomUrls = PagesController.Instance.GetPageUrls(pageId).ToList();
                bool IsLocaleNotEnabled = string.IsNullOrEmpty(PortalSettings.Current.PortalAlias.CultureCode);
                List<Url> Urls = new List<Url>();
                foreach (Url url in CustomUrls)
                {
                    DNNLocalization.Locale locale = DNNLocalization.LocaleController.Instance.GetLocale(url.Locale.Key);
                    if (!string.IsNullOrEmpty(url.Locale.Value) && locale != null && locale.Code == PortalSettings.Current.CultureCode)
                    {
                        Urls.Add(url);
                    }
                }
                if (Urls.Count == 0 || IsLocaleNotEnabled)
                {
                    foreach (Url url in CustomUrls)
                    {
                        DNNLocalization.Locale locale = DNNLocalization.LocaleController.Instance.GetLocale(url.Locale.Key);
                        if (locale == null && url.SiteAlias.Value == PortalSettings.Current.DefaultPortalAlias)
                        {
                            Urls.Add(url);
                        }
                    }
                }
                return Urls;
            }

            public static IEnumerable<KeyValuePair<int, string>> StatusCodes => new[]
                    {
                      new KeyValuePair<int, string>(200, DotNetNuke.Services.Localization.Localization.GetString("Active", Components.Constants.LocalResourcesFile)),
                      new KeyValuePair<int, string>(301, DotNetNuke.Services.Localization.Localization.GetString("Redirect", Components.Constants.LocalResourcesFile))
                     };

            public static ActionResult DeleteCustomUrl(UrlIdDto dto)
            {
                ActionResult actionResult = new ActionResult();

                if (!SecurityService.Instance.CanManagePage(dto.TabId))
                {
                    //HttpStatusCode.Forbidden  message is hardcoded in DotNetnuke so we localized our side.
                    actionResult.AddError("HttpStatusCode.Forbidden", DotNetNuke.Services.Localization.Localization.GetString("UserAuthorizationForbidden", Components.Constants.LocalResourcesFile));
                }
                if (actionResult.IsSuccess)
                {
                    PageUrlResult result = PagesController.Instance.DeleteCustomUrl(dto);
                    if (result.Success)
                    {
                        actionResult.Data = GetCustomUrls(dto.TabId);
                    }
                    else
                    {
                        actionResult.AddError("DeleteCustomUrlError", result.ErrorMessage);
                    }
                }

                return actionResult;
            }
        }
    }
}