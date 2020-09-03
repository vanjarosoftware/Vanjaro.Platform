using DotNetNuke.Entities.Modules;
using DotNetNuke.Web.Api;

namespace Vanjaro.Common.ASPNET.WebAPI
{
    public class WebApiController : DnnApiController
    {
        public virtual string AllowedAccessRoles(string Identifier)
        {
            return string.Empty;
        }

        public virtual string AccessRoles()
        {
            return string.Empty;
        }

        public ModuleInfo ModuleInfo()
        {
            return ActiveModule;
        }
    }
}