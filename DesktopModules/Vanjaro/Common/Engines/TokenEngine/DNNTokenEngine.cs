using DotNetNuke.Services.Tokens;
using System.Threading;
using Vanjaro.Common.Globals;

namespace Vanjaro.Common.Engines.TokenEngine
{
    public class DNNTokenEngine : ITokenEngine
    {
        private readonly DNNContext DNNContext;

        /// <summary>
        /// Initializes the DNNTokenEngine
        /// </summary>
        /// <param name="DNNContext"></param>
        public DNNTokenEngine(DNNContext DNNContext)
        {
            this.DNNContext = DNNContext;
        }

        /// <summary>
        /// Parses the given template for DNN Standard Tokens
        /// </summary>
        /// <param name="Template"></param>
        /// <returns></returns>
        public string Parse(string Template)
        {
            return new TokenReplace(Scope.DefaultSettings, Thread.CurrentThread.CurrentCulture.ToString(), DNNContext.PortalSettings, DNNContext.UserInfo, DNNContext.ModuleInfo.ModuleID).ReplaceEnvironmentTokens(Template);
        }
    }
}