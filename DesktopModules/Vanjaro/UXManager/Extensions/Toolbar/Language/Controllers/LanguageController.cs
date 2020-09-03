using System.Linq;
using DotNetNuke.Web.Api;
using System.Collections.Generic;
using Mandeeps.DNN.Libraries.Common.Engines.UIEngine;


namespace Vanjaro.UXManager.Extensions.Toolbar.Language.Controllers
{
    public class LanguageController : DnnApiController
    {
        public static List<IUIData> GetData(string Identifier, Dictionary<string, string> UIEngineInfo)
        {
            Dictionary<string, IUIData> Settings = new Dictionary<string, IUIData>();
            return Settings.Values.ToList();
        }
    }
}