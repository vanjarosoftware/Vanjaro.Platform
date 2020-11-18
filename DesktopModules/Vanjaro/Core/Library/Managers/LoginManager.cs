using DotNetNuke.Abstractions;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Modules.Definitions;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using DotNetNuke.Security;
using DotNetNuke.Security.Membership;
using DotNetNuke.Services.Authentication;
using DotNetNuke.Services.Exceptions;
using DotNetNuke.Services.UserRequest;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Web;
using Vanjaro.Common.Utilities;
using Vanjaro.Core.Data.Entities;
using Vanjaro.Core.Services.Authentication.OAuth;
using static Vanjaro.Core.Factories;

namespace Vanjaro.Core
{
    public static partial class Managers
    {
        public class LoginManager
        {
            
            public static UserAuthenticatedEventArgs UserLogin(dynamic userLogin)
            {
                string IPAddress = UserRequestIPAddressController.Instance.GetUserRequestIPAddress(new HttpRequestWrapper(HttpContext.Current.Request));
                UserLoginStatus loginStatus = UserLoginStatus.LOGIN_FAILURE;
                string userName = PortalSecurity.Instance.InputFilter(userLogin.Username,
                                        PortalSecurity.FilterFlag.NoScripting |
                                        PortalSecurity.FilterFlag.NoAngleBrackets |
                                        PortalSecurity.FilterFlag.NoMarkup);
                //check if we use email address here rather than username
                //UserInfo userByEmail = null;
                //bool emailUsedAsUsername = PortalController.GetPortalSettingAsBoolean("Registration_UseEmailAsUserName", PortalSettings.Current.PortalId, false);

                //if (emailUsedAsUsername)
                //{
                //    // one additonal call to db to see if an account with that email actually exists
                //    userByEmail = UserController.GetUserByEmail(PortalSettings.Current.PortalId, userName);

                //    if (userByEmail != null)
                //    {
                //        //we need the username of the account in order to authenticate in the next step
                //        userName = userByEmail.Username;
                //    }
                //}


                //if (!emailUsedAsUsername || userByEmail != null)
                //{
                UserInfo objUser = UserController.ValidateUser(PortalSettings.Current.PortalId, userName, userLogin.Password, "DNN", string.Empty, PortalSettings.Current.PortalName, IPAddress, ref loginStatus);
                //}

                bool authenticated = Null.NullBoolean;
                string message = Null.NullString;
                if (loginStatus == UserLoginStatus.LOGIN_USERNOTAPPROVED)
                {
                    message = "UserNotAuthorized";
                }
                else
                {
                    authenticated = (loginStatus != UserLoginStatus.LOGIN_FAILURE);
                }

                //if (objUser != null && loginStatus != UserLoginStatus.LOGIN_FAILURE && emailUsedAsUsername)
                //{
                //    //make sure internal username matches current e-mail address
                //    if (objUser.Username.ToLower() != objUser.Email.ToLower())
                //    {
                //        UserController.ChangeUsername(objUser.UserID, objUser.Email);
                //        userName = objUser.Username = objUser.Email;
                //    }
                //}

                //Raise UserAuthenticated Event
                UserAuthenticatedEventArgs eventArgs = new UserAuthenticatedEventArgs(objUser, userName, loginStatus, "DNN")
                {
                    Authenticated = authenticated,
                    Message = message,
                    RememberMe = userLogin.Remember
                };

                //if (loginStatus == UserLoginStatus.LOGIN_SUCCESS || loginStatus == UserLoginStatus.LOGIN_SUPERUSER)
                //{
                //    UserController.UserLogin(PortalSettings.Current.PortalId, objUser, PortalSettings.Current.PortalName, IPAddress, false);
                //}

                return eventArgs;
            }
            public static void AddUpdateLoginModule(int TabID, int PortalId)
            {
                try
                {
                    using (VanjaroRepo vrepo = new VanjaroRepo())
                    {
                        LoginModule loginModule = null;
                        ModuleDefinitionInfo moduleDefinitionInfo = ModuleDefinitionController.GetModuleDefinitionByFriendlyName(Components.Constants.AccountLogin);
                        System.Collections.Generic.List<LoginModule> moduleInfo = vrepo.Fetch<LoginModule>("Select T.PortalID,Tabm.TabID,Tabm.ModuleID,Tabm.PaneName,Tabm.IsDeleted from TabModules Tabm inner join Tabs T On Tabm.TabID = t.TabID Where PortalID=@0 AND ModuleTitle= @1", PortalId, Components.Constants.AccountLogin);
                        foreach (LoginModule d in moduleInfo)
                        {
                            loginModule = d;
                        }

                        if (loginModule == null)
                        {
                            ModuleInfo module = new ModuleInfo
                            {
                                PortalID = PortalId,
                                TabID = TabID,
                                ModuleOrder = 1,
                                ModuleTitle = moduleDefinitionInfo.FriendlyName,
                                PaneName = "ContentPane",
                                ModuleDefID = moduleDefinitionInfo.ModuleDefID,
                                CacheTime = moduleDefinitionInfo.DefaultCacheTime,
                                InheritViewPermissions = true,
                                AllTabs = false
                            };
                            int ModuleID = ModuleController.Instance.AddModule(module);
                            ModuleController.Instance.DeleteTabModule(TabID, ModuleID, true);
                        }
                        else
                        {
                            ModuleController.Instance.MoveModule(loginModule.ModuleID, loginModule.TabID, TabID, loginModule.PaneName);
                            if (!loginModule.IsDeleted)
                            {
                                ModuleController.Instance.DeleteTabModule(TabID, loginModule.ModuleID, true);
                            }
                        }
                    }
                }
                catch (Exception exc) { Exceptions.LogException(exc); }
            }

            public static string Logoff()
            {
                
                return ServiceProvider.NavigationManager.NavigateURL(PortalSettings.Current.ActiveTab.TabID, "Logoff");
            }

            private class LoginModule
            {
                public int PortalID { get; set; }
                public int TabID { get; set; }
                public int ModuleID { get; set; }
                public string PaneName { get; set; }
                public bool IsDeleted { get; set; }
            }

            public static List<IOAuthClient> GetOAuthClients()
            {
                List<IOAuthClient> OAuthClients = CacheFactory.Get(CacheFactory.Keys.OAuthClients + "-" + PortalSettings.Current.PortalId);

                if (OAuthClients == null)
                {
                    OAuthClients = new List<IOAuthClient>();
                    string[] binAssemblies = Directory.GetFiles(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "bin")).Where(c => c.EndsWith(".dll")).ToArray();

                    foreach (string Path in binAssemblies)
                    {
                        try
                        {
                            //get all assemblies
                            IEnumerable<IOAuthClient> AssembliesToAdd = from t in System.Reflection.Assembly.LoadFrom(Path).GetTypes()
                                                                        where t != (typeof(IOAuthClient)) && (typeof(IOAuthClient).IsAssignableFrom(t))
                                                                        select Activator.CreateInstance(t) as IOAuthClient;
                            OAuthClients.AddRange(AssembliesToAdd.ToList<IOAuthClient>());
                        }
                        catch { continue; }
                    }
                    
                    CacheFactory.Set(CacheFactory.Keys.OAuthClients + "-" + PortalSettings.Current.PortalId, OAuthClients);
                }

                return OAuthClients;
            }
        }
    }
}