using DotNetNuke.Entities.Users;
using Vanjaro.Common.PetaPoco;

namespace Vanjaro.Common.Data.Scripts
{
    public static class BrowseUploadScript
    {
        public static Sql IsImageFolder(int FolderID)
        {
            Sql sb = Sql.Builder.Append("IF((select COUNT(*) from " + CommonScript.DnnTablePrefix + "files where folderid=" + FolderID + ") > 0)");
            sb.Append(" BEGIN");
            sb.Append(" SELECT CASE");
            sb.Append(" WHEN ((SELECT cast(COUNT(*) as float) FROM " + CommonScript.DnnTablePrefix + "files WHERE folderid=" + FolderID + " and ContentType like 'image%') / (SELECT cast(COUNT(*) as float) FROM " + CommonScript.DnnTablePrefix + "files WHERE folderid=" + FolderID + ") > 0.5) OR ((SELECT cast(COUNT(*) as float) FROM " + CommonScript.DnnTablePrefix + "files WHERE folderid=" + FolderID + " and LOWER(Extension) in('jpg','jpeg','gif','png','svg','webp')) / (SELECT cast(COUNT(*) as float) FROM " + CommonScript.DnnTablePrefix + "files WHERE folderid=" + FolderID + ") > 0.5) THEN 1");
            sb.Append(" ELSE 0");
            sb.Append(" END AS HasManyImages");
            sb.Append(" END");
            sb.Append(" ELSE select 0");
            return sb;
        }

        public static Sql GetFolders(int PortalID, UserInfo UserInfo, int ParentFolderID, string UserRoleIDs)
        {
            if (UserInfo.IsSuperUser && PortalID < 0)
            {
                Sql sb = Sql.Builder.Append("SELECT A.FolderID,A.FolderPath,A.StorageLocation,A.FolderMappingID,B.ChildCount FROM(");
                sb.Append("SELECT FolderID, StorageLocation,FolderPath,FolderMappingID FROM (");
                sb.Append(" select f.FolderID,f.StorageLocation, FolderPath,f.FolderMappingID from " + CommonScript.DnnTablePrefix + "folders f");
                sb.Append(" where ParentID =" + ParentFolderID);
                sb.Append(" AND FolderPath!='Templates/' AND FolderPath!='Users/' AND FolderPath!='vThemes/' AND FolderPath!='Containers/' AND FolderPath!='Skins/' AND FolderPath Not Like '%.versions/%'");
                sb.Append(" AND PortalID is null) RootFolders");
                sb.Append(" GROUP BY FolderID, StorageLocation, FolderPath,FolderMappingID) A");
                sb.Append(" left Join");
                sb.Append(" (SELECT ParentID, Count(*) as ChildCount from (");
                sb.Append(" select ParentID,FolderID  FROM (");
                sb.Append(" select f.FolderID,f.ParentID,f.FolderMappingID, FolderPath from " + CommonScript.DnnTablePrefix + "folders f");
                sb.Append(" where PortalID is null AND FolderPath Not Like '%.versions/%') ChildCountDer");
                sb.Append(" GROUP By ParentID, FolderID) ChildCountTable");
                sb.Append(" Group by ParentID) B");
                sb.Append(" ON A.FolderID = B.ParentID");
                sb.Append(" Order By FolderPath");
                return sb;
            }
            else
            {
                if (string.IsNullOrEmpty(UserRoleIDs))
                {
                    UserRoleIDs = "-3";
                }

                Sql sb = Sql.Builder.Append("SELECT A.FolderID,A.FolderPath,A.StorageLocation,A.FolderMappingID,B.ChildCount FROM(");
                sb.Append(" SELECT FolderID, StorageLocation,FolderPath,FolderMappingID FROM (");
                sb.Append(" select f.FolderID, f.StorageLocation, FolderPath,f.FolderMappingID from " + CommonScript.DnnTablePrefix + "folders f join " + CommonScript.DnnTablePrefix + "folderpermission fp");
                sb.Append(" on (f.FolderID = fp.FolderID)");
                sb.Append(" where ((AllowAccess = 1");
                sb.Append(" AND (RoleID = -1 OR RoleID IN (" + UserRoleIDs + ") OR UserID = " + UserInfo.UserID + ")) OR (select issuperuser from users where userid=" + UserInfo.UserID + ")=1)");
                sb.Append(" AND ParentID = " + ParentFolderID + "");
                sb.Append(" AND FolderPath!='Templates/' AND FolderPath!='Users/' AND FolderPath!='vThemes/' AND FolderPath!='Containers/' AND FolderPath!='Skins/' AND FolderPath Not Like '%.versions/%'");
                string guid = string.Empty;
                if (!string.IsNullOrEmpty(guid))
                    sb.Append(" AND FolderPath!='" + guid + "/Templates/' AND FolderPath!='" + guid + "/Users/' AND FolderPath!='" + guid + "/vThemes/' AND FolderPath!='" + guid + "/Containers/' AND FolderPath!='" + guid + "/Skins/'");
                sb.Append(" AND PortalID = " + PortalID + ") RootFolders");
                sb.Append(" GROUP BY FolderID, StorageLocation, FolderPath,FolderMappingID) A");
                sb.Append(" left Join");
                sb.Append(" (SELECT ParentID, Count(*) as ChildCount from (");
                sb.Append(" select ParentID,FolderID  FROM (");
                sb.Append(" select f.FolderID,f.ParentID,f.FolderMappingID, FolderPath,fp.AllowAccess,fp.RoleID,fp.UserID from " + CommonScript.DnnTablePrefix + "folders f join " + CommonScript.DnnTablePrefix + "folderpermission fp");
                sb.Append(" on (f.FolderID = fp.FolderID)");
                sb.Append(" where ((AllowAccess = 1");
                sb.Append(" AND (RoleID = -1 OR RoleID IN (" + UserRoleIDs + ") OR UserID = " + UserInfo.UserID + ")) OR (select issuperuser from users where userid=" + UserInfo.UserID + ")=1)");
                sb.Append(" AND FolderPath Not Like '%.versions/%' AND PortalID = " + PortalID + " ) ChildCountDer");
                sb.Append(" GROUP By ParentID, FolderID) ChildCountTable");
                sb.Append(" Group by ParentID) B");
                sb.Append(" ON A.FolderID = B.ParentID");
                sb.Append(" Order By FolderPath");
                return sb;
            }
        }
    }
}