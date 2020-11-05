using DotNetNuke.Common;
using DotNetNuke.Web.Api;
using Vanjaro.Common.ASPNET.WebAPI;
using Vanjaro.Common.Engines.UIEngine;
using Vanjaro.URL.Data.Entities;
using Vanjaro.URL.Factories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;

namespace Vanjaro.URL.Controllers
{
    [AuthorizeAccessRoles(AccessRoles = "admin")]
    public class UrlController : WebApiController
    {
        public static List<IUIData> GetData(int PortalID, int ModuleID, string Identifier, Dictionary<string, string> UIEngineInfo, Dictionary<string, string> Parameters)
        {
            Dictionary<string, IUIData> Settings = new Dictionary<string, IUIData>();
            List<URLEntity> Urls = new List<URLEntity>();
            if (Parameters.ContainsKey("eid")&& Parameters.ContainsKey("ename"))
            {
                Urls = URLFactory.GetUrlHistory(ModuleID, int.Parse(Parameters["eid"]), Parameters["ename"]).OrderByDescending(o => o.URLID).ToList();
            }
            Settings.Add("UrlHistory", new UIData { Name = "UrlHistory", Options = Urls });

            return Settings.Values.ToList();
        }

        public override string AccessRoles()
        {
            return AppFactory.GetAccessRoles(this.ActiveModule, this.UserInfo);
        }
    }
}