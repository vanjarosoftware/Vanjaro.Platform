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

        internal static string GetPublishPage(string Locale)
        {
            string Query = "Select top 1 [ID], [PortalID], [TabID], [Content], [Style], [Version], [CreatedBy], [CreatedOn], [UpdatedBy], [UpdatedOn], [IsPublished], [PublishedBy], [PublishedOn], [Locale], [StateID] FROM " + CommonScript.TablePrefix + "VJ_Core_Pages Where ";
            Query += "TabID =@0 and IsPublished=@2 ";
            Query += string.IsNullOrEmpty(Locale) ? "and Locale is null " : "and Locale=@1";
            Query += " order by Version desc";
            return Query;
        }
    }
}