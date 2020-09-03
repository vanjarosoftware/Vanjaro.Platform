using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using DotNetNuke.Common;
using System.Xml;
using System.IO;

namespace Vanjaro.UXManager.Extensions.Menu.Roles.Components
{
    public class Utilities
    {
      
        public static bool IsHttpModuleInstalled()
        {
            HttpModuleCollection modules = HttpContext.Current.ApplicationInstance.Modules;

            if (modules["LiveUtilitiesModule"] != null)
                return true;
            else
                return false;
        }
        public static void InstallHttpModule()
        {
            UpdateWebConfig(true);
        }
        public static void UninstallHttpModule()
        {
            UpdateWebConfig(false);
        }

        private static void UpdateWebConfig(bool InstallModule)
        {
            bool SaveChanges = false;

            string WebConfigPath = Globals.ApplicationMapPath + "\\web.config";
            XmlDocument WebConfig = new XmlDocument();
            FileStream fS = new FileStream(WebConfigPath, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite);
            WebConfig.Load(fS);
            //WebConfig = DotNetNuke.Common.Utilities.Config.Load();// gets the web.config file as xmldocument use this in case of permision issue (access denied).
            XmlNode HttpModules = WebConfig.SelectSingleNode("//configuration/system.web/httpModules");

            if (HttpModules != null) //Pre IIS 7 Config
            {
                XmlNode LiveUtilitiesModule = HttpModules.SelectSingleNode("add[@name='LiveUtilitiesModule']");

                if (InstallModule)
                {
                    if (LiveUtilitiesModule == null)
                    {
                        LiveUtilitiesModule = WebConfig.CreateElement("add");
                        HttpModules.AppendChild(LiveUtilitiesModule);

                        XmlAttribute Name = WebConfig.CreateAttribute("name");
                        Name.Value = "LiveUtilitiesModule";
                        LiveUtilitiesModule.Attributes.Append(Name);

                        XmlAttribute Type = WebConfig.CreateAttribute("type");
                        Type.Value = "Mandeeps.DNN.Modules.LiveUtilities.Components.HttpModule,Mandeeps.DNN.Modules.LiveUtilities";
                        LiveUtilitiesModule.Attributes.Append(Type);

                        SaveChanges = true;
                    }
                }
                else if (LiveUtilitiesModule != null)
                {
                    HttpModules.RemoveChild(LiveUtilitiesModule);
                    SaveChanges = true;
                }

            }

            HttpModules = WebConfig.SelectSingleNode("//configuration/system.webServer/modules");

            if (HttpModules != null) //Post IIS 7 Config
            {
                XmlNode LiveUtilitiesModule = HttpModules.SelectSingleNode("add[@name='LiveUtilitiesModule']");

                if (InstallModule)
                {
                    if (LiveUtilitiesModule == null)
                    {
                        LiveUtilitiesModule = WebConfig.CreateElement("add");
                        HttpModules.AppendChild(LiveUtilitiesModule);

                        XmlAttribute Name = WebConfig.CreateAttribute("name");
                        Name.Value = "LiveUtilitiesModule";
                        LiveUtilitiesModule.Attributes.Append(Name);

                        XmlAttribute Type = WebConfig.CreateAttribute("type");
                        Type.Value = "Mandeeps.DNN.Modules.LiveUtilities.Components.HttpModule,Mandeeps.DNN.Modules.LiveUtilities";
                        LiveUtilitiesModule.Attributes.Append(Type);

                        XmlAttribute PreCondition = WebConfig.CreateAttribute("preCondition");
                        PreCondition.Value = "managedHandler";
                        LiveUtilitiesModule.Attributes.Append(PreCondition);

                        SaveChanges = true;
                    }
                }
                else if (LiveUtilitiesModule != null)
                {
                    HttpModules.RemoveChild(LiveUtilitiesModule);
                    SaveChanges = true;
                }

            }
            
            if (SaveChanges)
            {
                //DotNetNuke.Common.Utilities.Config.Save(WebConfig);//save the changes to web.config file
                fS.Seek(0L, SeekOrigin.Begin);
                fS.SetLength(0L);
                WebConfig.Save(fS);
            }

            fS.Close();
            fS.Dispose();
        }
    }
}