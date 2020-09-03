using Dnn.PersonaBar.SiteSettings.Services.Dto;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using DotNetNuke.Services.Localization;
using System;
using System.Linq;
using System.Net;
using Vanjaro.UXManager.Library.Common;

namespace Vanjaro.UXManager.Extensions.Menu.Domain
{
    public static partial class Managers
    {
        public class DomainManager
        {
            public static ActionResult GetSiteAliases(int? portalId, UserInfo userInfo)
            {
                ActionResult actionResult = new ActionResult();
                try
                {
                    int pid = portalId ?? PortalSettings.Current.PortalId;
                    if (!userInfo.IsSuperUser && PortalSettings.Current.PortalId != pid)
                    {
                        actionResult.AddError(HttpStatusCode.Unauthorized.ToString(), Components.Constants.AuthFailureMessage);
                    }

                    if (actionResult.IsSuccess)
                    {
                        var aliases = PortalAliasController.Instance.GetPortalAliasesByPortalId(pid).Select(a => new
                        {
                            a.PortalAliasID,
                            a.HTTPAlias,
                            BrowserType = a.BrowserType.ToString(),
                            a.Skin,
                            a.IsPrimary,
                            a.CultureCode,
                            Deletable = a.PortalAliasID != PortalSettings.Current.PortalAlias.PortalAliasID && !a.IsPrimary,
                            Editable = a.PortalAliasID != PortalSettings.Current.PortalAlias.PortalAliasID
                        });
                        actionResult.Data = new
                        {
                            PortalAliases = aliases,
                            Languages = LocaleController.Instance.GetLocales(pid).Select(l => new
                            {
                                l.Key,
                                Value = l.Key
                            })
                        };
                    }
                }
                catch (Exception exc)
                {
                    actionResult.AddError(HttpStatusCode.InternalServerError.ToString(), exc.Message);
                }
                return actionResult;
            }
            public static ActionResult GetSiteAlias(UserInfo userInfo, int portalAliasId)
            {
                ActionResult actionResult = new ActionResult();
                try
                {
                    PortalAliasInfo alias = PortalAliasController.Instance.GetPortalAliasByPortalAliasID(portalAliasId);
                    if (!userInfo.IsSuperUser && alias.PortalID != PortalSettings.Current.PortalId)
                    {
                        actionResult.AddError(HttpStatusCode.Unauthorized.ToString(), Components.Constants.AuthFailureMessage);
                    }

                    if (actionResult.IsSuccess)
                    {
                        UpdateSiteAliasRequest PortalAlias = new UpdateSiteAliasRequest()
                        {
                            PortalId = alias.PortalID,
                            PortalAliasID = alias.PortalAliasID,
                            HTTPAlias = alias.HTTPAlias,
                            BrowserType = alias.BrowserType.ToString(),
                            Skin = alias.Skin,
                            CultureCode = alias.CultureCode,
                            IsPrimary = alias.IsPrimary
                        };
                        actionResult.Data = PortalAlias;
                    }
                }
                catch (Exception exc)
                {
                    actionResult.AddError(HttpStatusCode.InternalServerError.ToString(), exc.Message);
                }
                return actionResult;
            }
        }
    }
}