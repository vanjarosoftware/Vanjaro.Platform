using Vanjaro.Core.Data.PetaPoco;

namespace Vanjaro.Core.Data.Scripts
{
    public class PageScript
    {
        internal static Sql GetAllPublishedPages(int PortalID, string Locale)
        {
            if (string.IsNullOrEmpty(Locale))
            {
                Sql sb = Sql.Builder.Append("select * from " + CommonScript.TablePrefix + "VJ_Core_Pages pages join(");
                sb.Append("select TabID,MAX(Version) Version from " + CommonScript.TablePrefix + "VJ_Core_Pages where PortalID=@0 and IsPublished=1 and Locale is null ", PortalID);
                sb.Append("group by TabID) j on pages.TabID=j.TabID and pages.Version=j.Version ");
                sb.Append("where pages.PortalID=@0 and pages.IsPublished=1 and pages.Locale is null", PortalID);
                return sb;
            }
            else
            {
                Sql sb = Sql.Builder.Append("select * from " + CommonScript.TablePrefix + "VJ_Core_Pages pages join(");
                sb.Append("select TabID,MAX(Version) Version from " + CommonScript.TablePrefix + "VJ_Core_Pages where PortalID=@0 and IsPublished=1 and Locale=@1 ", PortalID, Locale);
                sb.Append("group by TabID) j on pages.TabID=j.TabID and pages.Version=j.Version ");
                sb.Append("where pages.PortalID=@0 and pages.IsPublished=1 and pages.Locale=@1", PortalID, Locale);
                return sb;
            }
        }
    }
}