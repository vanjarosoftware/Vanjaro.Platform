using DotNetNuke.Web.Api;
using System.Collections.Generic;
using System.Linq;
using Vanjaro.Common.Engines.UIEngine;


namespace Vanjaro.UXManager.Extensions.Toolbar.DeviceMode.Controllers
{
    public class DeviceModeController : DnnApiController
    {
        public static List<IUIData> GetData(string Identifier, Dictionary<string, string> UIEngineInfo)
        {
            Dictionary<string, IUIData> Settings = new Dictionary<string, IUIData>();
            return Settings.Values.ToList();
        }
    }
}