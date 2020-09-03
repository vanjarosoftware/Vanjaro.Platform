using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Portals;
using HtmlAgilityPack;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using Vanjaro.Core.Data.Entities;
using Vanjaro.Core.Data.PetaPoco;
using DNNLocalization = DotNetNuke.Services.Localization;

namespace Vanjaro.Skin
{
    public static partial class Managers
    {
        public class MigrateManager
        {
            internal static string GetHTMLText(ModuleInfo m)
            {
                string HtmlContent = string.Empty;
                Sql sql = new Sql();
                sql.Append("SELECT TOP 1 * FROM " + Core.Data.Scripts.CommonScript.TablePrefix + "HtmlText WHERE ModuleID=@0 ORDER BY [Version] DESC", m.ModuleID);
                using (VanjaroRepo db = new VanjaroRepo())
                {
                    dynamic data = db.Fetch<dynamic>(sql).FirstOrDefault();
                    if (data != null && data.Content != null)
                    {
                        HtmlContent = data.Content;
                    }
                }
                return HtmlContent;

            }

            #region Migrate Page
            internal static string MigrateInjectBlocks(PortalSettings PortalSettings, HtmlDocument html)
            {
                IEnumerable<HtmlNode> query = html.DocumentNode.Descendants("div");
                List<string> DataPanes = new List<string>();
                foreach (HtmlNode item in query.ToList())
                {
                    foreach (ModuleInfo m in PortalSettings.ActiveTab.Modules)
                    {
                        if (item.Attributes.Where(a => a.Name == "data-pane").FirstOrDefault() != null && !string.IsNullOrEmpty(item.Attributes.Where(a => a.Name == "data-pane").FirstOrDefault().Value) && item.Attributes.Where(a => a.Name == "data-pane").FirstOrDefault().Value.ToLower() == m.PaneName.ToLower())
                        {
                            if (m.DesktopModule.ModuleName.ToUpper() == "DNN_HTML")
                            {
                                item.InnerHtml += "<div data-m2v=\"\" data-appname=\"" + m.ModuleTitle + "\">" + HttpUtility.HtmlDecode(MigrateManager.GetHTMLText(m)) + "</div>";
                            }
                            else
                            {
                                item.InnerHtml += "<div data-m2v=\"\" data-appname=\"" + m.ModuleTitle + "\" dmid=\"" + m.DesktopModuleID + "\" mid=\"" + m.ModuleID + "\" uid=\"0\"><div vjmod=\"true\"><app id=\"" + m.ModuleID + "\"></app></div></div>";
                            }

                            DataPanes.AddRange(item.Attributes.Where(a => a.Name == "data-pane").Select(s => s.Value.ToLower()).ToList());
                        }
                    }
                }


                string Outhtml = html.DocumentNode.OuterHtml;

                foreach (ModuleInfo m in PortalSettings.ActiveTab.Modules)
                {
                    if (!DataPanes.Contains(m.PaneName.ToLower()))
                    {
                        if (m.DesktopModule.ModuleName.ToUpper() == "DNN_HTML")
                        {
                            Outhtml += "<div data-m2v=\"\" data-appname=\"" + m.ModuleTitle + "\">" + HttpUtility.HtmlDecode(MigrateManager.GetHTMLText(m)) + "</div>";
                        }
                        else
                        {
                            Outhtml += "<div data-m2v=\"\" data-appname=\"" + m.ModuleTitle + "\" dmid=\"" + m.DesktopModuleID + "\" mid=\"" + m.ModuleID + "\" uid=\"0\"><div vjmod=\"true\"><app id=\"" + m.ModuleID + "\"></app></div></div>";
                        }
                    }
                }
                return Outhtml;
            }

            internal static string GetMigratePageToastMarkup(System.Web.UI.Page Page)
            {
                StringBuilder Markup = new StringBuilder();
                string LocalResourcesFile = Page.ResolveUrl("~/DesktopModules/Vanjaro/UXManager/Library/App_LocalResources/Shared.resx");
                string MigrateOverridePageYesBtn = DNNLocalization.Localization.GetString("MigrateOverridePageYesBtn", LocalResourcesFile) ?? "Yes";
                string MigratePublishBtn = DNNLocalization.Localization.GetString("MigratePublishBtn", LocalResourcesFile);
                //string MigrateOverridePageNoBtn = DNNLocalization.Localization.GetString("MigrateOverridePageNoBtn", LocalResourcesFile) ?? "No";
                string MigrateToastTitle = DNNLocalization.Localization.GetString("MigrateToastTitle", LocalResourcesFile) ?? "Migrate Page";

                Markup.Append("var MigratePageMarkup=$('<div>');  MigratePageMarkup.addClass('MigratedPage-Confirmbox');");

                Markup.Append("var PublishBtn = $('<button>'); PublishBtn.text('" + MigratePublishBtn + "');");
                Markup.Append("PublishBtn.addClass('btn btn-success pubish-btn btn-sm');");

                Markup.Append("var OverridePageYesBtn = $('<button>'); OverridePageYesBtn.text('" + MigrateOverridePageYesBtn + "');");
                Markup.Append("OverridePageYesBtn.addClass(' reimportbtn btn btn-danger btn-sm') ; OverridePageYesBtn.click(function(){");
                Markup.Append("swal({ title: '" + DNNLocalization.Localization.GetString("ReImportContent_Title", LocalResourcesFile) + "',text: '" + DNNLocalization.Localization.GetString("ReImportContent_Text", LocalResourcesFile) + "',type: 'warning', showCancelButton: true, confirmButtonColor: '#DD6B55', confirmButtonText: '" + DNNLocalization.Localization.GetString("ReImportContent_Button", LocalResourcesFile) + "',cancelButtonText: '" + DNNLocalization.Localization.GetString("ReImportContent_CancelButton", LocalResourcesFile) + "',closeOnConfirm: true,closeOnCancel: true},function(isConfirm) { if(isConfirm){");
                Markup.Append(" var sf = $.ServicesFramework(-1);");
                Markup.Append("$.ajax({ type: \"GET\",url:$.ServicesFramework(-1).getServiceRoot(\"Vanjaro\") + \"Page/Delete\" + \"?m2v=true\",");
                Markup.Append("headers: {'ModuleId': parseInt(sf.getModuleId()),'TabId': parseInt(sf.getTabId()),'RequestVerificationToken': sf.getAntiForgeryValue()},");
                Markup.Append("success: function(response) {window.location.href=window.location.href.replace('m2v=true','m2v=false').replace('m2v/true','m2v/false')}});");
                Markup.Append("}});");
                Markup.Append("});");

                //Markup.Append("var OverridePageNoBtn = $('<button>'); OverridePageNoBtn.text('" + MigrateOverridePageNoBtn + "');");
                //Markup.Append("OverridePageNoBtn.click(function(){$('.toast-close-button').click();});");
                //Markup.Append("MigratePageMarkup.append(OverridePageYesBtn); MigratePageMarkup.append(OverridePageNoBtn);");
                Markup.Append("MigratePageMarkup.append(PublishBtn);");
                Markup.Append("MigratePageMarkup.append(OverridePageYesBtn);");
                Markup.Append("window.parent.ShowNotification('" + MigrateToastTitle + "',MigratePageMarkup, 'info', '', false,false);");
                Markup.Append("$('.MigratedPage-Confirmbox').parents('.toast-info').addClass('migrate-toast')");

                return Markup.ToString();
            }

            internal static string GetMigratePageCSS(Page page)
            {
                StringBuilder Markup = new StringBuilder();
                Markup.Append("#toast-container .toastr.migrate-toast{width: 260px;padding: 15px 15px 15px 30px;} #toast-container .toastr.migrate-toast:before {content: none;} .toast-message .pubish-btn, .toast-message .reimportbtn{width: 90px !important;}");
                return Markup.ToString();
            }


            //internal static bool IsPublished(int TabID)
            //{
            //    return PageManager.GetPages(TabID).Where(x => x.IsPublished).Count() > 0;
            //}
            #endregion
        }
    }
}