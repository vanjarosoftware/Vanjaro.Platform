using Dnn.PersonaBar.SiteSettings.Services.Dto;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Urls;
using DotNetNuke.Entities.Users;
using DotNetNuke.Services.Localization;
using DotNetNuke.Web.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Http;
using Vanjaro.Common.ASPNET.WebAPI;
using Vanjaro.Common.Engines.UIEngine;
using Vanjaro.UXManager.Extensions.Menu.Domain.Factories;
using Vanjaro.UXManager.Library.Common;

namespace Vanjaro.UXManager.Extensions.Menu.Domain.Controllers
{
    [ValidateAntiForgeryToken]
    [AuthorizeAccessRoles(AccessRoles = "admin")]
    public class DomainController : UIEngineController
    {
        public static List<IUIData> GetData(string Identifier, Dictionary<string, string> parameters, Dictionary<string, string> UIEngineInfo, UserInfo userInfo, PortalSettings portalSettings)
        {
            Dictionary<string, IUIData> Settings = new Dictionary<string, IUIData>();
            switch (Identifier.ToLower())
            {
                case "domain_domains":
                    {
                        string ActiveSiteAlias = portalSettings.PortalAlias != null && !string.IsNullOrEmpty(portalSettings.PortalAlias.HTTPAlias) ? portalSettings.PortalAlias.HTTPAlias : string.Empty;
                        Settings.Add("PortalAliases", new UIData { Name = "PortalAliases", Options = Managers.DomainManager.GetSiteAliases(portalSettings.PortalId, userInfo).Data, Value = ActiveSiteAlias });
                        return Settings.Values.ToList();
                    }
                case "domain_settings":
                    {
                        int sid = 0;
                        UpdateSiteAliasRequest updateSiteAliasRequest = new UpdateSiteAliasRequest() { PortalId = portalSettings.PortalId, CultureCode = portalSettings.CultureCode };
                        if (parameters.Count > 0)
                        {
                            sid = int.Parse(parameters["sid"]);
                        }

                        if (sid > 0)
                        {
                            updateSiteAliasRequest = Managers.DomainManager.GetSiteAlias(userInfo, sid).Data;
                        }

                        Settings.Add("UpdateSiteAliasRequest", new UIData { Name = "UpdateSiteAliasRequest", Options = updateSiteAliasRequest });
                        Settings.Add("Languages", new UIData { Name = "Languages", Options = Managers.DomainManager.GetSiteAliases(portalSettings.PortalId, userInfo).Data.Languages, Value = !string.IsNullOrEmpty(updateSiteAliasRequest.CultureCode) ? updateSiteAliasRequest.CultureCode : "Default" });
                        return Settings.Values.ToList();
                    }
                default:
                    return null;
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddUpdateSiteAlias(UpdateSiteAliasRequest request)
        {
            ActionResult actionResult = new ActionResult();

            if (request.PortalAliasID > 0)
            {
                actionResult = UpdateSiteAlias(request);
            }
            else
            {
                actionResult = AddSiteAlias(request);
            }

            return actionResult;
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddSiteAlias(UpdateSiteAliasRequest request)
        {
            ActionResult actionResult = new ActionResult();
            try
            {
                int pid = request.PortalId ?? PortalSettings.Current.PortalId;
                if (!UserInfo.IsSuperUser && pid != PortalSettings.Current.PortalId)
                {
                    actionResult.AddError(HttpStatusCode.Unauthorized.ToString(), Components.Constants.AuthFailureMessage);
                }

                string strAlias = request.HTTPAlias;
                if (!string.IsNullOrEmpty(strAlias))
                {
                    strAlias = strAlias.Trim();
                }

                if (IsHttpAliasValid(strAlias))
                {
                    PortalAliasCollection aliases = PortalAliasController.Instance.GetPortalAliases();
                    if (aliases.Contains(strAlias))
                    {
                        actionResult.AddError(HttpStatusCode.BadRequest.ToString(), string.Format(Localization.GetString("DuplicateAlias", Dnn.PersonaBar.SiteSettings.Components.Constants.Constants.LocalResourcesFile)));
                    }

                    if (actionResult.IsSuccess)
                    {
                        Enum.TryParse(request.BrowserType, out BrowserTypes browser);
                        PortalAliasInfo portalAlias = new PortalAliasInfo()
                        {
                            PortalID = pid,
                            HTTPAlias = strAlias,
                            Skin = request.Skin,
                            CultureCode = request.CultureCode,
                            BrowserType = browser,
                            IsPrimary = request.IsPrimary
                        };
                        PortalAliasController.Instance.AddPortalAlias(portalAlias);
                        actionResult.Data = new { SiteAliases = Managers.DomainManager.GetSiteAliases(PortalSettings.Current.PortalId, UserInfo).Data };
                    }
                }
                else
                {
                    actionResult.AddError(HttpStatusCode.BadRequest.ToString(), string.Format(Localization.GetString("InvalidAlias", Dnn.PersonaBar.SiteSettings.Components.Constants.Constants.LocalResourcesFile)));
                }
            }
            catch (Exception exc)
            {
                actionResult.AddError(HttpStatusCode.InternalServerError.ToString(), exc.Message);
            }
            return actionResult;
        }

        [HttpPost]
        [RequireHost]
        [ValidateAntiForgeryToken]
        public ActionResult UpdateSiteAlias(UpdateSiteAliasRequest request)
        {
            ActionResult actionResult = new ActionResult();
            try
            {
                int pid = request.PortalId ?? PortalSettings.Current.PortalId;
                if (!UserInfo.IsSuperUser && pid != PortalSettings.Current.PortalId)
                {
                    actionResult.AddError(HttpStatusCode.Unauthorized.ToString(), Components.Constants.AuthFailureMessage);
                }

                string strAlias = request.HTTPAlias;
                if (!string.IsNullOrEmpty(strAlias))
                {
                    strAlias = strAlias.Trim();
                }

                if (IsHttpAliasValid(strAlias))
                {
                    Enum.TryParse(request.BrowserType, out BrowserTypes browser);
                    if (request.PortalAliasID != null && actionResult.IsSuccess)
                    {
                        PortalAliasInfo portalAlias = new PortalAliasInfo()
                        {
                            PortalID = pid,
                            PortalAliasID = request.PortalAliasID.Value,
                            HTTPAlias = strAlias,
                            Skin = request.Skin,
                            CultureCode = request.CultureCode,
                            BrowserType = browser,
                            IsPrimary = request.IsPrimary
                        };

                        PortalAliasController.Instance.UpdatePortalAlias(portalAlias);
                        actionResult.Data = new { SiteAliases = Managers.DomainManager.GetSiteAliases(PortalSettings.Current.PortalId, UserInfo).Data };
                    }
                    else
                    {
                        actionResult.AddError(HttpStatusCode.BadRequest.ToString(), string.Format(Localization.GetString("InvalidAlias", Dnn.PersonaBar.SiteSettings.Components.Constants.Constants.LocalResourcesFile)));
                    }
                }
                else
                {
                    actionResult.AddError(HttpStatusCode.BadRequest.ToString(), string.Format(Localization.GetString("InvalidAlias", Dnn.PersonaBar.SiteSettings.Components.Constants.Constants.LocalResourcesFile)));
                }
            }
            catch (Exception exc)
            {
                actionResult.AddError(HttpStatusCode.InternalServerError.ToString(), exc.Message);
            }
            return actionResult;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteSiteAlias(int portalAliasId)
        {
            ActionResult actionResult = new ActionResult();
            try
            {
                PortalAliasInfo portalAlias = PortalAliasController.Instance.GetPortalAliasByPortalAliasID(portalAliasId);
                if (!UserInfo.IsSuperUser && portalAlias.PortalID != PortalSettings.Current.PortalId)
                {
                    actionResult.AddError(HttpStatusCode.Unauthorized.ToString(), Components.Constants.AuthFailureMessage);
                }

                if (actionResult.IsSuccess)
                {
                    PortalAliasController.Instance.DeletePortalAlias(portalAlias);
                    actionResult.Data = new { SiteAliases = Managers.DomainManager.GetSiteAliases(PortalSettings.Current.PortalId, UserInfo).Data };
                }
            }
            catch (Exception exc)
            {
                actionResult.AddError(HttpStatusCode.InternalServerError.ToString(), exc.Message);
            }
            return actionResult;
        }
        private bool IsHttpAliasValid(string strAlias)
        {
            bool isValid = true;
            if (string.IsNullOrEmpty(strAlias))
            {
                isValid = false;
            }
            else
            {
                if (strAlias.IndexOf("://", StringComparison.Ordinal) != -1)
                {
                    strAlias = strAlias.Remove(0, strAlias.IndexOf("://", StringComparison.Ordinal) + 3);
                }
                if (strAlias.IndexOf("\\\\", StringComparison.Ordinal) != -1)
                {
                    strAlias = strAlias.Remove(0, strAlias.IndexOf("\\\\", StringComparison.Ordinal) + 2);
                }

                //Validate Alias, this needs to be done with lowercase, downstream we only check with lowercase variables
                if (!PortalAliasController.ValidateAlias(strAlias.ToLowerInvariant(), false))
                {
                    isValid = false;
                }
            }
            return isValid;
        }
        public override string AccessRoles()
        {
            return AppFactory.GetAccessRoles(UserInfo);
        }
    }
}