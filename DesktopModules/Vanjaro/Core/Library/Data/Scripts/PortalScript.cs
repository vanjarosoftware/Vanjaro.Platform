using System.Text;
using Vanjaro.Core.Data.PetaPoco;

namespace Vanjaro.Core.Data.Scripts
{
    public class PortalScript
    {
        internal static Sql UpdateContainerSrcContainerHaveValue()
        {
            Sql sb = Sql.Builder.Append("Update tm Set tm.ContainerSrc=t.ContainerSrc from " + CommonScript.DnnTablePrefix + "tabs t join " + CommonScript.DnnTablePrefix + "TabModules tm on(t.TabID = tm.TabID) where t.ContainerSrc is not null and tm.ContainerSrc is null");
            return sb;
        }

        internal static Sql UpdateContainerSrcBothColNull()
        {
            Sql sb = Sql.Builder.Append("Update tm set tm.ContainerSrc=(select top 1 SettingValue from " + CommonScript.DnnTablePrefix + "PortalSettings Where SettingName='DefaultPortalContainer' and PortalID=t.PortalID) from " + CommonScript.DnnTablePrefix + "tabmodules tm join " + CommonScript.DnnTablePrefix + "tabs t on(t.tabid = tm.TabID) where tm.ContainerSrc is null and t.ContainerSrc is null");
            return sb;
        }

        internal static Sql UpdateSkinSrc()
        {
            Sql sb = Sql.Builder.Append("update " + CommonScript.DnnTablePrefix + "PortalSettings set SettingValue='[G]Containers/Vanjaro/Base.ascx' where SettingName='DefaultAdminContainer' ");
            sb.Append("update " + CommonScript.DnnTablePrefix + "PortalSettings set SettingValue='[G]Skins/Vanjaro/Base.ascx' where SettingName='DefaultAdminSkin' ");
            sb.Append("update " + CommonScript.DnnTablePrefix + "PortalSettings set SettingValue='[G]Containers/Vanjaro/Base.ascx' where SettingName='DefaultPortalContainer' ");
            sb.Append("update " + CommonScript.DnnTablePrefix + "PortalSettings set SettingValue='[G]Skins/Vanjaro/Base.ascx' where SettingName='DefaultPortalSkin' ");
            return sb;
        }

        internal static Sql UpdateTabSkinSrc(string SkinSrc, int PortalID)
        {
            Sql sb = Sql.Builder.Append("update " + CommonScript.DnnTablePrefix + "Tabs set SkinSrc = @0 where SkinSrc is null and PortalID=@1", SkinSrc, PortalID);
            return sb;
        }

        internal static string UpdatePortalSettings(string SettingName, string SettingValue, int PortalID, int UserID)
        {
            StringBuilder st = new StringBuilder();
            st.Append("IF (NOT EXISTS(SELECT * FROM " + CommonScript.DnnTablePrefix + "PortalSettings where SettingName='" + SettingName + "' and PortalID=" + PortalID + ")) ");
            st.Append("BEGIN  INSERT Into  " + CommonScript.DnnTablePrefix + "PortalSettings (PortalID,  SettingName,  SettingValue, CultureCode,CreatedByUserID, CreatedOnDate, LastModifiedByUserID, LastModifiedOnDate)");
            st.Append("VALUES (" + PortalID + ", '" + SettingName + "', '" + SettingValue + "', NULLIF('', N''), IsNull(" + UserID + ", -1),GetDate(),IsNull(" + UserID + ", -1),GetDate());");
            st.Append("END ELSE BEGIN ");
            st.Append("UPDATE " + CommonScript.DnnTablePrefix + "PortalSettings SET [SettingValue] = '" + SettingValue + "', [LastModifiedByUserID] = IsNull(" + UserID + ", -1), [LastModifiedOnDate] = GetDate() Where SettingName='" + SettingName + "'  and PortalID=" + PortalID + " END;");
            return st.ToString();
        }


        internal static Sql UpdateTabContainerSrc(int PortalID, int TabID)
        {
            Sql sb = Sql.Builder.Append("Update " + CommonScript.DnnTablePrefix + "TabModules set ContainerSrc = @0, DisplayTitle = @1 where TabID =@2", "[g]containers/vanjaro/base.ascx", true, TabID);
            return sb;
        }

    }
}