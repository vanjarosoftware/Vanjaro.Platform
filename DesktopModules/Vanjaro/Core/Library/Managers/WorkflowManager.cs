using Dnn.PersonaBar.Pages.Components;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Tabs;
using DotNetNuke.Entities.Users;
using DotNetNuke.Security;
using DotNetNuke.Security.Permissions;
using DotNetNuke.Security.Roles;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Web;
using Vanjaro.Common.Permissions;
using Vanjaro.Core.Components;
using Vanjaro.Core.Components.Interfaces;
using Vanjaro.Core.Data.Entities;
using static Vanjaro.Core.Components.Enum;
using static Vanjaro.Core.Factories;
using DNNLocalization = DotNetNuke.Services.Localization;

namespace Vanjaro.Core
{
    public static partial class Managers
    {
        public class WorkflowManager
        {
            public static List<StringText> GetDDLWorkflow(int PortalID, bool IncludeDeleted)
            {
                List<StringText> DDLList = new List<StringText>();
                foreach (Workflow w in GetAll(PortalID, IncludeDeleted).ToList())
                {
                    StringText st = new StringText
                    {
                        Text = w.Name,
                        Value = w.ID.ToString(),
                        Content = GetWorkflowStatesInfo(w.ID).ToString()
                    };
                    DDLList.Add(st);
                }
                return DDLList;
            }

            public static List<Workflow> GetAll(int PortalID, bool IncludeDeleted)
            {
                return WorkflowFactory.GetAll(PortalID, IncludeDeleted);
            }

            public static string GetWorkflowStatesInfo(int workflowId = -1)
            {
                string data = string.Empty;

                Workflow wf = null;
                if (workflowId > -1)
                {
                    wf = WorkflowFactory.GetWorkflowbyID(workflowId);
                }

                if (wf != null)
                {
                    StringBuilder sb = new StringBuilder();
                    int Couter = 0;
                    List<WorkflowState> State = WorkflowFactory.GetAllStatesbyWorkflowID(workflowId).Where(a => a.IsActive == true).OrderBy(a => a.Order).ToList();
                    foreach (WorkflowState item in State)
                    {

                        Couter++;
                        sb.Append(item.Name);
                        if (State.Count != Couter)
                        {
                            sb.Append(" >> ");
                        }
                    }

                    data = sb.ToString() + "<br /> <br style=\"line-height:8px\" /><p> " + wf.Description + "</p>";
                }
                else
                {
                    data = "Published";
                }

                return data;
            }

            public static Workflow GetWorkflow(int ID)
            {
                return WorkflowFactory.GetWorkflowbyID(ID);
            }

            public static List<WorkflowState> GetAllWorkflowStates(int WorkflowID)
            {
                return WorkflowFactory.GetAllStatesbyWorkflowID(WorkflowID);
            }

            public static List<WorkflowState> GetWorkflowStates(int WorkflowID)
            {
                List<WorkflowState> WorkflowState = new List<WorkflowState>();
                foreach (WorkflowState State in GetAllWorkflowStates(WorkflowID))
                {
                    WorkflowState.Add(new WorkflowState { Name = State.Name, WorkflowID = State.WorkflowID, StateID = State.StateID, IsActive = State.IsActive, Notify = State.Notify, IsDeleted = IsWorkflowStateDeleted(WorkflowID, State), Order = State.Order });
                }

                return WorkflowState.OrderBy(a => a.Order).ToList();
            }

            public static WorkflowState GetStateByID(int ID)
            {
                return WorkflowFactory.GetStateByID(ID);
            }

            public static List<GenericPermissionInfo> GetGenericPermissions(List<WorkflowStatePermission> Permissions)
            {
                List<GenericPermissionInfo> GenericPermissions = new List<GenericPermissionInfo>();
                foreach (WorkflowStatePermission wp in Permissions)
                {
                    GenericPermissionInfo gp = new GenericPermissionInfo
                    {
                        AllowAccess = wp.AllowAccess,
                        RoleID = wp.RoleID.HasValue ? wp.RoleID.Value : -4,
                        UserID = wp.UserID.HasValue ? wp.UserID.Value : -1,
                        PermissionID = wp.PermissionID
                    };
                    if (gp.UserID > -1)
                    {
                        UserInfo UserInfo = UserController.GetUserById(PortalSettings.Current.PortalId, gp.UserID);
                        if (UserInfo != null)
                        {
                            gp.DisplayName = UserInfo.DisplayName;
                        }
                    }
                    GenericPermissions.Add(gp);
                }
                return GenericPermissions;
            }

            public static List<WorkflowStatePermission> GetStatePermissionByStateID(int ID)
            {
                return WorkflowFactory.GetStatePermissionsByStateID(ID);
            }

            public static Permissions GetStatePermission(int StateId)
            {
                UserInfo UserInfo = UserController.Instance.GetCurrentUserInfo();
                Permissions Permissions = new Permissions(true, false);
                DNNModulePermissionInfo DnnPermissionInfo = Vanjaro.Common.Manager.PermissionManager.GetPermissionInfo(PageWorkflowPermission.PERMISSION_CODE).Where(p => p.PermissionKey == PageWorkflowPermission.PERMISSION_REVIEWCONTENT).FirstOrDefault();
                if (DnnPermissionInfo != null)
                {
                    WorkflowState wState = GetStateByID(StateId);
                    if (wState != null)
                    {
                        foreach (GenericPermissionInfo perm in GetGenericPermissions(GetStatePermissionByStateID(StateId)))
                        {
                            foreach (WorkflowPermissionInfo p in GetPermissionByCode(PageWorkflowPermission.PERMISSION_CODE))
                            {
                                if (p.PermissionID == perm.PermissionID)
                                {
                                    perm.PermissionName = p.PermissionKey;
                                }

                            }

                            if (perm.UserID == -1 && perm.RoleID != -4)
                            {
                                if (perm.RoleID != -1)
                                {
                                    perm.RoleName = RoleController.Instance.GetRoleById(UserInfo.PortalID, perm.RoleID).RoleName;
                                }

                                Vanjaro.Common.Manager.PermissionManager.AddRolePermission(Permissions, perm);
                            }
                            else if (perm.UserID != -1 && perm.RoleID == -4)
                            {
                                Vanjaro.Common.Manager.PermissionManager.AddUserPermission(Permissions, perm);
                            }

                        }
                    }

                    GenericPermissionInfo Permission = new GenericPermissionInfo();
                    RoleInfo AdministratorRole = RoleController.Instance.GetRoleById(UserInfo.PortalID, DotNetNuke.Entities.Portals.PortalSettings.Current.AdministratorRoleId);
                    Permission.RoleID = AdministratorRole.RoleID;
                    Permission.RoleName = AdministratorRole.RoleName;
                    Permission.PermissionName = DnnPermissionInfo.PermissionKey;
                    Permission.PermissionID = DnnPermissionInfo.PermissionID;
                    Permission.AllowAccess = true;

                    Vanjaro.Common.Manager.PermissionManager.AddRolePermission(Permissions, Permission);
                }
                return Permissions;
            }

            public static List<WorkflowPermissionInfo> GetPermissionByCode(string Code)
            {
                return WorkflowFactory.GetPermissionByCode(Code);
            }

            public static Dictionary<string, dynamic> GetPermission()
            {
                Dictionary<string, dynamic> permData = new Dictionary<string, dynamic>();
                Permissions Permissions = new Permissions();

                List<Permission> PermissionDefinitions = new List<Permission>();

                foreach (DNNModulePermissionInfo p in Vanjaro.Common.Manager.PermissionManager.GetPermissionInfo(PageWorkflowPermission.PERMISSION_CODE).Where(p => p.PermissionKey == PageWorkflowPermission.PERMISSION_REVIEWCONTENT))
                {
                    PermissionDefinitions.Add(AddPermissionDefinitions(p.PermissionKey, p.PermissionID));
                }

                Permissions _permission = GetStatePermission(0);
                _permission.PermissionDefinitions = PermissionDefinitions;
                Permissions = _permission;
                Permissions.Inherit = false;
                Permissions.ShowInheritCheckBox = false;
                Permissions.InheritPermissionID = _permission.InheritPermissionID;
                permData.Add("workflowPermission", Permissions);
                return permData;
            }

            private static Permission AddPermissionDefinitions(string PermissionKey, int PermissionID)
            {
                Permission Permission = new Permission
                {
                    AllowAccess = true,
                    PermissionName = PermissionKey,
                    PermissionId = PermissionID
                };
                return Permission;
            }

            public static Workflow Update(int WorkflowID, string Name, string Description, int Revisions, bool IsDeleted, int UserId, int? PortalID)
            {
                Workflow wflow = new Workflow
                {
                    ID = WorkflowID,
                    Description = Description,
                    IsDeleted = IsDeleted,
                    PortalID = PortalID,
                    Revisions = Revisions
                };
                if (IsDeleted)
                {
                    wflow.DeletedBy = UserId;
                    wflow.DeletedOn = DateTime.UtcNow;
                }
                else
                {
                    wflow.DeletedBy = null;
                    wflow.DeletedOn = null;
                }
                wflow.Name = Name;
                wflow.CreatedBy = UserId;
                wflow.CreatedOn = DateTime.UtcNow;
                return WorkflowFactory.UpdateWorkflow(wflow);
            }

            public static bool Delete(int WorkflowID, bool IsDeleted, int UserId)
            {
                if (!IsWorkflowUsing(WorkflowID))
                {
                    Workflow wflow = GetWorkflow(WorkflowID);
                    wflow.IsDeleted = IsDeleted;
                    if (IsDeleted)
                    {
                        wflow.DeletedBy = UserId;
                        wflow.DeletedOn = DateTime.UtcNow;
                    }
                    else
                    {
                        wflow.DeletedBy = null;
                        wflow.DeletedOn = null;
                    }
                    WorkflowFactory.DeleteWorkflow(wflow);
                    return true;
                }
                else
                {
                    return false;
                }
            }

            public static bool IsWorkflowUsing(int WorkflowID)
            {
                bool IsUsing = false;
                Setting setting = SettingFactory.GetSetting(PortalController.Instance.GetCurrentSettings().PortalId, 0, "setting_workflow", "WorkflowID");
                if (setting != null && (int.Parse(setting.Value) == WorkflowID))
                {
                    return true;
                }

                List<WorkflowState> States = GetWorkflowStates(WorkflowID);
                foreach (WorkflowState State in States)
                {
                    if (PageManager.GetAllByState(State.StateID).Count > 0)
                    {
                        IsUsing = true;
                        break;
                    }
                }
                return IsUsing;
            }

            public static List<WorkflowStatePermission> GetStatePermissions(int StateID, List<GenericPermissionInfo> GenericPermissions)
            {
                List<WorkflowStatePermission> Permissions = new List<WorkflowStatePermission>();
                foreach (GenericPermissionInfo gp in GenericPermissions)
                {
                    if (gp.RoleID != -4 || gp.UserID != -4)
                    {
                        WorkflowStatePermission ws = new WorkflowStatePermission
                        {
                            AllowAccess = gp.AllowAccess,
                            StateID = StateID,
                            CreatedOn = gp.CreatedOnDate,
                            RoleID = gp.RoleID,
                            UpdatedBy = gp.LastModifiedByUserID,
                            PermissionID = gp.PermissionID
                        };

                        if (gp.CreatedOnDate == DateTime.MinValue)
                        {
                            ws.CreatedOn = DateTime.Now;
                        }
                        else
                        {
                            ws.CreatedBy = gp.CreatedByUserID;
                        }

                        if (gp.LastModifiedOnDate == DateTime.MinValue)
                        {
                            ws.UpdatedOn = null;
                        }
                        else
                        {
                            ws.UpdatedOn = gp.LastModifiedOnDate;
                        }

                        if (gp.UserID < 0)
                        {
                            ws.UserID = null;
                        }
                        else
                        {
                            ws.RoleID = null;
                            ws.UserID = gp.UserID;
                        }
                        Permissions.Add(ws);

                    }
                }
                return Permissions;
            }

            internal static bool IsExists(int WorkflowId, string State)
            {
                if (WorkflowFactory.GetAllStatesbyWorkflowID(WorkflowId).Where(w => w.Name == State && w.WorkflowID == WorkflowId).FirstOrDefault() == null)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }

            public static bool IsWorkflowStateDeleted(int WorkflowID, WorkflowState workflowstate)
            {
                if (IsFirstState(WorkflowID, workflowstate.StateID) || IsLastState(WorkflowID, workflowstate.StateID))
                {
                    return false;
                }
                else
                {
                    if (!(IsExists(workflowstate.WorkflowID, workflowstate.StateID.ToString())))
                    {
                        int Content = PageManager.GetAllByState(workflowstate.StateID).Count;
                        if (Content > 0)
                        {
                            return false;
                        }
                        else
                        {
                            return true;
                        }
                    }
                }
                return false;
            }


            public static void UpdateWorkflowPermissions(dynamic Data)
            {
                List<GenericPermissionInfo> Permissions = new List<GenericPermissionInfo>();
                foreach (dynamic item in Data.PermissionsRoles)
                {
                    string PermissionID = "";
                    foreach (dynamic p in item.Permissions)
                    {
                        if (bool.Parse(p.AllowAccess.ToString()))
                        {
                            PermissionID = p.PermissionId.ToString();
                            if (!string.IsNullOrEmpty(PermissionID))
                            {
                                Permissions.Add(new GenericPermissionInfo { AllowAccess = true, RoleID = int.Parse(item.RoleId.ToString()), PermissionID = int.Parse(PermissionID) });
                            }
                        }
                    }

                }
                foreach (dynamic item in Data.PermissionsUsers)
                {
                    string PermissionID = "";
                    foreach (dynamic p in item.Permissions)
                    {
                        if (bool.Parse(p.AllowAccess.ToString()))
                        {
                            PermissionID = p.PermissionId.ToString();
                            if (!string.IsNullOrEmpty(PermissionID))
                            {
                                Permissions.Add(new GenericPermissionInfo { AllowAccess = true, UserID = int.Parse(item.UserId.ToString()), PermissionID = int.Parse(PermissionID) });
                            }
                        }
                    }

                }
                int WorkflowID = int.Parse(Data.Workflow.ID.ToString());
                WorkflowFactory.ClearAllWorkflowPermissionsID(WorkflowID);
                WorkflowFactory.UpdateWorkflowPermissions(GetWorkflowPermissions(WorkflowID, Permissions));
            }

            public static List<WorkflowPermission> GetWorkflowPermissions(int WorkflowID, List<GenericPermissionInfo> GenericPermissions)
            {
                List<WorkflowPermission> Permissions = new List<WorkflowPermission>();
                foreach (GenericPermissionInfo gp in GenericPermissions)
                {
                    if (gp.RoleID != -4 || gp.UserID != -4)
                    {
                        WorkflowPermission ws = new WorkflowPermission
                        {
                            AllowAccess = gp.AllowAccess,
                            WorkflowID = WorkflowID,
                            CreatedOn = gp.CreatedOnDate,
                            RoleID = gp.RoleID,
                            UpdatedBy = gp.LastModifiedByUserID,
                            PermissionID = gp.PermissionID
                        };

                        if (gp.CreatedOnDate == DateTime.MinValue)
                        {
                            ws.CreatedOn = DateTime.Now;
                        }
                        else
                        {
                            ws.CreatedBy = gp.CreatedByUserID;
                        }

                        if (gp.LastModifiedOnDate == DateTime.MinValue)
                        {
                            ws.UpdatedOn = null;
                        }
                        else
                        {
                            ws.UpdatedOn = gp.LastModifiedOnDate;
                        }

                        if (gp.UserID < 0)
                        {
                            ws.UserID = null;
                        }
                        else
                        {
                            ws.RoleID = null;
                            ws.UserID = gp.UserID;
                        }
                        Permissions.Add(ws);

                    }
                }
                return Permissions;
            }

            public static List<WorkflowState> UpdateState(int WorkflowID, dynamic Data)
            {
                Workflow Workflow = GetWorkflow(WorkflowID);
                int StateID = int.Parse(Data.StateID.ToString());
                WorkflowState wState = GetStateByID(StateID);
                List<WorkflowState> wStates = WorkflowFactory.GetAllStatesbyWorkflowID(WorkflowID).OrderBy(o => o.Order).ToList();
                bool IsNew = false;
                if (wState == null)
                {
                    wState = new WorkflowState
                    {
                        WorkflowID = WorkflowID
                    };
                    IsNew = true;
                }

                wState.Name = Data.Name.ToString();
                wState.Notify = bool.Parse(Data.Notify.ToString());
                wState.IsActive = bool.Parse(Data.IsActive.ToString());

                if (IsNew)
                {
                    wState.Order = WorkflowFactory.UpdateSatesOrder(wStates, Workflow, true, null);
                }

                WorkflowFactory.UpdateWorkflowState(wState);

                if ((IsNew && wStates.Count > 2) || (wStates.Count > 2 && !IsFirstState(WorkflowID, wState.StateID) && !IsLastState(WorkflowID, wState.StateID)))
                {
                    WorkflowFactory.ClearAllWorkflowStatePermissionsByStateID(StateID);

                    List<GenericPermissionInfo> Permissions = new List<GenericPermissionInfo>();
                    foreach (dynamic item in Data.RolePermissions)
                    {
                        string PermissionID = "";
                        foreach (dynamic p in item.Permissions)
                        {
                            if (p.PermissionName.ToString() == PageWorkflowPermission.PERMISSION_REVIEWCONTENT && bool.Parse(p.AllowAccess.ToString()))
                            {
                                PermissionID = p.PermissionId.ToString();
                                break;
                            }
                        }
                        if (!string.IsNullOrEmpty(PermissionID))
                        {
                            Permissions.Add(new GenericPermissionInfo { AllowAccess = true, RoleID = int.Parse(item.RoleId.ToString()), PermissionID = int.Parse(PermissionID) });
                        }
                    }
                    foreach (dynamic item in Data.UserPermissions)
                    {
                        string PermissionID = "";
                        foreach (dynamic p in item.Permissions)
                        {
                            if (p.PermissionName.ToString() == PageWorkflowPermission.PERMISSION_REVIEWCONTENT && bool.Parse(p.AllowAccess.ToString()))
                            {
                                PermissionID = p.PermissionId.ToString();
                                break;
                            }
                        }
                        if (!string.IsNullOrEmpty(PermissionID))
                        {
                            Permissions.Add(new GenericPermissionInfo { AllowAccess = true, UserID = int.Parse(item.UserId.ToString()), PermissionID = int.Parse(PermissionID) });
                        }
                    }
                    WorkflowFactory.UpdateWorkflowStatePermissions(GetStatePermissions(wState.StateID, Permissions));
                }


                return GetWorkflowStates(WorkflowID);
            }

            public static List<GenericPermissionInfo> GetGenericPermissions(List<WorkflowPermission> Permissions)
            {
                List<GenericPermissionInfo> GenericPermissions = new List<GenericPermissionInfo>();
                foreach (WorkflowPermission wp in Permissions)
                {
                    GenericPermissionInfo gp = new GenericPermissionInfo
                    {
                        AllowAccess = wp.AllowAccess,
                        RoleID = wp.RoleID.HasValue ? wp.RoleID.Value : -4,
                        UserID = wp.UserID.HasValue ? wp.UserID.Value : -1,
                        PermissionID = wp.PermissionID
                    };
                    if (gp.UserID > -1)
                    {
                        UserInfo UserInfo = UserController.GetUserById(PortalSettings.Current.PortalId, gp.UserID);
                        if (UserInfo != null)
                        {
                            gp.DisplayName = UserInfo.DisplayName;
                        }
                    }
                    GenericPermissions.Add(gp);


                }
                return GenericPermissions;
            }

            public static Permissions GetWorkflowPermission(int WorkflowID)
            {
                List<WorkflowPermission> permissions = WorkflowFactory.GetWorkflowPermissionsByID(WorkflowID);
                return GetWorkflowPermission(WorkflowID, permissions);
            }
            private static Permissions GetWorkflowPermission(int WorkflowID, List<WorkflowPermission> permissions)
            {
                UserInfo UserInfo = UserController.Instance.GetCurrentUserInfo();
                Permissions Permissions = new Permissions();
                List<Permission> PermissionDefinitions = new List<Permission>();
                foreach (DNNModulePermissionInfo p in Vanjaro.Common.Manager.PermissionManager.GetPermissionInfo("SYSTEM_TAB"))
                {
                    PermissionDefinitions.Add(AddPermissionDefinitions(p.PermissionName, p.PermissionID));
                }

                foreach (GenericPermissionInfo perm in GetGenericPermissions(permissions))
                {

                    if (perm.UserID == -1 && perm.RoleID != -4)
                    {
                        if (perm.RoleID != -1)
                        {
                            perm.RoleName = RoleController.Instance.GetRoleById(UserInfo.PortalID, perm.RoleID).RoleName;
                        }
                        else if (perm.RoleID == -1)
                        {
                            perm.RoleName = "All Users";
                        }

                        Vanjaro.Common.Manager.PermissionManager.AddRolePermission(Permissions, perm);
                    }
                    else if (perm.UserID != -1 && perm.RoleID == -4)
                    {
                        Vanjaro.Common.Manager.PermissionManager.AddUserPermission(Permissions, perm);
                    }
                }

                foreach (Permission pinfo in PermissionDefinitions.ToList())
                {
                    GenericPermissionInfo alluserinfo = new GenericPermissionInfo
                    {
                        AllowAccess = false,
                        PermissionID = pinfo.PermissionId,
                        RoleID = -1,
                        RoleName = "All Users"
                    };
                    Vanjaro.Common.Manager.PermissionManager.AddRolePermission(Permissions, alluserinfo);

                    GenericPermissionInfo allreginfo = new GenericPermissionInfo
                    {
                        AllowAccess = false,
                        PermissionID = pinfo.PermissionId,
                        RoleID = 1,
                        RoleName = "Registered Users"
                    };
                    Vanjaro.Common.Manager.PermissionManager.AddRolePermission(Permissions, allreginfo);
                }

                Permissions.PermissionDefinitions = PermissionDefinitions;
                return Permissions;

            }
            public static Dictionary<string, dynamic> GetWorkflowPermission(int WorkflowID, int PortalID)
            {
                Dictionary<string, dynamic> permData = new Dictionary<string, dynamic>();
                int InheritPermissionID = -1;
                List<WorkflowPermission> permissions = WorkflowFactory.GetWorkflowPermissionsByID(WorkflowID);
                if (WorkflowID > 0 && permissions.Count > 0)
                {

                    permData.Add("Permissions", GetWorkflowPermission(WorkflowID, permissions));
                }
                else
                {
                    dynamic data = PagesController.Instance.GetDefaultSettings(0);

                    Permissions Permissions = new Permissions
                    {
                        PermissionDefinitions = new List<Permission>()
                    };
                    foreach (dynamic p in data.Permissions.PermissionDefinitions)
                    {
                        Permission permission = new Permission
                        {
                            PermissionName = p.PermissionName,
                            PermissionId = p.PermissionId,
                            AllowAccess = true
                        };
                        if (permission.PermissionName.ToLower().Replace(" ", "") == "viewtab")
                        {
                            InheritPermissionID = permission.PermissionId;
                        }

                        Permissions.PermissionDefinitions.Add(permission);
                    }
                    foreach (dynamic RolePerm in data.Permissions.RolePermissions)
                    {
                        RolePermission rolepermission = new RolePermission
                        {
                            RoleId = RolePerm.RoleId,
                            RoleName = RolePerm.RoleName,
                            Locked = RolePerm.Locked,
                            IsDefault = RolePerm.IsDefault
                        };
                        foreach (dynamic item in RolePerm.Permissions)
                        {
                            Permission permission = new Permission
                            {
                                PermissionName = item.PermissionName,
                                PermissionId = item.PermissionId,
                                View = item.View,
                                AllowAccess = item.AllowAccess
                            };

                            rolepermission.Permissions.Add(permission);
                        }
                        //Adding default page permission for administrator
                        if (RolePerm.Permissions.Count == 0 && RolePerm.RoleName == "Administrators")
                        {
                            foreach (dynamic p in data.Permissions.PermissionDefinitions)
                            {
                                Permission permission = new Permission
                                {
                                    PermissionName = p.PermissionName,
                                    PermissionId = p.PermissionId,
                                    AllowAccess = true
                                };
                                rolepermission.Permissions.Add(permission);
                            }
                        }
                        Permissions.RolePermissions.Add(rolepermission);
                    }
                    foreach (dynamic UserPerm in data.Permissions.UserPermissions)
                    {
                        UserPermission userpermission = new UserPermission
                        {
                            UserId = UserPerm.UserId
                        };
                        userpermission.AvatarUrl = Vanjaro.Common.Utilities.UserUtils.GetProfileImage(PortalID, userpermission.UserId);
                        UserInfo userInfo = UserController.GetUserById(PortalID, userpermission.UserId);
                        userpermission.Email = userInfo.Email;
                        userpermission.UserName = userInfo.Username;
                        userpermission.DisplayName = UserPerm.DisplayName;

                        foreach (dynamic item in UserPerm.Permissions)
                        {
                            Permission permission = new Permission
                            {
                                PermissionName = item.PermissionName,
                                PermissionId = item.PermissionId,
                                View = item.View,
                                AllowAccess = item.AllowAccess
                            };
                            userpermission.Permissions.Add(permission);
                        }
                        Permissions.UserPermissions.Add(userpermission);
                    }

                    Permissions.Inherit = false;
                    Permissions.ShowInheritCheckBox = false;
                    Permissions.InheritPermissionID = InheritPermissionID;
                    permData.Add("Permissions", Permissions);
                }


                return permData;
            }

            public static void DeleteWorkflowState(int WorkflowStateId, int WorkflowId)
            {
                WorkflowFactory.DeleteWorkflowState(WorkflowStateId, WorkflowId);
            }

            public static List<StringValue> GetAllWorkflowScope(string ResourceFile)
            {
                List<StringValue> WorkflowScope = new List<StringValue>
                {
                    new StringValue { Text = DNNLocalization.Localization.GetString("Global", ResourceFile), Value = "0" },
                    new StringValue { Text = DNNLocalization.Localization.GetString("PortalBase", ResourceFile), Value = "1" }
                };
                return WorkflowScope;
            }

            public static bool IsFirstState(int WorkflowID, int StateID)
            {
                return GetAllWorkflowStates(WorkflowID).Where(a => a.IsActive == true).OrderBy(ws => ws.Order).FirstOrDefault().StateID == StateID;
            }

            public static bool IsLastState(int WorkflowID, int StateID)
            {
                return GetAllWorkflowStates(WorkflowID).Where(a => a.IsActive == true).OrderBy(ws => ws.Order).LastOrDefault().StateID == StateID;
            }

            public static WorkflowState GetFirstStateID(int WorkflowID)
            {
                return GetAllWorkflowStates(WorkflowID).Where(a => a.IsActive == true).OrderBy(ws => ws.Order).FirstOrDefault();
            }

            public static WorkflowState GetLastStateID(int WorkflowID)
            {
                return GetAllWorkflowStates(WorkflowID).Where(a => a.IsActive == true).OrderBy(ws => ws.Order).LastOrDefault();
            }

            public static int GetMaxRevisions()
            {
                return GetMaxRevisions(0);
            }
            public static int GetMaxRevisions(int TabID)
            {
                PortalSettings PS = PortalController.Instance.GetCurrentSettings() as PortalSettings;
                int PortalID = PS != null ? PS.PortalId : 0;
                Setting setting = SettingFactory.GetSetting(PortalID, TabID, "setting_workflow", "MaxRevisions");
                if (setting == null)
                {
                    setting = SettingFactory.GetSetting(PortalID, 0, "setting_workflow", "MaxRevisions");
                    if (setting != null)
                    {
                        return int.Parse(setting.Value);
                    }
                    else
                    {
                        return 5;
                    }
                }
                else
                {
                    return int.Parse(setting.Value);
                }
            }

            public static WorkflowTypes GetWorkflowType(int WorkflowID)
            {
                List<WorkflowState> wsts = GetWorkflowStates(WorkflowID);
                if (wsts.Count == 2)
                {
                    return WorkflowTypes.ContentStaging;
                }

                if (wsts.Count > 2)
                {
                    return WorkflowTypes.ContentApproval;
                }

                return WorkflowTypes.ContentStaging;
            }

            public static bool HasReviewPermission(UserInfo UserInfo)
            {
                return WorkflowFactory.HasReviewPermission(UserInfo);
            }

            public static bool HasReviewPermission(int StateID, UserInfo UserInfo)
            {
                return (CheckPermissionsWithPermissionKey(StateID, PageWorkflowPermission.PERMISSION_REVIEWCONTENT, UserInfo));
            }

            private static bool CheckPermissionsWithPermissionKey(int StateID, string PermissionKey, UserInfo userInfo)
            {
                return CheckPermissionsWithPermissionKey(StateID, PermissionKey, PageWorkflowPermission.PERMISSION_CODE, userInfo);
            }

            private static bool CheckPermissionsWithPermissionKey(int StateID, string PermissionKey, string PermissionCode, UserInfo UserInfo)
            {
                PermissionController permissionController = new PermissionController();
                ArrayList arrPermissions = permissionController.GetPermissionByCodeAndKey(PermissionCode, PermissionKey);
                if (arrPermissions.Count > 0)
                {
                    PermissionInfo pInfo = arrPermissions[0] as PermissionInfo;
                    if (pInfo != null)
                    {
                        return CheckPermissions(StateID, UserInfo);
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }

            private static bool CheckPermissions(int StateID, UserInfo userInfo)
            {
                StringBuilder sb = new StringBuilder();
                List<WorkflowStatePermission> Permissions = GetStatePermissions(StateID);
                foreach (WorkflowStatePermission vPerm in Permissions)
                {
                    string temp = null;
                    string temp2 = null;

                    if (!vPerm.AllowAccess)
                    {
                        temp = "!";
                    }
                    else
                    {
                        temp = "";
                    }

                    if (vPerm.UserID.HasValue && vPerm.UserID.Value > -1)
                    {
                        temp2 = string.Concat(new object[] { temp, "[", vPerm.UserID, "];" });
                    }
                    else if (vPerm.RoleID.HasValue && vPerm.RoleID.Value > -1)
                    {
                        if (PortalSettings.Current != null)
                        {

                            RoleInfo rInfo = new RoleController().GetRoleById(PortalSettings.Current.PortalId, vPerm.RoleID.Value);
                            if (rInfo != null)
                            {
                                temp2 = temp + rInfo.RoleName + ";";
                            }
                        }
                    }
                    else
                    {
                        temp2 = string.Concat(new object[] { temp, "[", -1, "];" });
                    }

                    if (temp == "!")
                    {
                        sb.Insert(0, temp2);
                    }
                    else
                    {
                        sb.Append(temp2);
                    }
                }

                string temp3 = sb.ToString();
                if (!temp3.StartsWith(";"))
                {
                    temp3.Insert(0, ";");
                }

                if (userInfo == null)
                {
                    if (PortalSecurity.IsInRoles("Administrators"))
                    {
                        return true;
                    }
                    else
                    {
                        return PortalSecurity.IsInRoles(temp3);
                    }
                }
                else
                {
                    return IsInRoles(temp3, userInfo);
                }
            }

            private static List<WorkflowStatePermission> GetStatePermissions(int StateID)
            {
                return WorkflowFactory.GetStatePermissionsByStateID(StateID);
            }

            private static bool IsInRoles(string roles, UserInfo currentUserInfo)
            {
                bool isSuperUser = currentUserInfo.IsSuperUser || currentUserInfo.IsInRole("Administrators");
                if (!isSuperUser && (roles != null))
                {
                    HttpContext current = HttpContext.Current;
                    string[] strArray = roles.Split(new char[] { ';' });
                    for (int i = 0; i < strArray.Length; i = i + 1)
                    {
                        string role = strArray[i];
                        if (!string.IsNullOrEmpty(role))
                        {
                            if (role.StartsWith("!"))
                            {
                                PortalSettings currentPortalSettings = PortalController.Instance.GetCurrentSettings() as PortalSettings;
                                if ((currentPortalSettings.PortalId != currentUserInfo.PortalID) || (currentPortalSettings.AdministratorId != currentUserInfo.UserID))
                                {
                                    string str2 = role.Replace("!", "");
                                    if ((!current.Request.IsAuthenticated && (str2 == "Unauthenticated Users")) || ((str2 == "All Users") || currentUserInfo.IsInRole(str2)))
                                    {
                                        return false;
                                    }
                                }
                            }
                            else if ((!current.Request.IsAuthenticated && (role == "Unauthenticated Users")) || ((role == "All Users") || currentUserInfo.IsInRole(role)))
                            {
                                return true;
                            }
                        }
                    }
                }
                return isSuperUser;
            }

            public static List<WorkflowLog> GetEntityWorkflowLogs(string Entity, int EntityID, int Version)
            {
                return WorkflowFactory.GetEntityWorkflowLogs(Entity, EntityID, Version);
            }

            public static int GetNextStateID(int WorkFlowID, int StateID)
            {
                bool FoundState = false;

                foreach (WorkflowState State in GetAllWorkflowStates(WorkFlowID).Where(a => a.IsActive == true).OrderBy(ws => ws.Order))
                {
                    if (FoundState)
                    {
                        return State.StateID;
                    }

                    if (State.StateID == StateID)
                    {
                        FoundState = true;
                    }
                }

                return -1;
            }

            public static int GetPreviousStateID(int WorkFlowID, int StateID)
            {
                bool FoundState = false;

                foreach (WorkflowState State in GetAllWorkflowStates(WorkFlowID).Where(a => a.IsActive == true).OrderByDescending(ws => ws.Order))
                {
                    if (FoundState)
                    {
                        return State.StateID;
                    }

                    if (State.StateID == StateID)
                    {
                        FoundState = true;
                    }
                }

                return -1;
            }



            internal static void SendWorkflowNotification(int PortalID, Pages Page, string Comment, string Type)
            {
                if (Page.StateID.HasValue)
                {
                    TabInfo tabInfo = TabController.Instance.GetTab(Page.TabID, PortalID);
                    string Name = tabInfo.TabName;
                    string URL = tabInfo.FullUrl;
                    List<int> RoleIds = new List<int>();
                    List<string> Emails = new List<string>();
                    string ViewResourceFile = Vanjaro.Common.Utilities.Url.ResolveUrl("~/DesktopModules/Vanjaro/Core/Library/App_LocalResources/Shared.resx");
                    int StateID = Page.StateID.Value;
                    WorkflowState CurrentStateName = GetStateByID(StateID);
                    if (GetWorkflowType(CurrentStateName.WorkflowID) == WorkflowTypes.ContentApproval)
                    {
                        if ((Type.ToLower() == "approve" || Type.ToLower() == "publish" || Type.ToLower() == "reject") && CurrentStateName.Notify)
                        {
                            foreach (WorkflowStatePermission item in GetStatePermissionByStateID(StateID))
                            {
                                if (item.UserID.HasValue && item.UserID > -1)
                                {
                                    UserInfo UInfo = UserController.GetUserById(PortalID, item.UserID.Value);
                                    if (UInfo != null)
                                    {
                                        Emails.Add(UInfo.Email);
                                    }
                                }
                                else if (item.RoleID.HasValue)
                                {
                                    if (!RoleIds.Contains(item.RoleID.Value))
                                    {
                                        RoleController r = new RoleController();
                                        RoleInfo rInfo = r.GetRoleById(PortalID, item.RoleID.Value);

                                        if (rInfo != null)
                                        {
                                            List<UserInfo> ui = RoleController.Instance.GetUsersByRole(PortalID, rInfo.RoleName).ToList();

                                            foreach (UserInfo item1 in ui)
                                            {
                                                if (item1 != null)
                                                {
                                                    Emails.Add(item1.Email);
                                                }
                                            }

                                        }
                                        RoleIds.Add(item.RoleID.Value);
                                    }

                                }
                            }

                            if (Emails.Count == 0)
                            {
                                List<UserInfo> AdminUsers = RoleController.Instance.GetUsersByRole(PortalID, "Administrators").ToList();
                                foreach (UserInfo item1 in AdminUsers)
                                {
                                    if (item1 != null)
                                    {
                                        Emails.Add(item1.Email);
                                    }
                                }
                            }

                            if (IsFirstState(CurrentStateName.WorkflowID, CurrentStateName.StateID) || IsLastState(CurrentStateName.WorkflowID, CurrentStateName.StateID))
                            {
                                UserInfo uInfo = new UserController().GetUser(PortalID, Page.CreatedBy);
                                Emails.Add(uInfo.Email);
                            }
                        }

                        string NotificationSubject = string.Empty, NotificationBody = string.Empty;

                        if (Type.ToLower() == "approve" || Type.ToLower() == "publish")
                        {
                            string OldStateName = GetStateByID(GetPreviousStateID(CurrentStateName.WorkflowID, StateID)).Name;
                            NotificationSubject = ReplaceNotificationsTokens(Name, URL, DNNLocalization.Localization.GetString("ApproveSubject", ViewResourceFile), OldStateName, CurrentStateName.Name, Comment);
                            NotificationBody = ReplaceNotificationsTokens(Name, URL, DNNLocalization.Localization.GetString("ApproveBody", ViewResourceFile), OldStateName, CurrentStateName.Name, Comment);
                        }
                        else if (Type.ToLower() == "reject")
                        {
                            string OldStateName = GetStateByID(GetNextStateID(CurrentStateName.WorkflowID, StateID)).Name;
                            NotificationSubject = ReplaceNotificationsTokens(Name, URL, DNNLocalization.Localization.GetString("RejectSubject", ViewResourceFile), OldStateName, CurrentStateName.Name, Comment);
                            NotificationBody = ReplaceNotificationsTokens(Name, URL, DNNLocalization.Localization.GetString("RejectBody", ViewResourceFile), OldStateName, CurrentStateName.Name, Comment);
                        }

                        string FromEmail = "Vanjaro@dnnuser.com";
                        foreach (string ToEmail in Emails.Distinct())
                        {
                            Vanjaro.Common.Manager.NotificationManager.QueueMail(PortalID, 0, NotificationSubject, NotificationBody, ToEmail, new List<Vanjaro.Common.Data.Entities.Attachment>(), FromEmail, FromEmail);
                        }
                    }
                }
            }

            private static string ReplaceNotificationsTokens(string Name, string URL, string Template, string OldStateName, string NewStateName, string Comment)
            {
                Template = Template.Replace("[Name]", Name);
                Template = Template.Replace("[Link]", URL);
                Template = Template.Replace("[OldStateName]", OldStateName);
                Template = Template.Replace("[NewStateName]", NewStateName);
                Template = Template.Replace("[Comment]", Comment);

                PortalSettings pS = PortalController.Instance.GetCurrentSettings() as PortalSettings;
                if (pS != null)
                {
                    Template = Template.Replace("[DisplayName]", pS.UserInfo.DisplayName);
                }
                else
                {
                    Template = Template.Replace("[DisplayName]", string.Empty);
                }

                return Template;
            }

            public static int GetDefaultWorkflow()
            {
                return GetDefaultWorkflow(0);
            }

            public static int GetDefaultWorkflow(int TabID)
            {
                PortalSettings PS = PortalController.Instance.GetCurrentSettings() as PortalSettings;
                int PortalID = PS != null ? PS.PortalId : 0;

                Setting setting = SettingFactory.GetSetting(PortalID, TabID, "setting_workflow", "WorkflowID");
                if (setting == null)
                {
                    setting = SettingFactory.GetSetting(PortalID, 0, "setting_workflow", "WorkflowID");
                    if (setting != null)
                    {
                        return int.Parse(setting.Value);
                    }
                    else
                    {
                        return 1;//Default Workflow Id inserted in table;
                    }
                }
                else
                {
                    return int.Parse(setting.Value);
                }
            }

            public static List<WorkflowContent> GetPagesbyUserID(int PortalID, int UserID)
            {
                return WorkflowFactory.GetPagesbyUserID(PortalID, UserID);
            }

            public static List<WorkflowContent> GetReviewContentbyUserID(int UserID, int Page, int PageSize, int StateID, string WorkflowReviewType)
            {
                return WorkflowFactory.GetReviewContentbyUserID(UserID, Page, PageSize, StateID, WorkflowReviewType);
            }

            public static int GetReviewCountByUserID(int UserID, int Page, int PageSize, int StateID, string WorkflowReviewType)
            {
                return WorkflowFactory.GetReviewCountByUserID(UserID, Page, PageSize, StateID, WorkflowReviewType);
            }


            public static List<StringValue> GetStatesforReview(int PortalID, int UserID, string ReviewType)
            {
                return WorkflowFactory.GetStatesforReview(PortalID, UserID, ReviewType);
            }

            public static void AddComment(string Entity, int EntityID, string Action, string Comment, PortalSettings PortalSettings)
            {

                string CacheKey = CacheFactory.GetCacheKey(CacheFactory.Keys.Workflow + "AddComment");
                List<Type> ExtensionTypes = CacheFactory.Get(CacheKey);
                if (ExtensionTypes == null)
                {
                    string[] binAssemblies = Directory.GetFiles(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "bin")).Where(c => c.EndsWith(".dll")).ToArray();
                    List<Type> _ExtensionTypes = new List<Type>();
                    foreach (string Path in binAssemblies)
                    {

                        try
                        {

                            IEnumerable<Type> AssembliesToAdd = from t in Assembly.LoadFrom(Path).GetTypes()
                                                                where typeof(IReviewComment).IsAssignableFrom(t) && t.GetConstructor(Type.EmptyTypes) != null
                                                                select t;

                            _ExtensionTypes.AddRange(AssembliesToAdd.ToList());
                        }
                        catch { continue; }
                    }

                    CacheFactory.Set(CacheKey, _ExtensionTypes);
                    ExtensionTypes = _ExtensionTypes;
                }

                foreach (Type t in ExtensionTypes)
                {
                    IReviewComment ReviewContent = Activator.CreateInstance(t) as IReviewComment;

                    try
                    {
                        ReviewContent.AddComment(Entity, EntityID, Action, Comment, PortalSettings);
                    }
                    catch { }
                }

            }
        }
    }
}