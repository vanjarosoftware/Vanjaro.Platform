using DotNetNuke.Entities.Portals;
using DotNetNuke.Security.Permissions;
using DotNetNuke.Web.Api;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Web;
using System.Web.Http.Controllers;

namespace Vanjaro.Common.ASPNET.WebAPI
{
    public class AuthorizeAccessRolesAttribute : AuthorizeAttributeBase, IOverrideDefaultAuthLevel
    {
        public string AccessRoles { get; set; }

        public override bool IsAuthorized(AuthFilterContext context)
        {
            string AllowedAccessRoles = AccessRoles;

            dynamic Controller = context.ActionContext.ControllerContext.Controller;

            if (context.ActionContext.ActionDescriptor.ActionBinding.ActionDescriptor != null)
            {
                ReflectedHttpActionDescriptor reflectedHttpActionDescriptor = context.ActionContext.ActionDescriptor.ActionBinding.ActionDescriptor as ReflectedHttpActionDescriptor;

                if (reflectedHttpActionDescriptor != null && reflectedHttpActionDescriptor.MethodInfo != null && reflectedHttpActionDescriptor.MethodInfo.CustomAttributes != null)
                {
                    System.Reflection.CustomAttributeData MethodAuthorizeAccessRolesAttribute = reflectedHttpActionDescriptor.MethodInfo.CustomAttributes.Where(a => a.AttributeType.Name == "AuthorizeAccessRolesAttribute").FirstOrDefault();

                    if (MethodAuthorizeAccessRolesAttribute != null)
                    {
                        AllowedAccessRoles = MethodAuthorizeAccessRolesAttribute.NamedArguments[0].TypedValue.Value.ToString();
                    }
                }
            }

            try
            {
                //There are no explictly specifiled roles in attribute
                if (string.IsNullOrEmpty(AllowedAccessRoles))
                {
                    NameValueCollection QueryString = HttpUtility.ParseQueryString(context.ActionContext.Request.RequestUri.Query);

                    if (PortalSettings.Current.UserInfo.IsSuperUser)
                    {
                        return true;
                    }

                    if (!string.IsNullOrEmpty(QueryString["identifier"]) && QueryString["identifier"] == "common_controls_editorconfig" && (PortalSettings.Current.UserInfo.IsInRole("Administrators") || ModulePermissionController.CanEditModuleContent(Controller.ModuleInfo())))
                    {
                        return true;
                    }

                    if (QueryString != null && !string.IsNullOrEmpty(QueryString["identifier"]))
                    {
                        AllowedAccessRoles = Controller.AllowedAccessRoles(QueryString["identifier"]);
                    }

                    if (!string.IsNullOrEmpty(AllowedAccessRoles) && !string.IsNullOrEmpty(context.ActionContext.ActionDescriptor.ActionName) && context.ActionContext.ActionDescriptor.ActionName.ToLower() == "updatedata")
                    {
                        List<string> AllowedAccessRolesRolesArray = AllowedAccessRoles.Split(',').Distinct().ToList();
                        AllowedAccessRolesRolesArray.Remove("user");
                        AllowedAccessRolesRolesArray.Remove("anonymous");
                        AllowedAccessRoles = string.Join(",", AllowedAccessRolesRolesArray.Distinct());
                    }
                }

                if (!string.IsNullOrEmpty(AllowedAccessRoles))
                {
                    string InRoles = Controller.AccessRoles();
                    string[] AllowedAccessRolesRolesArray = AllowedAccessRoles.Split(',');

                    if (!string.IsNullOrEmpty(InRoles))
                    {
                        foreach (string role in InRoles.Split(','))
                        {
                            if (AllowedAccessRolesRolesArray.Where(a => a == role).SingleOrDefault() != null)
                            {
                                return true;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                DotNetNuke.Services.Exceptions.Exceptions.LogException(ex);
            }
            return false;
        }
    }
}