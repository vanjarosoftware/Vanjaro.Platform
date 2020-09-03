using DotNetNuke.Security.Permissions;

namespace Vanjaro.Core.Components
{
    public class PageWorkflowPermission
    {
        public const string PERMISSION_CODE = "VANJARO_WORKFLOW_LIBRARY";
        public const string PERMISSION_REVIEWCONTENT = "Review Content";

        public static void InitTabPermissions()
        {
            PermissionController permCtl = new PermissionController();
            #region Permissions Already Exists?
            bool pREVIEWCONTENT_Exists = false;

            foreach (Data.Entities.WorkflowPermissionInfo p in Managers.WorkflowManager.GetPermissionByCode(PERMISSION_CODE))
            {
                if ((p.PermissionKey == PERMISSION_REVIEWCONTENT))
                {
                    pREVIEWCONTENT_Exists = true;
                }
            }
            #endregion

            #region Add Permissions

            try
            {
                if (!pREVIEWCONTENT_Exists)
                {
                    PermissionInfo pi = new PermissionInfo
                    {
                        ModuleDefID = -1,
                        PermissionCode = PERMISSION_CODE,
                        PermissionKey = PERMISSION_REVIEWCONTENT,

                        PermissionName = PERMISSION_REVIEWCONTENT
                    };
                    permCtl.AddPermission(pi);
                }
            }
            catch { }


            #endregion
        }
    }
}