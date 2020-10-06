using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using DotNetNuke.Security.Roles;
using DotNetNuke.Services.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using Vanjaro.Common.Permissions;
using Vanjaro.Core.Components;
using Vanjaro.Core.Data.Entities;
using Vanjaro.Core.Data.PetaPoco;
using Vanjaro.Core.Data.Scripts;

namespace Vanjaro.Core
{
    public static partial class Factories
    {
        internal class WorkflowFactory
        {
            internal static List<Workflow> GetAll(int PortalID, bool IncludeDeleted)
            {
                string CacheKey = CacheFactory.GetCacheKey(CacheFactory.Keys.Workflow + "GetAll", PortalID, IncludeDeleted);
                List<Workflow> Workflows = CacheFactory.Get(CacheKey) as List<Workflow>;
                if (Workflows == null)
                {
                    if (IncludeDeleted)
                    {
                        Workflows = Workflow.Query("where PortalID is null or PortalID=@0", PortalID).ToList();
                    }
                    else
                    {
                        Workflows = Workflow.Query("where (PortalID is null or PortalID=@0) and IsDeleted=@1", PortalID, IncludeDeleted).ToList();
                    }

                    CacheFactory.Set(CacheKey, Workflows);
                }
                return Workflows;
            }

            internal static List<Workflow> GetAll(int PortalID)
            {
                return Workflow.Query("where PortalID=@0", PortalID).ToList();
            }


            internal static Workflow GetWorkflowbyID(int WorkflowId)
            {
                string CacheKey = CacheFactory.GetCacheKey(CacheFactory.Keys.Workflow + "GetWorkflowbyID", WorkflowId);
                Workflow Workflow = CacheFactory.Get(CacheKey);
                if (Workflow == null)
                {
                    Workflow = Workflow.Query("where ID=@0", WorkflowId).FirstOrDefault();
                    CacheFactory.Set(CacheKey, Workflow);
                }
                return Workflow;
            }

            internal static List<WorkflowState> GetAllStatesbyWorkflowID(int WorkflowId)
            {
                string CacheKey = CacheFactory.GetCacheKey(CacheFactory.Keys.Workflow + "GetAllStatesbyWorkflowID", WorkflowId);
                List<WorkflowState> WorkflowStates = CacheFactory.Get(CacheKey) as List<WorkflowState>;
                if (WorkflowStates == null)
                {
                    WorkflowStates = WorkflowState.Query("where WorkflowId=@0", WorkflowId).ToList();
                    CacheFactory.Set(CacheKey, WorkflowStates);
                }
                return WorkflowStates;

            }

            internal static WorkflowState GetStateByID(int ID)
            {
                string CacheKey = CacheFactory.GetCacheKey(CacheFactory.Keys.Workflow + "GetStateByID", ID);
                WorkflowState WorkflowState = CacheFactory.Get(CacheKey);
                if (WorkflowState == null)
                {
                    WorkflowState = WorkflowState.Query("where StateID=@0", ID).FirstOrDefault();
                    CacheFactory.Set(CacheKey, WorkflowState);
                }
                return WorkflowState;
            }

            internal static List<WorkflowStatePermission> GetStatePermissionsByStateID(int ID)
            {
                string CacheKey = CacheFactory.GetCacheKey(CacheFactory.Keys.Workflow + "GetStatePermissionsByStateID", ID);
                List<WorkflowStatePermission> _WorkflowStatePerm = CacheFactory.Get(CacheKey) as List<WorkflowStatePermission>;
                if (_WorkflowStatePerm == null)
                {
                    _WorkflowStatePerm = WorkflowStatePermission.Query("where StateID=@0", ID).ToList();
                    CacheFactory.Set(CacheKey, _WorkflowStatePerm);
                }
                return _WorkflowStatePerm;
            }

            internal static List<WorkflowPermission> GetWorkflowPermissionsByID(int ID)
            {
                string CacheKey = CacheFactory.GetCacheKey(CacheFactory.Keys.Workflow + "GetWorkflowPermissionsByID", ID);
                List<WorkflowPermission> _WorkflowPerm = CacheFactory.Get(CacheKey) as List<WorkflowPermission>;
                if (_WorkflowPerm == null)
                {
                    _WorkflowPerm = WorkflowPermission.Query("where WorkflowID=@0", ID).ToList();
                    CacheFactory.Set(CacheKey, _WorkflowPerm);
                }
                return _WorkflowPerm;
            }


            internal static List<WorkflowPermissionInfo> GetPermissionByCode(string Code)
            {
                string CacheKey = CacheFactory.GetCacheKey(CacheFactory.Keys.Workflow + "GetPermissionByCode", Code);
                List<WorkflowPermissionInfo> _WorkflowStatePermInfo = CacheFactory.Get(CacheKey) as List<WorkflowPermissionInfo>;
                if (_WorkflowStatePermInfo == null)
                {
                    using (VanjaroRepo db = new VanjaroRepo())
                    {
                        _WorkflowStatePermInfo = db.Query<WorkflowPermissionInfo>("SELECT p.* FROM " + Data.Scripts.CommonScript.DnnTablePrefix + "Permission AS p WHERE p.PermissionCode = @0", Code).ToList();
                    }
                    CacheFactory.Set(CacheKey, _WorkflowStatePermInfo);
                }
                return _WorkflowStatePermInfo;

            }

            internal static Workflow UpdateWorkflow(Workflow workflow)
            {
                Workflow wflow = new Workflow();
                if (workflow.ID == 0)
                {
                    wflow.Description = workflow.Description;
                    wflow.IsDeleted = workflow.IsDeleted;
                    wflow.Name = workflow.Name;
                    wflow.CreatedBy = workflow.CreatedBy;
                    wflow.CreatedOn = workflow.CreatedOn;
                    wflow.PortalID = workflow.PortalID;
                    wflow.Revisions = 10;
                    wflow.Insert();
                    UpdateWorkflowState(new WorkflowState { WorkflowID = wflow.ID, IsActive = true, Name = "Draft", Notify = true, Order = 1 });
                    WorkflowState wState = UpdateWorkflowState(new WorkflowState { WorkflowID = wflow.ID, IsActive = true, Name = "Ready for Review", Notify = true, Order = 2 });
                    if (wState != null)
                    {
                        DNNModulePermissionInfo DnnPermissionInfo = Vanjaro.Common.Manager.PermissionManager.GetPermissionInfo(PageWorkflowPermission.PERMISSION_CODE).Where(p => p.PermissionKey == PageWorkflowPermission.PERMISSION_REVIEWCONTENT).FirstOrDefault();
                        WorkflowStatePermission statePerm = new WorkflowStatePermission
                        {
                            StateID = wState.StateID,
                            PermissionID = DnnPermissionInfo.PermissionID,
                            AllowAccess = true
                        };
                        int PortalID = workflow.PortalID.HasValue ? workflow.PortalID.Value : PortalSettings.Current.PortalId;
                        statePerm.RoleID = RoleController.Instance.GetRoleByName(PortalID, "Administrators").RoleID;
                        statePerm.UserID = null;
                        statePerm.CreatedBy = workflow.CreatedBy;
                        statePerm.CreatedOn = workflow.CreatedOn;
                        statePerm.UpdatedBy = workflow.CreatedBy;
                        statePerm.UpdatedOn = workflow.CreatedOn;
                        statePerm.Insert();
                    }
                    UpdateWorkflowState(new WorkflowState { WorkflowID = wflow.ID, IsActive = true, Name = "Published", Notify = true, Order = 3 });

                }
                else
                {
                    wflow = GetWorkflowbyID(workflow.ID);
                    if (wflow != null)
                    {
                        wflow.Description = workflow.Description;
                        wflow.IsDeleted = workflow.IsDeleted;
                        wflow.Name = workflow.Name;
                        wflow.CreatedBy = wflow.CreatedBy;
                        wflow.CreatedOn = wflow.CreatedOn;
                        wflow.DeletedBy = workflow.DeletedBy;
                        wflow.DeletedOn = workflow.DeletedOn;
                        wflow.Revisions = workflow.Revisions;
                        wflow.PortalID = wflow.ID == 1 ? null : workflow.PortalID;
                        wflow.Update();
                    }
                }
                CacheFactory.Clear(CacheFactory.Keys.Workflow);
                return wflow;
            }

            internal static WorkflowState UpdateWorkflowState(WorkflowState WorkflowState)
            {
                if (WorkflowState.StateID == 0)
                {
                    WorkflowState.Insert();
                }
                else
                {
                    WorkflowState.Update();
                }

                CacheFactory.Clear(CacheFactory.Keys.Workflow);
                return WorkflowState;
            }

            internal static void ClearAllWorkflowStatePermissionsByStateID(int StateID)
            {
                foreach (WorkflowStatePermission permission in GetStatePermissionsByStateID(StateID))
                {
                    permission.Delete();
                }
                CacheFactory.Clear(CacheFactory.Keys.Workflow);
            }

            internal static void UpdateWorkflowStatePermissions(List<WorkflowStatePermission> Permissions)
            {
                foreach (WorkflowStatePermission permission in Permissions)
                {
                    permission.Insert();
                }
                CacheFactory.Clear(CacheFactory.Keys.Workflow);
            }

            internal static void ClearAllWorkflowPermissionsID(int workflowID)
            {
                foreach (WorkflowPermission permission in GetWorkflowPermissionsByID(workflowID))
                {
                    permission.Delete();
                }
                CacheFactory.Clear(CacheFactory.Keys.Workflow);
            }

            internal static void DeleteWorkflowState(int WorkflowStateId, int WorkflowId)
            {
                try
                {
                    WorkflowState wState = GetAllStatesbyWorkflowID(WorkflowId).Where(a => a.StateID == WorkflowStateId).FirstOrDefault();
                    if (wState != null && !wState.IsFirst && !wState.IsLast)
                    {
                        ClearAllWorkflowStatePermissionsByStateID(wState.StateID);
                        wState.Delete();
                        List<WorkflowState> wStates = GetAllStatesbyWorkflowID(WorkflowId).OrderBy(o => o.Order).ToList();
                        if (wStates.Count >= 2)
                        {
                            Workflow wflow = GetWorkflowbyID(WorkflowId);
                            UpdateSatesOrder(wStates, wflow, false, wState.Order);
                            CacheFactory.Clear(CacheFactory.Keys.Workflow);
                        }
                    }
                }
                catch (Exception ex) { Exceptions.LogException(ex); }
            }

            internal static int UpdateSatesOrder(List<WorkflowState> wStates, Workflow wflow, bool IncrementOrder, int? DeletedWorkflowStateOrder)
            {
                int Order = -1;
                if (IncrementOrder)
                {
                    WorkflowState wState = wStates.LastOrDefault();
                    Order = wState.Order;
                    wState.Order++;
                    wState.Update();
                    CacheFactory.Clear(CacheFactory.Keys.Workflow);
                    return Order;
                }
                else
                {
                    if (DeletedWorkflowStateOrder.HasValue)
                    {
                        wStates = wStates.Where(ws => ws.Order > DeletedWorkflowStateOrder).ToList();
                    }

                    foreach (WorkflowState wState in wStates)
                    {
                        wState.Order--;
                        wState.Update();
                    }
                    CacheFactory.Clear(CacheFactory.Keys.Workflow);
                    return DeletedWorkflowStateOrder.Value;
                }
            }

            internal static void DeleteWorkflow(Workflow wflow)
            {
                foreach (WorkflowState state in GetAllStatesbyWorkflowID(wflow.ID))
                {
                    WorkflowLog.Delete("Where StateID=@0", state.StateID);
                    WorkflowStatePermission.Delete("Where StateID=@0", state.StateID);
                    state.Delete();
                }

                WorkflowPermission.Delete("Where Workflowid=@0", wflow.ID);
                wflow.Delete();

                CacheFactory.Clear(CacheFactory.Keys.Workflow);
            }

            internal static List<WorkflowLog> GetPagesWorkflowLogs(int TabID, int Version)
            {
                string CacheKey = CacheFactory.GetCacheKey(CacheFactory.Keys.Workflow + "GetPagesWorkflowLogs", TabID, Version);
                List<WorkflowLog> WorkflowLogs = CacheFactory.Get(CacheKey) as List<WorkflowLog>;
                if (WorkflowLogs == null)
                {
                    WorkflowLogs = WorkflowLog.Query("Where Tabid=@0 and Version=@1", TabID, Version).ToList();
                    CacheFactory.Set(CacheKey, WorkflowLogs);
                }
                return WorkflowLogs;
            }

            internal static void AddWorkflowLog(int PortalID, int ModuleID, int UserID, Pages Page, string Action, string Comment)
            {
                if (!string.IsNullOrEmpty(Comment))
                {
                    WorkflowLog wlog = new WorkflowLog
                    {
                        PortalID = PortalID,
                        ModuleID = ModuleID,
                        ReviewedBy = UserID,
                        ReviewedOn = DateTime.UtcNow,
                        StateID = Page.StateID.Value,
                        Comment = Comment,
                        TabID = Page.TabID,
                        Version = Page.Version
                    };
                    if (Action.ToLower() == "approve" || Action.ToLower() == "publish")
                    {
                        wlog.Approved = true;
                    }
                    else if (Action.ToLower() == "reject")
                    {
                        wlog.Approved = false;
                    }

                    wlog.Insert();
                    CacheFactory.Clear(CacheFactory.Keys.Workflow);
                }
            }

            internal static bool HasReviewPermission(UserInfo userInfo)
            {
                return HasReviewPermission(userInfo, null);
            }

            internal static bool HasReviewPermission(UserInfo userInfo, List<int> States)
            {

                if (userInfo.IsSuperUser || userInfo.IsInRole("Administrators"))
                {
                    return true;
                }

                int PortalID = PortalSettings.Current.PortalId;
                List<int> RolesID = new List<int>();
                List<RoleInfo> Roles = RoleController.Instance.GetRoles(PortalID).Cast<RoleInfo>().ToList();
                foreach (string Role in userInfo.Roles)
                {
                    RolesID.Add(Roles.Where(a => a.RoleName == Role).Select(a => a.RoleID).FirstOrDefault());
                }

                if (RolesID.Count > 0 && States == null)
                {
                    return WorkflowStatePermission.Query("where (RoleID in (" + string.Join(",", RolesID) + ") or UserID =@0) and AllowAccess=@1", userInfo.UserID, true).ToList().Count > 0;
                }

                if (RolesID.Count > 0 && States != null)
                {
                    return WorkflowStatePermission.Query("where (RoleID in (" + string.Join(",", RolesID) + ") or UserID =@0) and AllowAccess=@1 and StateID in (" + string.Join(",", States) + ") ", userInfo.UserID, true).Select(a => a.StateID).Distinct().ToList().Count > 0;
                }

                return false;
            }

            internal static List<WorkflowPage> GetPagesbyUserID(int PortalID, int UserID)
            {
                string CacheKey = CacheFactory.GetCacheKey(CacheFactory.Keys.Workflow + "GetPagesbyUserID" + "PortalID", UserID, PortalID);
                List<WorkflowPage> WorkflowPages = CacheFactory.Get(CacheKey) as List<WorkflowPage>;
                if (WorkflowPages == null)
                {
                    Sql Query = WorkflowScript.GetPagesByUserID(PortalID, UserID);
                    using (VanjaroRepo db = new VanjaroRepo())
                    {
                        WorkflowPages = db.Fetch<WorkflowPage>(Query).ToList();
                    }

                    CacheFactory.Set(CacheKey, WorkflowPages);
                }
                return WorkflowPages;
            }

            internal static List<WorkflowPage> GetReviewPagesbyUserID(int UserID, int Page, int PageSize, int StateID)
            {
                string CacheKey = CacheFactory.GetCacheKey(CacheFactory.Keys.Workflow + "GetReviewPagesbyUserID", UserID, Page, PageSize, StateID);
                List<WorkflowPage> Pages = CacheFactory.Get(CacheKey) as List<WorkflowPage>;
                if (Pages == null)
                {
                    Sql Query = WorkflowScript.GetPagesByUserID(UserID, Page, PageSize, StateID);
                    using (VanjaroRepo db = new VanjaroRepo())
                    {
                        Pages = db.Fetch<WorkflowPage>(Query).ToList();
                    }
                    CacheFactory.Set(CacheKey, Pages);
                }
                return Pages;
            }

            internal static int GetReviewPagesCountByUserID(int UserID, int Page, int PageSize, int StateID)
            {
                string CacheKey = CacheFactory.GetCacheKey(CacheFactory.Keys.Workflow + "GetReviewPagesCountByUserID", UserID, Page, PageSize, StateID);
                int? Count = CacheFactory.Get(CacheKey);
                if (Count == null)
                {
                    Count = 0;
                    Sql Query = WorkflowScript.GetPagesCountByUserID(UserID, StateID);
                    using (VanjaroRepo db = new VanjaroRepo())
                    {
                        Count = db.Fetch<int>(Query).FirstOrDefault();
                    }
                    CacheFactory.Set(CacheKey, Count);
                }
                return Count.Value;

            }

            internal static void UpdateWorkflowPermissions(List<WorkflowPermission> Permissions)
            {
                foreach (WorkflowPermission permission in Permissions)
                {
                    permission.Insert();
                }
                CacheFactory.Clear(CacheFactory.Keys.Workflow);
            }

            internal static List<StringValue> GetStatesforReview(int PortalID, int UserID)
            {
                string CacheKey = CacheFactory.GetCacheKey(CacheFactory.Keys.Workflow + "GetStatesforReview" + "PortalID", UserID, PortalID);
                List<StringValue> result = CacheFactory.Get(CacheKey) as List<StringValue>;
                if (result == null)
                {
                    Sql Query = WorkflowScript.GetStatesforPendingReview(PortalID, UserID);
                    using (VanjaroRepo db = new VanjaroRepo())
                    {
                        result = db.Fetch<StringValue>(Query).ToList();
                    }
                    CacheFactory.Set(CacheKey, result);
                }
                return result;
            }
        }
    }
}