using DotNetNuke.Web.Api;

namespace Vanjaro.UXManager.Library.Entities
{
    public class DnnAdminAttribute : AuthorizeAttributeBase, IOverrideDefaultAuthLevel
    {
        public override bool IsAuthorized(AuthFilterContext context)
        {
            if (context != null)
            {
                DotNetNuke.Entities.Users.UserInfo UserInfo = ((DnnApiController)context.ActionContext.ControllerContext.Controller).UserInfo;
                if (UserInfo != null)
                {
                    return UserInfo.IsInRole("Administrators");
                }
            }
            return false;
        }
    }
}