using DotNetNuke.Entities.Content.Workflow.Entities;
using Vanjaro.Core.Components;
using Vanjaro.Core.Data.PetaPoco;

namespace Vanjaro.Core.Data.Scripts
{
    public class WorkflowScript
    {
        internal static Sql GetPagesByUserID(int PortalID, int UserID)
        {
            Sql sb = Sql.Builder.Append("SELECT p.ID,p.TabID as EntityID,p.Version,p.StateID,p.IsPublished,t.TabName as EntityName,t.Title,t.Description");
            sb.Append("FROM  " + CommonScript.TablePrefix + "vj_core_pages p JOIN (SELECT tabid, Max(version) AS Version FROM   " + CommonScript.TablePrefix + "vj_core_pages WHERE  ispublished = 0 GROUP  BY tabid) p_LatestVersion ON ( p.tabid = p_LatestVersion.tabid AND p.version = p_LatestVersion.version ) ");
            sb.Append("LEFT JOIN " + CommonScript.DnnTablePrefix + "tabs t ON p.tabid = t.tabid ");
            sb.Append("LEFT JOIN (SELECT stateid FROM   " + CommonScript.TablePrefix + "vj_core_workflowstatepermission wsp ");
            sb.Append("LEFT JOIN (SELECT roleid  FROM   " + CommonScript.DnnTablePrefix + "userroles WHERE  userid = @0 and ", UserID);
            sb.Append("(EffectiveDate >= Convert(datetime,  GETDATE()) or EffectiveDate is null) and");
            sb.Append("(ExpiryDate >= Convert(datetime,  GETDATE()) or ExpiryDate is null)) r ON r.roleid = wsp.roleid WHERE  wsp.allowaccess = 1 AND ( r.roleid IS NOT NULL OR wsp.userid = @0 ) ", UserID);
            sb.Append("GROUP  BY stateid) wsp ON ( wsp.stateid = p.stateid ) WHERE  wsp.stateid IS NOT NULL and t.TabName is not null and p.Locale is null AND p.PortalID = @0", PortalID);
            return sb;
        }

        internal static Sql GetReviewContentByUserID(int UserID, int Page, int PageSize, int StateID, string WorkflowReviewType)
        {
            Sql sb = new Sql();
            if (WorkflowReviewType.ToLower() == Enum.WorkflowLogType.VJPage.ToString().ToLower())
            {
                sb.Append("select * from (SELECT ROW_NUMBER() OVER (ORDER BY p.ID desc) AS Row_Number, p.ID,p.TabID as EntityID,p.Version,p.StateID,p.IsPublished,'VJPage' as EntityName,t.Title,t.Description,ps.Name as State,w.Name as Workflow");
                sb.Append("FROM  " + CommonScript.TablePrefix + "vj_core_pages p JOIN (SELECT tabid, Max(version) AS Version FROM   " + CommonScript.TablePrefix + "vj_core_pages WHERE  ispublished = 0 GROUP  BY tabid) p_LatestVersion ON ( p.tabid = p_LatestVersion.tabid AND p.version = p_LatestVersion.version ) ");
                sb.Append("LEFT JOIN " + CommonScript.DnnTablePrefix + "tabs t ON p.tabid = t.tabid ");
                sb.Append("LEFT JOIN " + CommonScript.TablePrefix + "VJ_Core_WorkflowState as ps on ps.StateID=p.StateID");
                sb.Append("LEFT JOIN " + CommonScript.TablePrefix + "VJ_Core_Workflow as w on w.ID=ps.WorkflowID");
                sb.Append("LEFT JOIN (SELECT stateid FROM   " + CommonScript.TablePrefix + "vj_core_workflowstatepermission wsp ");
                sb.Append("LEFT JOIN (SELECT roleid  FROM   " + CommonScript.DnnTablePrefix + "userroles WHERE  userid = @0 and ", UserID);
                sb.Append("(EffectiveDate >= Convert(datetime,  GETDATE()) or EffectiveDate is null) and");
                sb.Append("(ExpiryDate >= Convert(datetime,  GETDATE()) or ExpiryDate is null)) r ON r.roleid = wsp.roleid WHERE  wsp.allowaccess = 1 AND ( r.roleid IS NOT NULL OR wsp.userid = @0 ) ", UserID);
                sb.Append("GROUP  BY stateid) wsp ON ( wsp.stateid = p.stateid ) WHERE  wsp.stateid IS NOT NULL and t.TabName is not null and p.Locale is null");
            }
            if (StateID > 0)
            {
                sb.Append("and wsp.StateID=@0", StateID);
            }

            sb.Append(") as Pages Where Pages.[Row_Number] BETWEEN((@0 - 1) * @1 + 1) AND(@0 * @1)", Page, PageSize);
            return sb;
        }

        internal static Sql GetReviewCountByUserID(int UserID, int StateID, string WorkflowReviewType)
        {
            Sql sb = new Sql();
            if (WorkflowReviewType.ToLower() == Enum.WorkflowLogType.VJPage.ToString().ToLower())
            {
                sb.Append("SELECT count(*) as Count");
                sb.Append("FROM  " + CommonScript.TablePrefix + "vj_core_pages p JOIN (SELECT tabid, Max(version) AS Version FROM   " + CommonScript.TablePrefix + "vj_core_pages WHERE  ispublished = 0 GROUP  BY tabid) p_LatestVersion ON ( p.tabid = p_LatestVersion.tabid AND p.version = p_LatestVersion.version ) ");
                sb.Append("LEFT JOIN " + CommonScript.DnnTablePrefix + "tabs t ON p.tabid = t.tabid ");
                sb.Append("LEFT JOIN " + CommonScript.TablePrefix + "VJ_Core_WorkflowState as ps on ps.StateID=p.StateID");
                sb.Append("LEFT JOIN (SELECT stateid FROM   " + CommonScript.TablePrefix + "vj_core_workflowstatepermission wsp ");
                sb.Append("LEFT JOIN (SELECT roleid  FROM   " + CommonScript.DnnTablePrefix + "userroles WHERE  userid = @0 and ", UserID);
                sb.Append("(EffectiveDate >= Convert(datetime,  GETDATE()) or EffectiveDate is null) and");
                sb.Append("(ExpiryDate >= Convert(datetime,  GETDATE()) or ExpiryDate is null)) r ON r.roleid = wsp.roleid WHERE  wsp.allowaccess = 1 AND ( r.roleid IS NOT NULL OR wsp.userid = @0 ) ", UserID);
                sb.Append("GROUP  BY stateid) wsp ON ( wsp.stateid = p.stateid ) WHERE  wsp.stateid IS NOT NULL and t.TabName is not null");
            }
            if (StateID > 0)
            {
                sb.Append("and wsp.StateID=@0", StateID);
            }

            return sb;
        }

        internal static Sql GetStatesforPendingReview(int PortalID, int UserID, string ReviewType)
        {
            Sql sb = new Sql();
            if (ReviewType == Enum.WorkflowLogType.VJPage.ToString().ToLower())
            {
                sb.Append("select * from (SELECT p.StateID as Value,( w.Name + ' > '+  ps.Name )as Text");
                sb.Append("FROM  " + CommonScript.TablePrefix + "vj_core_pages p JOIN (SELECT tabid, Max(version) AS Version FROM   " + CommonScript.TablePrefix + "vj_core_pages WHERE  ispublished = 0 GROUP  BY tabid) p_LatestVersion ON ( p.tabid = p_LatestVersion.tabid AND p.version = p_LatestVersion.version ) ");
                sb.Append("LEFT JOIN " + CommonScript.DnnTablePrefix + "tabs t ON p.tabid = t.tabid");
                sb.Append("left join " + CommonScript.TablePrefix + "VJ_Core_WorkflowState as ps on ps.StateID=p.StateID");
                sb.Append("left join " + CommonScript.TablePrefix + "VJ_Core_Workflow as w on w.ID=ps.WorkflowID");
                sb.Append("LEFT JOIN (SELECT stateid FROM   " + CommonScript.TablePrefix + "vj_core_workflowstatepermission wsp ");
                sb.Append("LEFT JOIN (SELECT roleid  FROM   " + CommonScript.DnnTablePrefix + "userroles WHERE  userid = @0 and ", UserID);
                sb.Append("(EffectiveDate >= Convert(datetime,  GETDATE()) or EffectiveDate is null) and");
                sb.Append("(ExpiryDate >= Convert(datetime,  GETDATE()) or ExpiryDate is null)) r ON r.roleid = wsp.roleid WHERE  wsp.allowaccess = 1 AND ( r.roleid IS NOT NULL OR wsp.userid = @0 ) ", UserID);
                sb.Append("GROUP  BY stateid) wsp ON ( wsp.stateid = p.stateid ) WHERE  wsp.stateid IS NOT NULL and t.TabName is not null AND p.PortalID = @0", PortalID);
                sb.Append(") as Pages group by Value,Text");
            }

            return sb;
        }
    }
}