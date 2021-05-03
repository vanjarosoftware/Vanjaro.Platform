using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using DotNetNuke.Web.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using Vanjaro.Common.ASPNET.WebAPI;
using Vanjaro.Common.Engines.UIEngine;
using Vanjaro.Core;
using Vanjaro.Core.Data.Entities;
using Vanjaro.UXManager.Extensions.Menu.Workflow.Factories;
using Vanjaro.UXManager.Library.Common;
using static Vanjaro.UXManager.Extensions.Menu.Workflow.Factories.AppFactory;

namespace Vanjaro.UXManager.Extensions.Menu.Workflow.Controllers
{
    [ValidateAntiForgeryToken]
    [AuthorizeAccessRoles(AccessRoles = "admin")]
    public class WorkflowController : UIEngineController
    {
        internal static List<IUIData> GetData(UserInfo UserInfo, PortalSettings PortalSetting, string Identifier)
        {
            Dictionary<string, IUIData> Settings = new Dictionary<string, IUIData>();
            string WorkFlowId = Managers.SettingManager.GetValue(PortalSetting.PortalId, 0, AppFactory.Identifier.setting_workflow.ToString(), "WorkflowID", GetViews());
            int WorkflowId = string.IsNullOrEmpty(WorkFlowId) ? 0 : int.Parse(WorkFlowId);
            string ResourceFilePath = AppFactory.SharedResourceFile();
            Settings.Add("ddlWorkFlows", new UIData { Name = "ddlWorkFlows", Options = Managers.WorkflowManager.GetDDLWorkflow(PortalSetting.PortalId, false), OptionsText = "Text", OptionsValue = "Value", Value = WorkflowId.ToString() });
            Settings.Add("MaxRevisions", new UIData { Name = "MaxRevisions", Value = Managers.WorkflowManager.GetMaxRevisions().ToString() });
            Settings.Add("IsAdmin", new UIData { Name = "IsAdmin", Options = UserInfo.IsInRole("Administrators") || UserInfo.IsSuperUser });
            Settings.Add("WorkflowStateInfo", new UIData { Name = "WorkflowStateInfo", Value = Managers.WorkflowManager.GetWorkflowStatesInfo(WorkflowId) });
            Settings.Add("WorkflowId", new UIData { Name = "WorkflowId", Value = WorkflowId.ToString() });
            Core.Data.Entities.Workflow workflow = Managers.WorkflowManager.GetWorkflow(WorkflowId);
            List<WorkflowState> WorkflowState = Managers.WorkflowManager.GetWorkflowStates(WorkflowId);
            Settings.Add("Workflows", new UIData { Name = "Workflows", Options = Managers.WorkflowManager.GetAll(PortalSetting.PortalId, true) });
            Settings.Add("Workflow", new UIData { Name = "Workflow", Options = workflow });
            Settings.Add("WorkflowScope", new UIData { Name = "WorkflowScope", Options = Managers.WorkflowManager.GetAllWorkflowScope(ResourceFilePath), OptionsText = "Text", OptionsValue = "Value" });
            Settings.Add("WorkflowStates", new UIData { Name = "WorkflowStates", Options = WorkflowState });
            Settings.Add("workflowPermission", new UIData { Name = "workflowPermission", Options = Managers.WorkflowManager.GetPermission() });

            Settings.Add("Permissions", new UIData { Name = "Permissions", Options = Managers.WorkflowManager.GetWorkflowPermission(0, PortalSetting.PortalId) });
            return Settings.Values.ToList();
        }

        [HttpPost]
        public void UpdateAdvance(dynamic Data)
        {
            Managers.SettingManager.UpdateValue(PortalSettings.PortalId, 0, Identifier.setting_workflow.ToString(), "MaxRevisions", Data.MaxRevisions.Value.ToString());
        }

        [HttpPost]
        public void UpdateDefault(dynamic Data)
        {
            Core.Data.Entities.Workflow workflow = Managers.WorkflowManager.GetWorkflow(int.Parse(Data.WorkflowID.Value));
            if (workflow != null)
            {
                Managers.SettingManager.UpdateValue(PortalSettings.PortalId, 0, Identifier.setting_workflow.ToString(), "MaxRevisions", workflow.Revisions.ToString());
                Managers.SettingManager.UpdateValue(PortalSettings.PortalId, 0, Identifier.setting_workflow.ToString(), "WorkflowID", workflow.ID.ToString());
            }
        }

        [HttpGet]
        public ActionResult GetWorkflow(int WorkflowID)
        {
            ActionResult ActionResult = new ActionResult();
            Dictionary<string, dynamic> data = new Dictionary<string, dynamic>();
            Core.Data.Entities.Workflow workflow = Managers.WorkflowManager.GetWorkflow(WorkflowID);
            if (workflow == null)
            {
                workflow = new Core.Data.Entities.Workflow();
            }

            data.Add("Workflow", workflow);
            data.Add("WorkflowStates", Managers.WorkflowManager.GetWorkflowStates(WorkflowID));
            data.Add("workflowPermission", Managers.WorkflowManager.GetWorkflowPermission(WorkflowID, PortalSettings.PortalId));
            ActionResult.Data = data;
            return ActionResult;
        }

        [HttpPost]
        public ActionResult Add(dynamic Data)
        {
            Core.Data.Entities.Workflow Workflow = new Core.Data.Entities.Workflow
            {
                ID = int.Parse(Data.Workflow.ID.ToString()),
                Name = Data.Workflow.Name.ToString(),
                Description = Data.Workflow.Description.ToString(),
                IsDeleted = Convert.ToBoolean(Data.Workflow.IsDeleted.ToString())
            };
            Workflow.IsDeleted = Convert.ToBoolean(Data.Workflow.IsDeleted.ToString());
            Workflow.Revisions = int.Parse(Data.Workflow.Revisions.ToString());
            ActionResult ActionResult = new ActionResult();
            Dictionary<string, dynamic> data = new Dictionary<string, dynamic>();
            Workflow = Managers.WorkflowManager.Update(Workflow.ID, Workflow.Name, Workflow.Description, Workflow.Revisions, Workflow.IsDeleted, UserInfo.UserID, PortalSettings.PortalId);
            if (Workflow.PortalID.HasValue)
            {
                Managers.WorkflowManager.UpdateWorkflowPermissions(Data);
                data.Add("WorkflowID", Workflow.ID);
                data.Add("Workflows", Managers.WorkflowManager.GetAll(PortalSettings.PortalId, true));
                data.Add("WorkflowStates", Managers.WorkflowManager.GetWorkflowStates(Workflow.ID));
                data.Add("Workflow", Managers.WorkflowManager.GetWorkflow(Workflow.ID));
                data.Add("ddlWorkFlows", Managers.WorkflowManager.GetDDLWorkflow(PortalSettings.PortalId, false));
            }
            ActionResult.Data = data;
            return ActionResult;
        }

        [HttpPost]
        public ActionResult Delete(Core.Data.Entities.Workflow Workflow)
        {
            ActionResult ActionResult = new ActionResult();
            Dictionary<string, dynamic> data = new Dictionary<string, dynamic>
            {
                { "IsDeleted", Managers.WorkflowManager.Delete(Workflow.ID, Workflow.IsDeleted, UserInfo.UserID) },
                { "Workflows", Managers.WorkflowManager.GetAll(PortalSettings.PortalId, true) },
                { "ddlWorkFlows", Managers.WorkflowManager.GetDDLWorkflow(PortalSettings.PortalId, false) }
            };
            ActionResult.Data = data;
            return ActionResult;

        }

        [HttpPost]
        public ActionResult UpdateState(int WorkflowID, dynamic Data)
        {
            ActionResult ActionResult = new ActionResult();
            Dictionary<string, dynamic> data = new Dictionary<string, dynamic>();
            List<WorkflowState> WorkflowState = Managers.WorkflowManager.UpdateState(WorkflowID, Data);
            data.Add("WorkflowStates", WorkflowState);
            data.Add("Workflow", Managers.WorkflowManager.GetWorkflow(WorkflowID));
            ActionResult.Data = data;
            return ActionResult;
        }

        [HttpGet]
        public ActionResult Delete(int StateID)
        {
            ActionResult ActionResult = new ActionResult();
            Dictionary<string, dynamic> data = new Dictionary<string, dynamic>();
            WorkflowState wState = Managers.WorkflowManager.GetStateByID(StateID);
            if (wState != null && (Managers.WorkflowManager.IsWorkflowStateDeleted(wState.WorkflowID, wState)))
            {
                Managers.WorkflowManager.DeleteWorkflowState(wState.StateID, wState.WorkflowID);
                data.Add("IsDeleted", true);
            }
            else
            {
                data.Add("IsDeleted", false);
            }

            ActionResult.Data = data;
            return ActionResult;
        }

        [HttpGet]
        public ActionResult StatePermission(int StateId)
        {
            ActionResult ActionResult = new ActionResult
            {
                Data = Managers.WorkflowManager.GetStatePermission(StateId)
            };
            return ActionResult;
        }

        public override string AccessRoles()
        {
            return Factories.AppFactory.GetAccessRoles(UserInfo);
        }
    }
}