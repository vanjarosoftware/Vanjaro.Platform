using DotNetNuke.Web.Api;
using System.Collections.Generic;
using System.Linq;
using Vanjaro.Common.Engines.UIEngine;


namespace Vanjaro.UXManager.Extensions.Toolbar.Navigator.Controllers
{
    public class NavigatorController : DnnApiController
    {
        public static List<IUIData> GetData(string Identifier, Dictionary<string, string> UIEngineInfo)
        {
            Dictionary<string, IUIData> Settings = new Dictionary<string, IUIData>();
            return Settings.Values.ToList();
        }
    }
}